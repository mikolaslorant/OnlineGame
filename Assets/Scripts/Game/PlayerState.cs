using System;
using UnityEngine;

namespace Game
{
    public class PlayerState
    {
        public PlayerState(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }
}
