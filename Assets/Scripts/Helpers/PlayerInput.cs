using UnityEngine;

namespace Helpers
{
    public class PlayerInput
    {
        private byte _bitmap;

        public PlayerInput(byte bitmap)
        {
            _bitmap = bitmap;
        }

        public void AddKey(KeyCode keyCode)
        {
            // TODO: add constants for input numbers.
            switch (keyCode)
            {
                case KeyCode.UpArrow: _bitmap |= 1;
                    break;
                case KeyCode.DownArrow: _bitmap |= 1 << 1;
                    break;
                case KeyCode.LeftArrow: _bitmap |= 1 << 2;
                    break;
                case KeyCode.RightArrow: _bitmap |= 1 << 3;
                    break;
            }
        }

        public bool GetKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.UpArrow: 
                    return (_bitmap & 1) > 0;
                case KeyCode.DownArrow: 
                    return (_bitmap & (1 << 1)) > 0;
                case KeyCode.LeftArrow: 
                    return (_bitmap & (1 << 2)) > 0;
                case KeyCode.RightArrow: 
                    return (_bitmap & (1 << 3)) > 0;
                default: 
                    return false;
            }
        }
        
        public static PlayerInput GetPlayerInput()
        {
            PlayerInput playerInput = new PlayerInput(0);
            if (Input.GetKey(KeyCode.UpArrow))
                playerInput.AddKey(KeyCode.UpArrow);
            if (Input.GetKey(KeyCode.DownArrow))
                playerInput.AddKey(KeyCode.DownArrow);
            if (Input.GetKey(KeyCode.RightArrow))
                playerInput.AddKey(KeyCode.RightArrow);
            if (Input.GetKey(KeyCode.LeftArrow))
                playerInput.AddKey(KeyCode.LeftArrow);
            return playerInput;
        }

        public byte Bitmap => _bitmap;
    }
}
