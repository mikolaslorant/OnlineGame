using System.Collections.Generic;
using UnityEngine;

namespace Network.Streams
{
    public class ReliableFastStream : Stream
    {
        private List<Message> _inStream;
        private int _lastReceivedMessageId;
        private List<Message> _outStream;
        private int _lastSentMessageId;

        public ReliableFastStream(MessageType messageType) : base(messageType)
        {
            _inStream = new List<Message>();
            _lastReceivedMessageId = 0;
            _outStream = new List<Message>();
            _lastSentMessageId = 0;
        }
    
        public override void AddToInput(Message message)
        {
            if (message.Type() != MessageType.ACK)
                _inStream.Add(message);
            else
                _outStream.RemoveAll(m => m.Id <= message.Id);
        }

        public override void AddToOutput(Message message)
        {
            message.Id = ++_lastSentMessageId;
            _outStream.Add(message);
        }
        
        public override List<Message> GetMessagesReceived()
        {
            List<Message> messagesReceived = new List<Message>();
            foreach (Message message in _inStream)
            {
                if (message.Id == _lastReceivedMessageId + 1)
                {
                    messagesReceived.Add(message);
                    _lastReceivedMessageId++;
                    AckMessage ackMessage = new AckMessage(message.Id, message.ReceiverId, message.SenderId, message.Type());
                    _outStream.Add(ackMessage);
                }
            }
            _inStream.Clear();
            return messagesReceived;
        }

        public override List<Message> GetMessagesToSend()
        {
            List<Message> messagesToSend = new List<Message>();
            List<Message> newOutStream = new List<Message>();
            foreach (Message message in _outStream)
            {
                messagesToSend.Add(message);
                if (message.Type() != MessageType.ACK)
                {
                    newOutStream.Add(message);
                }
            }

            _outStream = newOutStream;
            return messagesToSend;
        }

        public override Reliability Type() => Reliability.ReliableFast;
    }
}
