using System;
using UnityEngine;

namespace Game
{
    public class PlayerState
    {
        private readonly Vector3 _position;

        public PlayerState(Vector3 position)
        {
            _position = position;
        }

        public Vector3 Position => _position;
    }
}
