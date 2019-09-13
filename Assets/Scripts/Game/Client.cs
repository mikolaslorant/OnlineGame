using System.Collections.Generic;
using System.Linq;
using Game;
using Helpers;
using UnityEngine;

namespace Network
{
    public class Client : MonoBehaviour
    {
        private const int ServerId = 0;
        
        public string hostname;
        public int port;
        public int listenPort;

        public int clientId;
        
        public CharacterController characterController;
        
        private Connection _connection;
        private IDictionary<int, ConnectionInfo> _connectionsTable;
        private PacketProcessor _packetProcessor;

        private ConnectionInfo _serverInfo;

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<int, ConnectionInfo>();
            _serverInfo = new ConnectionInfo(ServerId, hostname, port);
            _connectionsTable.Add(_serverInfo.ClientId, _serverInfo);
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
        }

        void Update()
        {
            _packetProcessor.ProcessInput();
            // Input
            List<Message> snapshotsReceived = _serverInfo.SnapshotStream.GetMessagesReceived();
            foreach (var message in snapshotsReceived)
            {
                var snapshotMessage = (SnapshotMessage) message;
                characterController.transform.position = snapshotMessage.PlayerState.Position;
            }

            // Output
            PlayerInput playerInput = PlayerInput.GetPlayerInput();
            if (playerInput.Bitmap != 0)
            {
                PlayerInputMessage playerInputMessage = new PlayerInputMessage(clientId, ServerId, playerInput);
                _serverInfo.InputStream.AddToOutput(playerInputMessage);
            }

            _packetProcessor.ProcessOutput();
        }
    }
}