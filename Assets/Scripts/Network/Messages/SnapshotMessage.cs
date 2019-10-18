using System.ComponentModel.Design.Serialization;
using System.IO;
using Game;
using UnityEngine;

namespace Network
{
    public class SnapshotMessage : Message
    {
        private readonly WorldState _worldState;
        private readonly int _tick;
        private readonly float _timeStamp;

        public SnapshotMessage(int senderId, int receiverId, WorldState worldState, int tick, float timeStamp) : base(senderId, receiverId)
        {
            _worldState = worldState;
            _tick = tick;
            _timeStamp = timeStamp;
        }
        
        public SnapshotMessage(int id, int senderId, int receiverId, WorldState worldState, int tick, float timeStamp) : base(id, senderId, receiverId)
        {
            _worldState = worldState;
            _tick = tick;
            _timeStamp = timeStamp;
        }

        // TODO: optimize byte usage.
        public override byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream()) {
                using (BinaryWriter writer = new BinaryWriter(m)) {
                    writer.Write(Id);
                    writer.Write(SenderId);
                    writer.Write(ReceiverId);
                    writer.Write((int) Type());
                    writer.Write(_worldState.Players.Count);
                    foreach (var player in _worldState.Players)
                    {
                        writer.Write(player.Key); // playerId
                        writer.Write(player.Value.Position.x);
                        writer.Write(player.Value.Position.y);
                        writer.Write(player.Value.Position.z);
                    }
                    writer.Write(_tick);
                    writer.Write(_timeStamp);
                }
                return m.ToArray();
            }
        }

        public override MessageType Type() => MessageType.Snapshot;

        public WorldState WorldState => _worldState;
        public int Tick => _tick;
        public float TimeStamp => _timeStamp;
    }
}
