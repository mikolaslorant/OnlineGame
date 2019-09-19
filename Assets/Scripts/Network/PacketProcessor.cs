using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class PacketProcessor
    {
        private readonly Connection _connection;

        private readonly IDictionary<int, ConnectionInfo> _connectionsTable;

        public PacketProcessor(Connection connection, IDictionary<int, ConnectionInfo> connectionsTable)
        {
            _connection = connection;
            _connectionsTable = connectionsTable;
        }

        public void ProcessInput()
        {
            // Input
            byte[] data;
            while ((data = _connection.GetData()) != null)
            {
                Message message = MessageDeserializer.Deserialize(data);
                Stream stream;
                if (message.Type() == MessageType.ACK)
                    stream = _connectionsTable[message.SenderId].Streams
                        .Find(s => s.MessageType == ((AckMessage) message).AckType);
                else
                    stream = _connectionsTable[message.SenderId].Streams
                        .Find(s => s.MessageType == message.Type());
                stream.AddToInput(message);
            }
        }

        public void ProcessOutput()
        {
            foreach (var connection in _connectionsTable.Values)
            {
                foreach (var stream in connection.Streams)
                {
                    List<Message> messagesToSend = stream.GetMessagesToSend();
                    foreach (var message in messagesToSend)
                    {
                        _connection.SendData(message.Serialize(), connection.Hostname, connection.Port);
                    }
                }
            }
        }
    }
}
