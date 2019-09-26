using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Network
{
    public class Connection
    {
        public double packetLoss = 0.2;

        private readonly Queue<byte[]> _queue;
        private readonly UdpClient _udpClient;
        private readonly System.Random _random;

        public Connection(int listenPort)
        {
            _random = new System.Random();
            var thread = new Thread(PollData);
            _queue = new Queue<byte[]>();
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

        public byte[] GetData()
        {
            byte[] data = null;
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    data = _queue.Dequeue();
                }
            }
            return data;
        }

        private void PollData()
        {
            while (true)
            {
                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var data = _udpClient.Receive(ref remoteIpEndPoint);
                lock (_queue)
                {
                    _queue.Enqueue(data);
                }
            }
        }
    
    }
}
