using System;
using System.Collections;
using System.IO;
using Game;
using Helpers;
using Network;
using Unity.UNetWeaver;
using UnityEngine;


namespace Network
{
    public class Compressor
    {
        public byte[] Compress(Message message)
        {
            //Debug.Log("Sender: " + message.SenderId + "Receiver: " + message.ReceiverId);
            switch (message.Type())
            {
                case MessageType.ACK:
                    return this.WriteAckMessage((AckMessage) message);
                case MessageType.ConnectionRequest:
                    return this.WriteConnectionRequestMessage((ConnectionRequestMessage) message);
                case MessageType.ConnectionResponse:
                    return this.WriteConnectionResponseMessage((ConnectionResponseMessage) message);
                case MessageType.Snapshot:
                    return this.WriteSnapshot((SnapshotMessage) message);
                case MessageType.Input:
                    return this.WriteInputMessage((PlayerInputMessage) message);
                default:
                    return null;
            }
        }

        public Message Decompress(byte[] packet)
        {
            byte type = 0;
            using (MemoryStream m = new MemoryStream(packet))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int id = reader.ReadInt32();
                    byte word = reader.ReadByte();
                    type = reader.ReadByte();
                    //Debug.Log("Receiver: " + (word & 7) + "Sender " + ((word >> 3) & 7));
                }
            }

            type >>= 3;

            MessageType messageType = (MessageType) type;

            switch (messageType)
            {
                case MessageType.ACK:
                    return ReadAckMessage(packet);
                case MessageType.ConnectionRequest:
                    return ReadConnectionRequestMessage(packet);
                case MessageType.ConnectionResponse:
                    return ReadConnectionResponseMessage(packet);
                case MessageType.Snapshot:
                    return ReadSnapshotMessage(packet);
                case MessageType.Input:
                    return ReadInputMessage(packet);
                default:
                    return null;
            }

            return null;
        }

        private byte[] WriteAckMessage(AckMessage message)
        {
            int word1 = 0;
            word1 = message.Id;
            byte word2 = (byte) message.SenderId;
            byte word3 = 0;
            word2 <<= 3;
            word2 |= (byte) message.ReceiverId;
            word3 |= (byte) message.Type();
            word3 <<= 3;
            word3 |= (byte) message.AckType;
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
                    int receiver = (int) byte2 % 7;
                    byte2 >>= 3;
                    int sender = (int) byte2 % 7;
                    MessageType ackType = (MessageType) ((int) byte3 % 7);
                    return new AckMessage(messageId, sender, receiver, ackType);
                }
            }
        }

        private byte[] WriteConnectionRequestMessage(ConnectionRequestMessage message)
        {
            int word1 = 0;
            word1 = message.Id;
            byte word2 = 0;
            word2 |= (byte) message.SenderId;
            word2 <<= 3;
            word2 |= (byte) message.ReceiverId;
            byte word3 = (byte) message.Type();
            word3 <<= 3;
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
                    int receiver = (int) byte2 % 7;
                    byte2 >>= 3;
                    int sender = (int) byte2 % 7;
                    return new ConnectionRequestMessage(messageId, sender, receiver);
                }
            }
        }

        private byte[] WriteConnectionResponseMessage(ConnectionResponseMessage message)
        {
            int word1 = 0;
            word1 = message.Id;
            byte word2 = 0;
            byte word3 = 0;
            word2 |= (byte) message.SenderId;
            word2 <<= 3;
            word2 += (byte) message.ReceiverId;
            word3 |= (byte) message.Type();
            word3 <<= 3;
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

        private ConnectionResponseMessage ReadConnectionResponseMessage(byte[] bytes)
        {
            using (MemoryStream m = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int messageId = reader.ReadInt32();
                    char byte2 = reader.ReadChar();
                    int receiver = (int) byte2 % 7;
                    byte2 >>= 3;
                    int sender = (int) byte2 % 7;
                    return new ConnectionResponseMessage(messageId, sender, receiver);
                }
            }
        }

        private byte[] WriteSnapshot(SnapshotMessage message)
        {
            int word1 = message.Id;
            byte word2 = (byte) message.SenderId;
            word2 <<= 3;
            word2 |= (byte) message.ReceiverId;
            byte word3 = (byte) message.Type();
            word3 <<= 3;
            word3 |= (byte) message.WorldState.Players.Count;

            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(word1);
                    writer.Write(word2);
                    writer.Write(word3);

                    foreach (var ps in message.WorldState.Players)
                    {
                        long word4 = 0;
                        word4 |= (byte) ps.Key;
                        word4 <<= 12;
                        int xDim = (int) Math.Floor((ps.Value.Position.x  + 100)/0.1);
                        word4 |= xDim;
                        int yDim = (int) Math.Floor((ps.Value.Position.y  + 100)/0.1);
                        word4 <<= 12;
                        word4 |= yDim;
                        int zDim = (int)Math.Floor((ps.Value.Position.z  + 100)/0.1);
                        word4 <<= 12;
                        word4 |= zDim;
                        word4 <<= 7;
                        word4 |= (int) Math.Floor((ps.Value.Rotation.x + 1) / 0.025);
                        word4 <<= 7;
                        word4 |= (int) Math.Floor((ps.Value.Rotation.y + 1) / 0.025);
                        word4 <<= 7;
                        word4 |= (int) Math.Floor((ps.Value.Rotation.z + 1) / 0.025);
                        word4 <<= 1;
                        word4 |= (ps.Value.Rotation.w > 0) ? 1 : 0;
                        word4 <<= 4;
                        word4 |= (ps.Value.Health / 10);
                        writer.Write(word4);
                    }
                    Debug.Log(message.Tick);
                    writer.Write(message.Tick);
                    writer.Write(message.TimeStamp);
                }

                return m.ToArray();
            }
        }

        private SnapshotMessage ReadSnapshotMessage(byte[] message)
        {
            using (MemoryStream m = new MemoryStream(message))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int messageId = reader.ReadInt32();
                    byte word1 = reader.ReadByte();
                    int receiver = word1 & 7;
                    word1 >>= 3;
                    int sender = word1 & 7;
                    word1 = reader.ReadByte();
                    int countPlayers = word1 & 7;
                    WorldState worldState = new WorldState();
                    for (int i = 0; i < countPlayers; i++)
                    {
                        long val = reader.ReadInt64();
                        int hp = (int) ((val & 15) * 10);
                        val >>= 4;
                        byte isPositive = (byte) (val & 1);
                        val >>= 1;
                        float zRotation = ((val & 127) * 0.025f) - 1.0f;
                        val >>= 7;
                        float yRotation = ((val & 127) * 0.025f) - 1.0f;
                        val >>= 7;
                        float xRotation = ((val & 127) * 0.025f) - 1.0f;
                        val >>= 7;
                        float wRotation = (float) Math.Sqrt(1.0f - (float) Math.Pow(zRotation, 2) -
                                                            (float) Math.Pow(yRotation, 2) -
                                                            (float) Math.Pow(xRotation, 2));
                        wRotation *= (isPositive > 0) ? 1 : -1;
                        Quaternion quaternion = new Quaternion(xRotation, yRotation, zRotation, wRotation);
                        float zPosition = ((val & 4095) * 0.1f - 100.0f);
                        val >>= 12;
                        float yPosition = ((val & 4095) * 0.1f - 100.0f);
                        val >>= 12;
                        float xPosition = ((val & 4095) * 0.1f - 100.0f);
                        Vector3 position = new Vector3(xPosition, yPosition, zPosition);
                        val >>= 12;
                        int key = (int) (val & 7);
                        PlayerState playerState = new PlayerState(position, quaternion, hp);
                        worldState.Players.Add(key, playerState);
                    }

                    int tick = reader.ReadInt32();
                    float timestamp = reader.ReadSingle();
                    
                    
                    return new SnapshotMessage(messageId, sender, receiver, worldState, tick, timestamp);
                }
            }
        }
        
        private byte[] WriteInputMessage(PlayerInputMessage message)
        {
            int word1 = message.Id;
            byte word2 = 0;
            word2 |= (byte)message.SenderId;
            word2 <<= 3;
            word2 |= (byte) message.ReceiverId;
            byte word3 = (byte) message.Type();
            word3 <<= 3;
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(word1);
                    writer.Write(word2);
                    writer.Write(word3);
                    writer.Write(message.PlayerInput.MouseXAxis);
                    writer.Write(message.PlayerInput.MouseYAxis);
                    writer.Write(message.PlayerInput.Bitmap);
                    writer.Write(message.PlayerInput.Tick);
                }

                return m.ToArray();
            }
        }
        
        private PlayerInputMessage ReadInputMessage(byte[] packet)
        {
            using (MemoryStream m = new MemoryStream(packet))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int id = reader.ReadInt32();
                    byte word1 = reader.ReadByte();
                    int receiver = word1 & 7;
                    word1 >>= 3;
                    int sender = word1 & 7;
                    byte word2 = reader.ReadByte();
                    float xMouse = reader.ReadSingle();
                    float yMouse = reader.ReadSingle();
                    byte bitmap = reader.ReadByte();
                    int tick = reader.ReadInt32();
                    return new PlayerInputMessage(id, sender, receiver, new PlayerInput(bitmap, xMouse, yMouse, tick));
                }
            }
        }
        
    }
}