using System;
using System.Collections.Generic;
using Game;
using Helpers;
using UnityEngine;

namespace Network.ClientTools
{
    public class ClientSidePredictor
    {
        private const float MAX_PREDICTION_ERROR = 0.1f;
        private CharacterController _predictorDummy;
        private CharacterController _characterController;
        private List<PlayerInput> _playerInputs;
        private float _speed;
        
        public ClientSidePredictor(CharacterController characterController, float speed)
        {
            var gameObject = new GameObject();
            _predictorDummy = gameObject.AddComponent<CharacterController>();
            _characterController = characterController;
            _playerInputs = new List<PlayerInput>();
            _speed = speed;
        }

        public void CorrectPlayerState(PlayerState playerState, int tick)
        {
            RemoveAppliedPlayerTicks(tick);
            _predictorDummy.transform.position = playerState.Position;
            foreach (var playerInput in _playerInputs)
            {
                var movement = PlayerInput.GetMovement(playerInput);
                var movementAfterSpeed = movement * _speed;
                //_predictorDummy.Move(movementAfterSpeed);
                _predictorDummy.transform.position += movementAfterSpeed;
            }
            var predictionError = (_predictorDummy.transform.position -
                                   _characterController.transform.position);
            // If prediction does not diverge maintain same position.
            if (HasDiverged(predictionError))
            {
                _characterController.transform.position = _predictorDummy.transform.position;
            }
        }

        public void UpdatePlayerState(PlayerInput playerInput)
        {
            _playerInputs.Add(playerInput);
            _characterController.Move(PlayerInput.GetMovement(playerInput) * _speed);
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
