using System.IO;
using Game;
using Helpers;
using UnityEngine;

namespace Network
{
    public class MessageDeserializer
    {
        public static Message Deserialize(byte[] data)
        {
            Message message = null;
            using (MemoryStream m = new MemoryStream(data)) {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    int id = reader.ReadInt32();
                    int senderId = reader.ReadInt32();
                    int receiverId = reader.ReadInt32();

                    MessageType messageType = (MessageType) reader.ReadInt32();
                    switch (messageType)
                    {
                        case MessageType.ACK:
                            MessageType ackType = (MessageType) reader.ReadInt32();
                            message = new AckMessage(id, senderId, receiverId, ackType);
                            break;
                        case MessageType.Input:
                            PlayerInput playerInput = new PlayerInput(reader.ReadByte(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadInt32());
                            message = new PlayerInputMessage(id, senderId, receiverId, playerInput);
                            break;
                        case MessageType.Snapshot:
                            WorldState worldState = new WorldState();
                            var playerCount = reader.ReadInt32();
                            for (int i = 0; i < playerCount; i++)
                            {
                                int playerId = reader.ReadInt32();
                                worldState.Players[playerId] 
                                    = new PlayerState(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), 
                                                      new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), reader.ReadInt32());
                            }
                            int tick = reader.ReadInt32();
                            float timeStamp = reader.ReadSingle();
                            message = new SnapshotMessage(id, senderId, receiverId, worldState, tick, timeStamp);
                            break;
                        case MessageType.ConnectionRequest:
                            message = new ConnectionRequestMessage(id, senderId, receiverId);
                            break;
                        case MessageType.ConnectionResponse:
                            message = new ConnectionResponseMessage(id, senderId, receiverId);
                            break;
                    }
                }
            }

            return message;
        }
    }
}
