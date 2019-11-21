using UnityEngine;

namespace Helpers
{
    public class PlayerInput
    {
        private byte _bitmap;
        private int _tick;
        private float _mouseXAxis;
        private float _mouseYAxis;

        public PlayerInput(byte bitmap, float mouseXAxis, float mouseYAxis, int tick)
        {
            _bitmap = bitmap;
            _mouseXAxis = mouseXAxis;
            _mouseYAxis = mouseYAxis;
            _tick = tick;
        }

        public void AddKey(KeyCode keyCode)
        {
            // TODO: add constants for input numbers.
            switch (keyCode)
            {
                case KeyCode.W: _bitmap |= 1;
                    break;
                case KeyCode.S: _bitmap |= 1 << 1;
                    break;
                case KeyCode.A: _bitmap |= 1 << 2;
                    break;
                case KeyCode.D: _bitmap |= 1 << 3;
                    break;
                case KeyCode.Space: _bitmap |= 1 << 4;
                    break;
                case KeyCode.LeftShift: _bitmap |= 1 << 5;
                    break;
                case KeyCode.Mouse0: _bitmap |= 1 << 6;
                    break;
                case KeyCode.Escape: _bitmap |= 1 << 7;
                    break;
            }
        }

        public bool GetKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.W: 
                    return (_bitmap & 1) > 0;
                case KeyCode.S: 
                    return (_bitmap & (1 << 1)) > 0;
                case KeyCode.A: 
                    return (_bitmap & (1 << 2)) > 0;
                case KeyCode.D: 
                    return (_bitmap & (1 << 3)) > 0;
                case KeyCode.Space:
                    return (_bitmap & (1 << 4)) > 0;
                case KeyCode.LeftShift:
                    return (_bitmap & (1 << 5)) > 0;
                case KeyCode.Mouse0:
                    return (_bitmap & (1 << 6)) > 0;
                case KeyCode.Escape:
                    return (_bitmap & (1 << 7)) > 0;
                default: 
                    return false;
            }
        }
        
        public static PlayerInput GetPlayerInput(int tick)
        {
            PlayerInput playerInput = new PlayerInput(0, 0, 0, tick);

            if (Input.GetKey(KeyCode.W))
                playerInput.AddKey(KeyCode.W);
            if (Input.GetKey(KeyCode.S))
                playerInput.AddKey(KeyCode.S);
            if (Input.GetKey(KeyCode.D))
                playerInput.AddKey(KeyCode.D);
            if (Input.GetKey(KeyCode.A))
                playerInput.AddKey(KeyCode.A);
            if (Input.GetKey(KeyCode.Space))
                playerInput.AddKey(KeyCode.Space);
            if (Input.GetKey(KeyCode.LeftShift))
                playerInput.AddKey(KeyCode.LeftShift);
            if (Input.GetKey(KeyCode.Mouse0))
                playerInput.AddKey(KeyCode.Mouse0);
            if (Input.GetKey(KeyCode.Escape))
                playerInput.AddKey(KeyCode.Escape);
            playerInput._mouseXAxis = Input.GetAxis("Mouse X");
            playerInput._mouseYAxis = Input.GetAxis("Mouse Y");

            return playerInput;
        }

        //Here we use the character controller to calculate the movement relative to the controller orientation
        public static Vector3 GetMovement(PlayerInput playerInput, CharacterController characterController)
        {
            var totalMovement = new Vector3();
            if (playerInput.GetKeyDown(KeyCode.W))
                totalMovement += characterController.transform.forward;
            if (playerInput.GetKeyDown(KeyCode.S))
                totalMovement -= characterController.transform.forward;
            if (playerInput.GetKeyDown(KeyCode.D))
                totalMovement += characterController.transform.right;
            if (playerInput.GetKeyDown(KeyCode.A))
                totalMovement -= characterController.transform.right;
            if (playerInput.GetKeyDown(KeyCode.Space))
                totalMovement += characterController.transform.up;
            if (playerInput.GetKeyDown(KeyCode.LeftShift))
                totalMovement -= characterController.transform.up;

            return totalMovement;
        }

        public byte Bitmap => _bitmap;
        public int Tick => _tick;
        public float MouseXAxis => _mouseXAxis;
        public float MouseYAxis => _mouseYAxis;
    }
}
