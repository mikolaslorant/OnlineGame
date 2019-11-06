using System.Collections.Generic;

namespace Network.Streams
{
    public abstract class Stream
    {
        private MessageType _messageType;

        public Stream(MessageType messageType)
        {
            _messageType = messageType;
        }

        public MessageType MessageType => _messageType;

        public abstract void AddToInput(Message message);
        public abstract void AddToOutput(Message message);
        public abstract List<Message> GetMessagesReceived();
        public abstract List<Message> GetMessagesToSend();
        public abstract Reliability Type();
    }
}
