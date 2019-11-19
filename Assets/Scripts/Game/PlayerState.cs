using System;
using UnityEngine;

namespace Game
{
    public class PlayerState
    {
        public PlayerState(Vector3 position, Quaternion rotation, int health)
        {
            Position = position;
            Rotation = rotation;
            Health = health;
        }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public int Health { get; set; }
    }
}
