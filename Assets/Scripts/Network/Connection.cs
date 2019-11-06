using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Network
{
    public class Connection
    {
        public double packetLoss = 0.0;

        private readonly Queue<ConnectionPacket> _queue;
        private readonly UdpClient _udpClient;
        private readonly System.Random _random;

        public Connection(int listenPort)
        {
            _random = new System.Random();
            var thread = new Thread(PollData);
            _queue = new Queue<ConnectionPacket>();
            _udpClient = new UdpClient(listenPort);
            thread.Start();
        }
    
        public void SendData(byte[] data, string hostname, int port)
        {
            if (_random.NextDouble() >= packetLoss)
            {
                _udpClient.Send(data, data.Length, hostname, port);
            }
        }

        public ConnectionPacket GetData()
        {
            ConnectionPacket connectionPacket = null;
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    connectionPacket = _queue.Dequeue();
                }
            }
            return connectionPacket;
        }

        private void PollData()
        {
            while (true)
            {
                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var bytes = _udpClient.Receive(ref remoteIpEndPoint);
                var data = new ConnectionPacket(bytes);
                data.Ip = remoteIpEndPoint.Address;
                data.Port = remoteIpEndPoint.Port;
                lock (_queue)
                {
                    _queue.Enqueue(data);
                }
            }
        }
    
    }
}
