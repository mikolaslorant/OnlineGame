using System;
using System.Collections.Generic;
using System.Net;
using Game;
using Helpers;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Network
{
    public class Server : MonoBehaviour
    {
        private const string _serverLayer = "Server";
        // Game rules
        public float speed;
        public int initialHealth;
        public int bulletDamage;
        public GameObject characterPrefab;
        // Network
        public int listenPort;
        private Connection _connection;
        private IDictionary<IPAddress, ConnectionInfo> _connectionsTable;
        private PacketProcessor _packetProcessor;
        // Server game state
        private float _currentTime;
        private const int ServerId = 0;
        // Contains the states of the clients ordered by client Id
        private IDictionary<int, ClientRepresentation> _clientStates;

        void Start()
        {
            _connection = new Connection(listenPort);
            _connectionsTable = new Dictionary<IPAddress, ConnectionInfo>();
            _clientStates = new Dictionary<int, ClientRepresentation>();
            _packetProcessor = new PacketProcessor(_connection, _connectionsTable);
            _currentTime = 0f;
        }

        void Update()
        {
            _packetProcessor.ProcessInput();
            CheckIncomingConnectionRequests();
            BroadCastSnapshot();
            _packetProcessor.ProcessOutput();
            _currentTime += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            ApplyPlayerInputs();
        }

        private void ApplyPlayerInputs()
        {
            foreach (var connection in _connectionsTable.Values)
            {
                // Input
                List<Message> playerInputsReceived = connection.InputStream.GetMessagesReceived();
                foreach (var message in playerInputsReceived)
                {
                    var playerInput = ((PlayerInputMessage) message).PlayerInput;

                    _clientStates[connection.ClientId].UpdateClientRepresentationMovement(PlayerInput.GetMovement(playerInput, _clientStates[connection.ClientId].CharacterController) * speed,
                        new Vector3(playerInput.MouseYAxis, playerInput.MouseXAxis, 0) * speed);

                    if (playerInput.GetKeyDown(KeyCode.Mouse0))
                    {
                        ProcessAuthoritativeShoot(_clientStates[connection.ClientId].CharacterController);
                    }

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

        private void CheckIncomingConnectionRequests()
        {
            foreach (var connection in _connectionsTable.Values)
            {
                List<Message> messages = connection.ConnectionRequestStream.GetMessagesReceived();
                if (messages.Count > 0 && !_clientStates.ContainsKey(connection.ClientId))
                {
                    ConnectionRequestMessage connectionRequestMessage = (ConnectionRequestMessage) messages[0];
                    // instance new player;
                    var newPlayer = Instantiate(characterPrefab, new Vector3(0, 5, 0), Quaternion.identity);
                    var  characterController =  newPlayer.AddComponent<CharacterController>();
                    characterController.height = 0;
                    characterController.radius = 3;
                    characterController.center = new Vector3(0, 1, -2);
                    // added to the client representation map
                    _clientStates[connection.ClientId] = new ClientRepresentation(newPlayer, characterController, 
                        new PlayerState(characterController.transform.position, characterController.transform.rotation, initialHealth), 0);
                    // return connection response message to new player
                    connection.ConnectionResponseStream.AddToOutput(new ConnectionResponseMessage(ServerId, connection.ClientId));
                }
            }
        }

        private void ProcessAuthoritativeShoot(CharacterController characterShooter)
        {
            Ray rayToShoot = new Ray(characterShooter.transform.position, characterShooter.transform.forward);
            RaycastHit hit;

            if(Physics.Raycast(rayToShoot, out hit))
            {
                //Here we look if the gameobject that was hit is another client's player
                //A better approach would be to get the client id from the gameobject that was hit
                foreach (var connection in _clientStates)
                {
                    if(connection.Value.PlayerGameObject.Equals(hit.collider.gameObject))
                    {
                        //Inflict bullet damage
                        connection.Value.UpdateClientRepresentationAttributes(bulletDamage);
                    }
                }
            }
        }
    }
}
