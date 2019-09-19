using System.Collections.Generic;
using Game;
using UnityEditor;
using UnityEngine;

namespace Network
{
    public class Server : MonoBehaviour
    {
        private const int ServerId = 0;
        
        public int listenPort;

        public CharacterController characterController;

        private Connection _connection;
        private IDictionary<int, ConnectionInfo> _connectionsTable;
        private PacketProcessor _packetProcessor;
        private float _currentTime;

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<int, ConnectionInfo>();
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _connectionsTable.Add(1, new ConnectionInfo(1, "localhost", 2000));
            _currentTime = 0f;
        }

        void Update()
        {
            _currentTime += Time.deltaTime;
            _packetProcessor.ProcessInput();
            ApplyPlayerMovements();
            BroadCastSnapshot();
            _packetProcessor.ProcessOutput();
        }

        private void ApplyPlayerMovements()
        {
            foreach (var connection in _connectionsTable.Values)
            {
                // Input
                List<Message> playerInputsReceived = connection.InputStream.GetMessagesReceived();
                foreach (var message in playerInputsReceived)
                {
                    var playerInputMessage = (PlayerInputMessage) message;
                    if (playerInputMessage.PlayerInput.GetKeyDown(KeyCode.UpArrow))
                        characterController.Move(new Vector3(0, 1, 0));
                    if (playerInputMessage.PlayerInput.GetKeyDown(KeyCode.DownArrow))
                        characterController.Move(new Vector3(0, -1, 0));
                    if (playerInputMessage.PlayerInput.GetKeyDown(KeyCode.RightArrow))
                        characterController.Move(new Vector3(1, 0, 0));
                    if (playerInputMessage.PlayerInput.GetKeyDown(KeyCode.LeftArrow))
                        characterController.Move(new Vector3(-1, 0, 0));
                }
            }
        }

        private void BroadCastSnapshot()
        {
            // Output
            PlayerState playerState = new PlayerState(characterController.transform.position);
            foreach (var connection in _connectionsTable.Values)
            {
                SnapshotMessage snapshotMessage = new SnapshotMessage(ServerId, connection.ClientId, playerState, _currentTime);
                connection.SnapshotStream.AddToOutput(snapshotMessage);
            }
        }
    }
}
