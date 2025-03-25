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
    public string controlScheme;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        // Unity disconnects the control scheme for local multiplayer on the same device
        _playerInput.SwitchCurrentControlScheme(controlScheme, new []{Keyboard.current});
    }

    // Update is called once per frame
    void Update()
    {
        directionInput = _playerInput.actions["Move"].ReadValue<Vector2>();
        driftPressed = _playerInput.actions["Drift"].WasPressedThisFrame();
        driftReleased = _playerInput.actions["Drift"].WasReleasedThisFrame();
        itemPressed = _playerInput.actions["Item"].WasPressedThisFrame();
    }
}
