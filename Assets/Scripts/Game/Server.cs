using System;
using System.Collections.Generic;
using Game;
using Helpers;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Network
{
    public class Server : MonoBehaviour
    {
        // Game rules
        public List<CharacterController> characterControllers;
        public float speed;

        // Network
        public int listenPort;
        private Connection _connection;
        private IDictionary<int, ConnectionInfo> _connectionsTable;
        private PacketProcessor _packetProcessor;
        // Server game state
        private float _currentTime;
        private const int ServerId = 0;
        // Contains the states of the clients ordered by client Id
        private IDictionary<int, ClientRepresentationOnServer> _clientStates;

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<int, ConnectionInfo>();
            _clientStates = new Dictionary<int, ClientRepresentationOnServer>();
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _connectionsTable.Add(1, new ConnectionInfo(1, "localhost", 2000));
            // Adding client with Id 1 state
            _clientStates.Add(1, 
                new ClientRepresentationOnServer(characterControllers[0], new PlayerState(characterControllers[0].transform.position), 0));
            _currentTime = 0f;
        }

        void Update()
        {
            _packetProcessor.ProcessInput();
            
            BroadCastSnapshot();
            _packetProcessor.ProcessOutput();
            // TODO: check time deltas. Deberia estar asociado al momento de la simulación o al momento en que se manda el paquete.
            _currentTime += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            ApplyPlayerMovements();
        }

        private void ApplyPlayerMovements()
        {
            foreach (var connection in _connectionsTable.Values)
            {
                // Input
                List<Message> playerInputsReceived = connection.InputStream.GetMessagesReceived();
                foreach (var message in playerInputsReceived)
                {
                    var playerInput = ((PlayerInputMessage) message).PlayerInput;
                    _clientStates[connection.ClientId].UpdateClientRepresentationOnServer(PlayerInput.GetMovement(playerInput) * speed);
                    _clientStates[connection.ClientId].Tick = playerInput.Tick;
                }
            }
        }

        private void BroadCastSnapshot()
        {
            foreach (var connection in _connectionsTable.Values)
            {
                // corresponding tick
                int tick = _clientStates[connection.ClientId].Tick;
                // player other than client
                WorldState worldState = new WorldState();
                foreach (var clientState in _clientStates)
                {
                    worldState.Players[clientState.Key] = clientState.Value.PlayerState;
                }
                SnapshotMessage snapshotMessage = new SnapshotMessage(ServerId, connection.ClientId, worldState, tick, _currentTime);
                connection.SnapshotStream.AddToOutput(snapshotMessage);
            }
        }
    }
}
