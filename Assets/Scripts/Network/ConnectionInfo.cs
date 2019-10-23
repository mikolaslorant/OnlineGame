using System.Collections.Generic;
using Network.Streams;

namespace Network
{
    public class ConnectionInfo
    {
        private readonly int _clientId;
        private readonly string _hostname;
        private readonly int _port;

        private List<Stream> _streams;

        public ConnectionInfo(int clientId, string hostname, int port)
        {
            this._clientId = clientId;
            this._hostname = hostname;
            this._port = port;
            this._streams = new List<Stream>()
            {
                new UnreliableStream(MessageType.Snapshot), 
                new ReliableFastStream(MessageType.Input)
            };
        }

        public int ClientId => _clientId;
        public string Hostname => _hostname;
        public int Port => _port;
        public List<Stream> Streams => _streams;

        public Stream SnapshotStream => _streams[0];
        public Stream InputStream => _streams[1];
    }
}
