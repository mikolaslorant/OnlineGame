using System.Runtime.CompilerServices;
using UnityEngine;
using Game;

namespace Network
{
    public class ClientRepresentation
    {
        private readonly GameObject _playerPrefab;
        // To move client on server
        private readonly CharacterController _characterController;
        // To build worldstate
        private readonly PlayerState _playerState;
        // To keep track of client inputs
        private int _tick;

        public int Tick
        {
            get => _tick;
            set => _tick = value;
        }

        public ClientRepresentation(GameObject playerPrefab, CharacterController characterController, PlayerState playerState, int tick)
        {
            _playerPrefab = playerPrefab;
            _characterController = characterController;
            _playerState = playerState;
            _tick = tick;
        }

        public void UpdateClientRepresentation(Vector3 newControllerMovement, Vector3 newControllerRotation)
        {
            _characterController.Move(newControllerMovement);
            _characterController.transform.Rotate(newControllerRotation);
            _playerState.Position = _characterController.transform.position;
            _playerState.Rotation = _characterController.transform.rotation;
        }

        public PlayerState PlayerState => _playerState;
        public CharacterController CharacterController => _characterController;
    }
}
