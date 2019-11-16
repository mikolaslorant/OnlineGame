using System;
using System.Collections.Generic;
using System.Numerics;
using Game;
using Helpers;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Network.ClientTools
{
    public class ClientSidePredictor
    {
        private const float MAX_PREDICTION_ERROR = 0.2f;
        private CharacterController _characterController;
        private List<PlayerInput> _playerInputs;
        private float _speed;
        
        public ClientSidePredictor(CharacterController characterController, float speed)
        {
            _characterController = characterController;
            _playerInputs = new List<PlayerInput>();
            _speed = speed;
        }

        public void CorrectPlayerState(PlayerState playerState, int tick)
        {
            RemoveAppliedPlayerTicks(tick);

            var predictedPosition = _characterController.transform.position;

            _characterController.Move(playerState.Position - predictedPosition);
            foreach (var playerInput in _playerInputs)
            {
                _characterController.Move(PlayerInput.GetMovement(playerInput, _characterController) * this._speed);
            }
            var predictionError = (predictedPosition -
                                   _characterController.transform.position);
            // If prediction does not diverge maintain same position.
            if (!HasDiverged(predictionError))
            {
                _characterController.Move(predictionError);
            }
        }

        public void UpdatePlayerState(PlayerInput playerInput)
        {
            _playerInputs.Add(playerInput);
            _characterController.Move(PlayerInput.GetMovement(playerInput, _characterController) * _speed);
            _characterController.transform.Rotate(new Vector3(playerInput.MouseYAxis, playerInput.MouseXAxis, 0) * _speed);
        }
        
        private void RemoveAppliedPlayerTicks(int tick)
        {
            while (_playerInputs.Count > 0 && _playerInputs[0].Tick <= tick)
            {
                _playerInputs.RemoveAt(0);
            }
        }

        private bool HasDiverged(Vector3 predictionError) => Math.Abs(predictionError.x) >= MAX_PREDICTION_ERROR ||
                                   Math.Abs(predictionError.y) >= MAX_PREDICTION_ERROR ||
                                   Math.Abs(predictionError.z) >= MAX_PREDICTION_ERROR;
    }
}
