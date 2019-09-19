using System.ComponentModel.Design.Serialization;
using System.IO;
using Game;
using UnityEngine;

namespace Network
{
    public class SnapshotMessage : Message
    {
        private readonly PlayerState _playerState;
        private readonly float _timeStamp;

        public SnapshotMessage(int senderId, int receiverId, PlayerState playerState, float timeStamp) : base(senderId, receiverId)
        {
            _timeStamp = timeStamp;
            _playerState = playerState;
        }
        
        public SnapshotMessage(int id, int senderId, int receiverId, PlayerState playerState) : base(id, senderId, receiverId)
        {
            _playerState = playerState;
        }

        public override byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(m)) {
                    writer.Write(Id);
                    writer.Write(SenderId);
                    writer.Write(ReceiverId);
                    writer.Write((int) Type());
                    writer.Write(_playerState.Position.x);
                    writer.Write(_playerState.Position.y);
                    writer.Write(_playerState.Position.z);
                }
                return m.ToArray();
            }
        }

        public override MessageType Type() => MessageType.Snapshot;

        public PlayerState PlayerState => _playerState;

        public float TimeStamp => _timeStamp;
    }
}
