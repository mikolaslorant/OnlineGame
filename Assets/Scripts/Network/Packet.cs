using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
    public class Packet
    {
        private int _messagesCount;
        private List<Message> _messages;

        public Packet(List<Message> messages)
        {
            _messagesCount = messages.Count;
            _messages = messages;
        }
    }
}
