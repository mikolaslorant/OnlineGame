
namespace Network
{
    public abstract class Message
    {
        public Message()
        {
            this.SenderId = 0;
            this.ReceiverId = 0;
        }

        public Message(int senderId, int receiverId)
        {
            this.SenderId = senderId;
            this.ReceiverId = receiverId;
        }
        
        public Message(int id, int senderId, int receiverId)
        {
            this.Id = id;
            this.SenderId = senderId;
            this.ReceiverId = receiverId;
        }
        public int Id { get; set; }
        
        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public abstract byte[] Serialize();
        public abstract MessageType Type();
    }
}
