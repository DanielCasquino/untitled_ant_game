using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public InputMap inputMap;
    public Vector2 playerMovement { get; private set; }

    public System.Action whenPressedDig;
    public System.Action whenPressedFill;

    void Awake()
    {
        inputMap = new InputMap();
    }

    void OnEnable()
    {
        inputMap.Enable();
        inputMap.Player.Dig.started += OnPressedDig;
        inputMap.Player.Fill.started += OnPressedFill;
    }

    void OnDisable()
    {
        inputMap.Player.Dig.started -= OnPressedDig;
        inputMap.Player.Fill.started -= OnPressedFill;
        inputMap.Disable();
    }

    void Update()
    {
        playerMovement = inputMap.Player.Movement.ReadValue<Vector2>();
    }

    void OnPressedDig(InputAction.CallbackContext context)
    {
        whenPressedDig?.Invoke();
    }

    void OnPressedFill(InputAction.CallbackContext context)
    {
        whenPressedFill?.Invoke();
    }
}
