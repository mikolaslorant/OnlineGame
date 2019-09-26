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
                            PlayerInput playerInput = new PlayerInput(reader.ReadByte());
                            message = new PlayerInputMessage(id, senderId, receiverId, playerInput);
                            break;
                        case MessageType.Snapshot:
                            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            float timeStamp = reader.ReadSingle();
                            PlayerState playerState = new PlayerState(position);
                            message = new SnapshotMessage(id, senderId, receiverId, playerState, timeStamp);
                            break;
                    }
                }
            }

            return message;
        }
    }
}
