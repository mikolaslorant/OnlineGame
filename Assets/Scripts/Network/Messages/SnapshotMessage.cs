using System.IO;
using Game;

namespace Network
{
    public class SnapshotMessage : Message
    {
        private readonly PlayerState _playerState;

        public SnapshotMessage(int senderId, int receiverId, PlayerState playerState) : base(senderId, receiverId)
        {
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
    }
}
