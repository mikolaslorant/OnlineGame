﻿using System.Collections.Generic;
using Game;
using Network.Enums;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Network.ClientTools
{
    public class InterpolationBuffer
    {
        private const int CurrentSnapshot = 0;
        private const int NextSnapshot = 1;
        private readonly int _minimumSize;
        private readonly IList<SnapshotMessage> _buffer;
        private readonly CharacterController _characterController;

        public InterpolationBuffer()
        {
            this._minimumSize = 3;
            this._buffer = new List<SnapshotMessage>();
            this.SynchronizeState = ClientSynchronizeState.Unsynchronized;
        }
        
        public ClientSynchronizeState SynchronizeState { get; set; }

        public void Add(SnapshotMessage snapshotMessage)
        {
            _buffer.Add(snapshotMessage);
            if (_buffer.Count > _minimumSize && SynchronizeState == ClientSynchronizeState.Buffering)
                SynchronizeState = ClientSynchronizeState.Synchronized;
        }

        // Interpolate remote players (their representation on the client) and return the current playerstate for the main local player.
        public WorldState Poll(int clientId, float clientTime)
        {
            if (SynchronizeState != ClientSynchronizeState.Synchronized)
                return null;
            if (_buffer.Count <= 1)
            {
                SynchronizeState = ClientSynchronizeState.Unsynchronized;
                return null;
            }
            if (clientTime >= _buffer[NextSnapshot].TimeStamp)
                _buffer.RemoveAt(CurrentSnapshot);
            WorldState worldState = new WorldState();
            foreach (var player in _buffer[CurrentSnapshot].WorldState.Players)
            {
                if (player.Key != clientId)
                {
                    var snapshotDeltaTime = _buffer[NextSnapshot].TimeStamp - _buffer[CurrentSnapshot].TimeStamp;
                    var snapshotDeltaStatePosition = _buffer[NextSnapshot].WorldState.Players[player.Key].Position -
                                                     _buffer[CurrentSnapshot].WorldState.Players[player.Key].Position;
                    var deltaTime = clientTime - _buffer[CurrentSnapshot].TimeStamp;
                    var interpolatedPosition = 
                        (snapshotDeltaStatePosition / snapshotDeltaTime) * (deltaTime) + _buffer[CurrentSnapshot].WorldState.Players[player.Key].Position;
                    worldState.Players[player.Key] = new PlayerState(interpolatedPosition);
                }
                else
                {
                    worldState.Players[player.Key] = new PlayerState(player.Value.Position);
                }

            }
            return worldState;
        }

        public PlayerState PollClient(int clientId, float clientTime)
        {
            if (SynchronizeState != ClientSynchronizeState.Synchronized)
                return null;
            if (_buffer.Count <= 1)
            {
                SynchronizeState = ClientSynchronizeState.Unsynchronized;
                return null;
            }
            if (clientTime >= _buffer[NextSnapshot].TimeStamp)
                _buffer.RemoveAt(CurrentSnapshot);
            return _buffer[CurrentSnapshot].WorldState.Players[clientId];
        }

        public int Tick => _buffer[CurrentSnapshot].Tick;
    }
}