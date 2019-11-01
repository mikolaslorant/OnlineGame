using System.Runtime.CompilerServices;
using UnityEngine;
using Game;

namespace Network
{
    public class ClientRepresentationOnServer
    {
        // To move client on server
        private CharacterController _characterController;
        // To build worldstate
        private PlayerState _playerState;
        // To keep track of client inputs
        private int _tick;

        public int Tick
        {
            get => _tick;
            set => _tick = value;
        }

        public ClientRepresentationOnServer(CharacterController characterController, PlayerState playerState, int tick)
        {
            _characterController = characterController;
            _playerState = playerState;
            _tick = tick;
        }

        public void UpdateClientRepresentationOnServer(Vector3 newControllerMovement, Vector3 newControllerRotation)
        {
            _characterController.Move(newControllerMovement);
            _characterController.transform.Rotate(newControllerRotation);
            _playerState.Position = _characterController.transform.position;
        }

        
        public PlayerState PlayerState => _playerState;
        public CharacterController CharacterController => _characterController;
    }
}
