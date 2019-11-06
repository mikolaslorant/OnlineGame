using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Network.Streams
{
    public class ReliableSlowStream : Stream
    {
        private List<Message> _inStream;
        private int _lastReceivedMessageId;
        private List<MessageWithTimeout> _outStream;
        private int _lastSentMessageId;

        private const long TimeoutMs = 1000L;

        public ReliableSlowStream(MessageType messageType) : base(messageType)
        {
            _inStream = new List<Message>();
            _lastReceivedMessageId = 0;
            _outStream = new List<MessageWithTimeout>();
            _lastSentMessageId = 0;
        }
    
        public override void AddToInput(Message message)
        {
            if (message.Type() != MessageType.ACK)
                _inStream.Add(message);
            else
                _outStream.RemoveAll(m => m.Message.Id <= message.Id);
        }

        public override void AddToOutput(Message message)
        {
            message.Id = ++_lastSentMessageId;
            _outStream.Add(new MessageWithTimeout(message));
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
                    _outStream.Add(new MessageWithTimeout(ackMessage));
                }
            }
            _inStream.Clear();
            return messagesReceived;
        }

        public override List<Message> GetMessagesToSend()
        {
            List<Message> messagesToSend = new List<Message>();
            List<MessageWithTimeout> newOutStream = new List<MessageWithTimeout>();
            foreach (MessageWithTimeout messageWithTimeout in _outStream)
            {
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                if (messageWithTimeout.Milliseconds == 0 || milliseconds - messageWithTimeout.Milliseconds > TimeoutMs)
                {
                    messageWithTimeout.Milliseconds = milliseconds;
                    messagesToSend.Add(messageWithTimeout.Message);
                }
                if (messageWithTimeout.Message.Type() != MessageType.ACK)
                {
                    
                    newOutStream.Add(messageWithTimeout);
                }
            }
            
            _outStream = newOutStream;
            return messagesToSend;
        }

        public override Reliability Type()
        {
            return Reliability.ReliableSlow;
        }
        
        private class MessageWithTimeout
        {
            public MessageWithTimeout(Message message)
            {
                Message = message;
                Milliseconds = 0;
            }

            public Message Message { get; }

            public long Milliseconds { get; set; }
        }
    }
}
