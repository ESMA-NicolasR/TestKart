using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Exposed inputs")]
    public Vector2 directionInput;
    public bool driftPressed;
    public bool driftReleased;
    public bool itemPressed;
    
    [Header("Controls")]
    public string controlScheme;
    
    // Internal components
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        // Unity disconnects the control scheme for local multiplayer on the same device
        _playerInput.SwitchCurrentControlScheme(controlScheme, Keyboard.current);
    }

    void Update()
    {
        // Read inputs
        directionInput = _playerInput.actions["Move"].ReadValue<Vector2>();
        driftPressed = _playerInput.actions["Drift"].WasPressedThisFrame();
        driftReleased = _playerInput.actions["Drift"].WasReleasedThisFrame();
        itemPressed = _playerInput.actions["Item"].WasPressedThisFrame();
    }
}
