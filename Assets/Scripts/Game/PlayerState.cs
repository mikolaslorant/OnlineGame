using System;
using UnityEngine;

namespace Game
{
    public class PlayerState
    {
        public PlayerState(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; set; }
    }
}
