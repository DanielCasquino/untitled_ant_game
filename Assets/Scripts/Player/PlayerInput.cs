using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public InputMap inputMap;
    public Vector2 playerMovement { get; private set; }
    public Vector2 mousePosition { get; private set; }

    public System.Action whenPlayerLeftClicked;
    public System.Action whenPlayerRightClicked;

    void Awake()
    {
        inputMap = new InputMap();
    }

    void OnEnable()
    {
        inputMap.Enable();
        inputMap.PlayerMouse.LeftClick.started += OnPlayerLeftClicked;
        inputMap.PlayerMouse.RightClick.started += OnPlayerRightClicked;
    }

    void OnDisable()
    {
        inputMap.PlayerMouse.LeftClick.started -= OnPlayerLeftClicked;
        inputMap.PlayerMouse.RightClick.started -= OnPlayerRightClicked;
        inputMap.Disable();
    }

    void Update()
    {
        mousePosition = inputMap.PlayerMouse.Position.ReadValue<Vector2>();
        playerMovement = inputMap.PlayerMovement.Movement.ReadValue<Vector2>();
    }

    void OnPlayerLeftClicked(InputAction.CallbackContext context)
    {
        whenPlayerLeftClicked?.Invoke();
    }

    void OnPlayerRightClicked(InputAction.CallbackContext context)
    {
        whenPlayerRightClicked?.Invoke();
    }
}
