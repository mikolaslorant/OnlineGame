using System.IO;
using Helpers;

namespace Network
{
    public class PlayerInputMessage : Message
    {
        private readonly PlayerInput _playerInput;

        private readonly long _sequenceNumber;

        public PlayerInputMessage(int senderId, int receiverId, PlayerInput playerInput, long sequenceNumber) : base(senderId, receiverId)
        {
            _playerInput = playerInput;
            _sequenceNumber = sequenceNumber;
        }

        public PlayerInputMessage(int id, int senderId, int receiverId, PlayerInput playerInput, long sequenceNumber) : base(id, senderId, receiverId)
        {
            _playerInput = playerInput;
            _sequenceNumber = sequenceNumber;
        }
        
        public override byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(m)) {
                    writer.Write(Id);
                    writer.Write(SenderId);
                    writer.Write(ReceiverId);
                    writer.Write((int) Type());
                    writer.Write(_playerInput.Bitmap);
                    writer.Write(_sequenceNumber);
                }
                return m.ToArray();
            }
        }

        public override MessageType Type()
        {
            return MessageType.Input;
        }

        public PlayerInput PlayerInput => _playerInput;
        public long SequenceNumber => _sequenceNumber;
    }
}
