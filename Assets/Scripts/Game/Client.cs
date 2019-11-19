﻿using System.Collections.Generic;
using System.Net;
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
        private IDictionary<int, GameObject> _players; // other players
        private CharacterController _characterController; // client
        public float speed;
        public GameObject muzzleFlash;
        public GameObject mainCharacterPrefab;
        public GameObject otherCharacterPrefab;

        // Network
        private bool _connected;
        private int _clientId;
        private const int ServerId = 0;
        public string hostname;
        public int port;
        public int listenPort;
        
        private Connection _connection;
        private IDictionary<IPAddress, ConnectionInfo> _connectionsTable;
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
            _connectionsTable = new Dictionary<IPAddress, ConnectionInfo>();
            _serverInfo = new ConnectionInfo(ServerId, hostname, port);
            _connectionsTable.Add(IPAddress.Parse(hostname), _serverInfo);
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _interpolationBuffer = new InterpolationBuffer();
            _players = new Dictionary<int, GameObject>();
            _tick = 0;
            _clientId = -1;
            _connected = false;
            _serverInfo.ConnectionRequestStream.AddToOutput(new ConnectionRequestMessage(0, ServerId));
        }

        void Update()
        {
            _packetProcessor.ProcessInput();
            if (!_connected)
            {
                List<Message> messages = _serverInfo.ConnectionResponseStream.GetMessagesReceived();
                if (messages.Count > 0)
                {
                    ConnectionResponseMessage connectionResponseMessage = (ConnectionResponseMessage) messages[0];
                    this._clientId = connectionResponseMessage.ReceiverId;
                    _connected = true;
                }
            }
            else
            {
                UpdateWorldStateWithSnapshot();
            }
            _packetProcessor.ProcessOutput();
            if (_interpolationBuffer.SynchronizeState == ClientSynchronizeState.Synchronized)
            {
                _currentTime += Time.deltaTime;
            }
            
        }

        void FixedUpdate()
        {
            if (_clientSidePredictor != null)
            {
                // Output
                PlayerInput playerInput = PlayerInput.GetPlayerInput(_tick);
                if (playerInput.Bitmap != 0 || playerInput.MouseXAxis != 0 || playerInput.MouseYAxis != 0)
                {
                    _clientSidePredictor.UpdatePlayerState(playerInput);
                    ApplyCosmeticEffects(playerInput);

                    PlayerInputMessage playerInputMessage = new PlayerInputMessage(_clientId, ServerId, playerInput);
                    _serverInfo.InputStream.AddToOutput(playerInputMessage);
                    _tick++;
                }

                var clientStateFromSnapshot = _interpolationBuffer.PollClient(_clientId, _currentTime);
                if (clientStateFromSnapshot != null)
                {
                    _clientSidePredictor.CorrectPlayerState(clientStateFromSnapshot, _interpolationBuffer.Tick);
                }
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
            WorldState worldState = _interpolationBuffer.Poll(_clientId, _currentTime);
            if (worldState != null)
            {
                foreach (var player in worldState.Players)
                {
                    if (!_players.ContainsKey(player.Key))
                    {
                        // instance new player;
                        GameObject newPlayer;
                        if (player.Key == _clientId)
                        {
                            newPlayer = Instantiate(mainCharacterPrefab, player.Value.Position, Quaternion.identity);
                            _characterController =  newPlayer.AddComponent<CharacterController>();
                            _characterController.height = 0;
                            _characterController.radius = 3;
                            _characterController.center = new Vector3(0, 1, -2);
                            _clientSidePredictor = new ClientSidePredictor(_characterController, speed);
                        }
                        else
                        {
                            newPlayer = Instantiate(otherCharacterPrefab, player.Value.Position, Quaternion.identity);
                        }
                        _players[player.Key] = newPlayer;
                    }
                    // Only remote players
                    else if (player.Key != _clientId)
                    {
                        
                        // setting interpolated position
                        _players[player.Key].transform.position = player.Value.Position;
                    }
                }
            }
        }

        private void ApplyCosmeticEffects(PlayerInput playerInput)
        {
            if(playerInput.GetKeyDown(KeyCode.Mouse0) && muzzleFlash != null)
            {
                Instantiate(muzzleFlash, _characterController.transform);
            }
        }
    }
}