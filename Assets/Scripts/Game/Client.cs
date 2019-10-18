using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Helpers;
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
        public float gravity;
        
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
        
        //  Client game state
        private InterpolationBuffer _interpolationBuffer;
        private float _currentTime;
        // Clients input tick
        private int _tick;
        private List<PlayerInput> _playerInputs;
       

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<int, ConnectionInfo>();
            _serverInfo = new ConnectionInfo(ServerId, hostname, port);
            _connectionsTable.Add(_serverInfo.ClientId, _serverInfo);
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _interpolationBuffer = new InterpolationBuffer();
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
            PlayerInput playerInput = PlayerInput.GetPlayerInput(_tick++);
            if (playerInput.Bitmap != 0)
            {
                PlayerInputMessage playerInputMessage = new PlayerInputMessage(clientId, ServerId, playerInput);
                _serverInfo.InputStream.AddToOutput(playerInputMessage);
            }
            _playerInputs.Add(playerInput);
            UpdatePlayerStateWithPrediction();
        }

        private void UpdateWorldStateWithSnapshot()
        {
            List<Message> messages = _serverInfo.SnapshotStream.GetMessagesReceived();
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

            WorldState worldState = _interpolationBuffer.Poll(clientId, _currentTime);
            int serverTick = _interpolationBuffer.Tick;
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
                        foreach (var inputs in _playerInputs)
                        {
                            
                        }
                    }
                }
            }
        }

        public void UpdatePlayerStateWithPrediction()
        {
            PlayerInput playerInput = _playerInputs.Last();
            var totalMovement = new Vector3(0,0,0);
            if (playerInput.GetKeyDown(KeyCode.UpArrow))
                totalMovement += new Vector3(0, 0, 1);
            if (playerInput.GetKeyDown(KeyCode.DownArrow))
                totalMovement += new Vector3(0, 0, -1);
            if (playerInput.GetKeyDown(KeyCode.RightArrow))
                totalMovement += new Vector3(1, 0, 0);
            if (playerInput.GetKeyDown(KeyCode.LeftArrow))
                totalMovement += new Vector3(-1, 0, 0);
            totalMovement *= speed;
            totalMovement.y -= 0.5f * gravity;
            characterController.Move(totalMovement);
        }
    }
}