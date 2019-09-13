using System.Collections.Generic;

namespace Network
{
    public class UnreliableStream : Stream
    {
        private List<Message> _inStream;
        private int _lastReceivedMessageId;
        private List<Message> _outStream;
        private int _lastSentMessageId;

        public UnreliableStream(MessageType messageType) : base(messageType)
        {
            _inStream = new List<Message>();
            _lastReceivedMessageId = 0;
            _outStream = new List<Message>();
            _lastSentMessageId = 0;
        }
        
        public override void AddToInput(Message message)
        {
            _inStream.Add(message);
        }

        public override void AddToOutput(Message message)
        {
            message.Id = ++_lastSentMessageId;
            _outStream.Add(message);
        }

        public override List<Message> GetMessagesReceived()
        {
            List<Message> messagesReceived = new List<Message>();
            Message newestMessage = null;
            foreach (Message message in _inStream)
            {
                if (message.Id > _lastReceivedMessageId)
                {
                    newestMessage = message;
                    _lastReceivedMessageId = message.Id;
                }
            }
            _inStream.Clear();
            if (newestMessage != null)
                messagesReceived.Add(newestMessage);
            return messagesReceived;
        }

        public override List<Message> GetMessagesToSend()
        {
            List<Message> messagesToSend = new List<Message>();
            foreach (Message message in _outStream)
            {
                messagesToSend.Add(message);
            }
            
            _outStream.Clear();
            return messagesToSend;
        }

        public override Reliability Type()
        {
            return Reliability.Unreliable;
        }
        
    }
}
