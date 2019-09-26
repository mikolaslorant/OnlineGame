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
        private InterpolationBuffer _interpolationBuffer;
        private float _currentTime;

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<int, ConnectionInfo>();
            _serverInfo = new ConnectionInfo(ServerId, hostname, port);
            _connectionsTable.Add(_serverInfo.ClientId, _serverInfo);
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _interpolationBuffer = new InterpolationBuffer();
        }

        void Update()
        {
            _packetProcessor.ProcessInput();
            UpdateStateWithSnapshot();
            _packetProcessor.ProcessOutput();
            if (_interpolationBuffer.SynchronizeState == ClientSynchronizeState.Synchronized)
            {
                _currentTime += Time.deltaTime;
            }
        }

        void FixedUpdate()
        {
            // Output
            PlayerInput playerInput = PlayerInput.GetPlayerInput();
            if (playerInput.Bitmap != 0)
            {
                PlayerInputMessage playerInputMessage = new PlayerInputMessage(clientId, ServerId, playerInput);
                _serverInfo.InputStream.AddToOutput(playerInputMessage);
            }
        }
        
        private void UpdateStateWithSnapshot()
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
            PlayerState interpolatedState = _interpolationBuffer.Poll(_currentTime);
            if (interpolatedState != null)
            {
                characterController.transform.position = interpolatedState.Position; 
            }
        }
    }
}