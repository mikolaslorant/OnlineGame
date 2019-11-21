using System.Collections.Generic;
using System.Net;
using Network.Streams;
using UnityEngine;

namespace Network
{
    public class PacketProcessor
    {
        private int _idCounter;
        private readonly Connection _connection;
        private readonly IDictionary<IPAddress, ConnectionInfo> _connectionsTable;
        private Compressor _compressor;

        public PacketProcessor(Connection connection, IDictionary<IPAddress, ConnectionInfo> connectionsTable)
        {
            _connection = connection;
            _connectionsTable = connectionsTable;
            _idCounter = 0;
            _compressor = new Compressor();
        }

        public void ProcessInput()
        {
            // Input
            ConnectionPacket connectionPacket;
            while ((connectionPacket = _connection.GetData()) != null)
            {
                InitializeConnectionInfoIfNewConnection(connectionPacket);
                Message message = _compressor.Decompress(connectionPacket.Data);
                Stream stream;
                if (message.Type() == MessageType.ACK)
                    stream = _connectionsTable[connectionPacket.Ip].Streams
                        .Find(s => s.MessageType == ((AckMessage) message).AckType);
                else
                    stream = _connectionsTable[connectionPacket.Ip].Streams
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
                        _connection.SendData(this._compressor.Compress(message), connection.Hostname, connection.Port);
                    }
                }
            }
        }

        private void InitializeConnectionInfoIfNewConnection(ConnectionPacket connectionPacket)
        {
            if (!_connectionsTable.ContainsKey(connectionPacket.Ip))
            {
                _connectionsTable[connectionPacket.Ip] = new ConnectionInfo(++_idCounter, connectionPacket.Ip.ToString(), connectionPacket.Port);
            }
        }
    }
}
