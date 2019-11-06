using System.IO;

namespace Network
{
    public class ConnectionResponseMessage : Message
    {
        public ConnectionResponseMessage(int senderId, int receiverId) : base(senderId, receiverId)
        {
        }

        public ConnectionResponseMessage(int id, int senderId, int receiverId) : base(id, senderId, receiverId)
        {
        }
        
        public override byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(Id);
                    writer.Write(SenderId);
                    writer.Write(ReceiverId);
                    writer.Write((int) Type());
                }
                return m.ToArray();
            }
        }

        public override MessageType Type()
        {
            return MessageType.ConnectionResponse;
        }
    }
}