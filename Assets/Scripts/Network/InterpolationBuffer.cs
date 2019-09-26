    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices.WindowsRuntime;
using Game;
using Network.Enums;
using UnityEditor.U2D;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Network
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

        public PlayerState Poll(float clientTime)
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
            var snapshotDeltaTime = _buffer[NextSnapshot].TimeStamp - _buffer[CurrentSnapshot].TimeStamp;
            var snapshotDeltaStatePosition = _buffer[NextSnapshot].PlayerState.Position -
                                     _buffer[CurrentSnapshot].PlayerState.Position;
            var deltaTime = clientTime - _buffer[CurrentSnapshot].TimeStamp;
            var interpolatedPosition = (snapshotDeltaStatePosition / snapshotDeltaTime) * (deltaTime) + _buffer[CurrentSnapshot].PlayerState.Position;
            return new PlayerState(interpolatedPosition);
        }
    }
}