using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerInput _playerInput;

    public Vector2 directionInput;
    public bool driftPressed;
    public bool driftReleased;
    public bool itemPressed;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        directionInput = new Vector2(_playerInput.actions["Steer"].ReadValue<float>(), _playerInput.actions["Move"].ReadValue<float>());
        driftPressed = _playerInput.actions["Drift"].WasPressedThisFrame();
        driftReleased = _playerInput.actions["Drift"].WasReleasedThisFrame();
        itemPressed = _playerInput.actions["Item"].WasPressedThisFrame();
    }
}
