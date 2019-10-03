using System;
using UnityEngine;

namespace Game
{
    public class PlayerState
    {
        private readonly Vector3 _position;
        private readonly long _sequenceNumber;

        public PlayerState(Vector3 position, long sequenceNumber)
        {
            _position = position;
            _sequenceNumber = sequenceNumber;
        }

        public Vector3 Position => _position;
    }
}
