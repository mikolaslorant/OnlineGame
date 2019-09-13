using System.IO;
using UnityEngine;

namespace Network
{
    public class AckMessage : Message
    {
        private readonly MessageType _ackType;

        public AckMessage(int senderId, int receiverId, MessageType ackType) : base(senderId, receiverId)
        {
            _ackType = ackType;
        }
        
        public AckMessage(int id, int senderId, int receiverId, MessageType ackType) : base(id, senderId, receiverId)
        {
            _ackType = ackType;
        }

        public override byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(m)) {
                    writer.Write(Id);
                    writer.Write(SenderId);
                    writer.Write(ReceiverId);
                    writer.Write((int) Type());
                    writer.Write((int) _ackType);
                }
                return m.ToArray();
            }
        }

        public override MessageType Type()
        {
            return MessageType.ACK;
        }

        public MessageType AckType => _ackType;
    }
}
