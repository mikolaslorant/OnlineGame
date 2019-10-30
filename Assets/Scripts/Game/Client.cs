using System.Collections.Generic;
using Game;
using Helpers;
using Network.ClientTools;
using Network.Enums;
using UnityEngine;

namespace Network
{
    public class Client : MonoBehaviour
    {
        // Game rules
        public IDictionary<int, GameObject> players; // other players
        public CharacterController characterController; // client
        public float speed;

        // Network
        public int clientId;
        private const int ServerId = 0;
        public string hostname;
        public int port;
        public int listenPort;
        
        private Connection _connection;
        private IDictionary<int, ConnectionInfo> _connectionsTable;
        private PacketProcessor _packetProcessor;
        private ConnectionInfo _serverInfo;
        
        // Interpolation buffer
        private InterpolationBuffer _interpolationBuffer;
        private float _currentTime;
        // Prediction
        private ClientSidePredictor _clientSidePredictor;
        private int _tick;
       

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<int, ConnectionInfo>();
            _serverInfo = new ConnectionInfo(ServerId, hostname, port);
            _connectionsTable.Add(_serverInfo.ClientId, _serverInfo);
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _interpolationBuffer = new InterpolationBuffer();
            _clientSidePredictor = new ClientSidePredictor(characterController, speed);
            _tick = 0;
        }

        void Update()
        {
            _packetProcessor.ProcessInput();
            UpdateWorldStateWithSnapshot();
            _packetProcessor.ProcessOutput();
            if (_interpolationBuffer.SynchronizeState == ClientSynchronizeState.Synchronized)
            {
                _currentTime += Time.deltaTime;
            }
        }

        void FixedUpdate()
        {
            // Output
            PlayerInput playerInput = PlayerInput.GetPlayerInput(_tick);
            if (playerInput.Bitmap != 0 || playerInput.MouseXAxis != 0 || playerInput.MouseYAxis != 0)
            {
                _clientSidePredictor.UpdatePlayerState(playerInput);
                PlayerInputMessage playerInputMessage = new PlayerInputMessage(clientId, ServerId, playerInput);
                _serverInfo.InputStream.AddToOutput(playerInputMessage);
                _tick++;
            }
        }

        private void UpdateWorldStateWithSnapshot()
        {
            List<Message> messages = _serverInfo.SnapshotStream.GetMessagesReceived();
            // Add to interpolation buffer.
            if (messages.Count > 0)
            {
                SnapshotMessage snapshotMessage = (SnapshotMessage) messages[0];
                if (_interpolationBuffer.SynchronizeState == ClientSynchronizeState.Unsynchronized)
                {
                    _currentTime = snapshotMessage.TimeStamp;
                    _interpolationBuffer.SynchronizeState = ClientSynchronizeState.Buffering;
                }
                _interpolationBuffer.Add(snapshotMessage);
            }
            // Apply interpolated state and correct prediction.
            WorldState worldState = _interpolationBuffer.Poll(clientId, _currentTime);
            if (worldState != null)
            {
                foreach (var player in worldState.Players)
                {
                    // Only remote players
                    if (player.Key != clientId)
                    {
                        players[player.Key].transform.position = player.Value.Position; // setting interpolated position
                    }
                    else
                    {
                        _clientSidePredictor.CorrectPlayerState(player.Value, _interpolationBuffer.Tick);
                    }
                }
            }
        }
    }
}