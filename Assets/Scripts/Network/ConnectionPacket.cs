
using System.Net;

namespace Network
{
    public class ConnectionPacket
    {

        public ConnectionPacket(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; set; }
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
    }
}