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
            this._minimumSize = 5;
            this._buffer = new List<SnapshotMessage>();
            this.SynchronizeState = ClientSynchronizeState.Unsynchronized;
        }
        
        public ClientSynchronizeState SynchronizeState { get; set; }

        public void Add(SnapshotMessage snapshotMessage)
        {
            _buffer.Add(snapshotMessage);
        }

        public PlayerState Poll(float clientTime)
        {
            if (_buffer.Count == 0)
                SynchronizeState = ClientSynchronizeState.Buffering;
            if (_buffer.Count < _minimumSize && SynchronizeState == ClientSynchronizeState.Buffering)
                return null;
            SynchronizeState = ClientSynchronizeState.Synchronized;
            if (_buffer[NextSnapshot].TimeStamp >= clientTime)
                _buffer.RemoveAt(CurrentSnapshot);
            var deltaTime = _buffer[NextSnapshot].TimeStamp - _buffer[CurrentSnapshot].TimeStamp;
            var deltaStatePosition = _buffer[NextSnapshot].PlayerState.Position -
                                     _buffer[CurrentSnapshot].PlayerState.Position;
            var interpolatedPosition =
                (deltaStatePosition / deltaTime) * (clientTime - _buffer[CurrentSnapshot].TimeStamp);
            return new PlayerState(interpolatedPosition);
        }
    }
}