using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public InputMap inputMap;
    public Vector2 playerMovement { get; private set; }

    public System.Action whenPlayerPressedDig;
    public System.Action whenPlayerPressedFill;

    void Awake()
    {
        inputMap = new InputMap();
    }

    void OnEnable()
    {
        inputMap.Enable();
        inputMap.Player.Dig.started += OnPlayerPressedDig;
        inputMap.Player.Fill.started += OnPlayerPressedFill;
    }

    void OnDisable()
    {
        inputMap.Player.Dig.started -= OnPlayerPressedDig;
        inputMap.Player.Fill.started -= OnPlayerPressedFill;
        inputMap.Disable();
    }

    void Update()
    {
        playerMovement = inputMap.Player.Movement.ReadValue<Vector2>();
    }

    void OnPlayerPressedDig(InputAction.CallbackContext context)
    {
        whenPlayerPressedDig?.Invoke();
    }

    void OnPlayerPressedFill(InputAction.CallbackContext context)
    {
        whenPlayerPressedFill?.Invoke();
    }
}
