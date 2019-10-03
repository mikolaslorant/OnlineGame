using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class ClientRepresentationOnServer
{

    private CharacterController _characterController;

    private PlayerState _playerState;

    public ClientRepresentationOnServer(CharacterController characterController, PlayerState playerState)
    {
        _characterController = characterController;
        _playerState = playerState;
    }

    public void UpdateClientRepresentationOnServer(Vector3 newControllerPosition, long sequenceNumber)
    {
        _characterController.Move(newControllerPosition);
        _playerState = new PlayerState(characterController.transform.position, sequenceNumber);
    }

    public void UpdateClientRepresentationOnServer(Vector3 newControllerPosition)
    {
        _characterController.Move(newControllerPosition);
        _playerState = new PlayerState(characterController.transform.position, _playerState.SequenceNumber);
    }
}
