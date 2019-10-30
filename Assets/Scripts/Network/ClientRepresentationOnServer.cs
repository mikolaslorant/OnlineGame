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

        public void UpdateClientRepresentationOnServer(Vector3 newControllerPosition, Vector3 newControllerRotation)
        {
            _characterController.Move(newControllerPosition);
            _characterController.transform.Rotate(newControllerRotation);
            _playerState = new PlayerState(_characterController.transform.position);
        }

        
        public PlayerState PlayerState => _playerState;
        public CharacterController CharacterController => _characterController;
    }
}
