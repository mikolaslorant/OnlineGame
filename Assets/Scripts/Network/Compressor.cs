using System.IO;
using Game;
using Helpers;
using UnityEngine;


namespace DefaultNamespace
{
    public class Compressor
    {
        public byte[] Compress(Message message)
        {
            switch (message.Type())
            {
                case MessageType.ACK:
                    return this.WriteAckMessage((AckMessage) message);
                case MessageType.ConnectionRequest:
                    return this.WriteConnectionRequestMessage((ConnectionRequestMessage) message);
                case MessageType.ConnectionResponse:
                    return this.WriteConnectionResponseMessage((ConnectionResponseMessage) message);
                
            }
        }

        public void Decompress(byte[] packet)
        {

        }

        private byte[] WriteAckMessage(AckMessage m)
        {
            int word1 = 0;
            word = m.Id();
            byte word3 = m.Sender();
            word2 <<= 3;
            word2 += (byte) m.Receiver();
            word2 <<= 3;
            word2 += (byte) m.Type();
            word2 <<= 3;
            word2 += (byte) m.AckType();
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(word1);
                    writer.Write(word2);
                    writer.Write(word3);
                }

                return m.ToArray();
            }
        }

        private AckMessage ReadAckMessage(byte[] bytes)
        {
            using (MemoryStream m = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int messageId = reader.ReadInt32();
                    char byte2 = reader.ReadChar();
                    char byte3 = reader.ReadChar();
                    int receiver = (int) byte2 % 8;
                    byte2 >>= 3;
                    int sender = (int) byte2 % 8;
                    MessageType ackType = (MessageType) byte3 % 8;
                    return new AckMessage(messageId, sender, receiver, ackType);
                }
            }

        }

        private byte[] WriteConnectionRequestMessage(ConnectionRequestMessage m)
        {
            int word1 = 0;
            word = m.Id();
            byte word3 = m.Sender();
            word2 <<= 3;
            word2 += (byte) m.Receiver();
            word2 <<= 3;
            word2 += (byte) m.Type();
            word2 <<= 3;
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(word1);
                    writer.Write(word2);
                    writer.Write(word3);
                }

                return m.ToArray();
            }
        }

        private ConnectionRequestMessage ReadConnectionRequestMessage(byte[] bytes)
        {
            using (MemoryStream m = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int messageId = reader.ReadInt32();
                    char byte2 = reader.ReadChar();
                    int receiver = (int) byte2 % 8;
                    byte2 >>= 3;
                    int sender = (int) byte2 % 8;
                    return new ConnectionRequestMessage(messageId, sender, receiver);
                }
            }
        }

        private byte[] WriteConnectionResponseMessage(ConnectionResponseMessage m)
        {
            int word1 = 0;
            word = m.Id();
            byte word3 = m.Sender();
            word2 <<= 3;
            word2 += (byte) m.Receiver();
            word2 <<= 3;
            word2 += (byte) m.Type();
            word2 <<= 3;
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(word1);
                    writer.Write(word2);
                    writer.Write(word3);
                }

                return m.ToArray();
            }
        }
        
        
    }
}