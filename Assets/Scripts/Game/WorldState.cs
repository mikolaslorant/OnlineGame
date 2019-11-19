using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Game
{
    public class WorldState
    {
        private readonly IDictionary<int, PlayerState> _players;
        private int _tick;

        public WorldState()
        {
            _players = new Dictionary<int, PlayerState>();
        }

        public IDictionary<int, PlayerState> Players => _players;
    }
}