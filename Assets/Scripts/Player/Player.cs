using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerCursor playerCursor;
    PlayerInput playerInput;
    Rigidbody rb;
    public UnityEvent<Vector3> whenPlayerDigged;
    public UnityEvent<Vector3> whenPlayerFilled;
    [SerializeField] float speed = 4f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        playerInput.whenPlayerLeftClicked += OnPlayerLeftClicked;
        playerInput.whenPlayerRightClicked += OnPlayerRightClicked;
    }

    void OnDisable()
    {
        playerInput.whenPlayerLeftClicked -= OnPlayerLeftClicked;
        playerInput.whenPlayerRightClicked -= OnPlayerRightClicked;
    }

    void FixedUpdate()
    {
        playerCursor.SetMousePosition(playerInput.mousePosition);

        Vector2 m = playerInput.playerMovement;
        rb.MovePosition(rb.position + new Vector3(m.x, m.y, 0) * Time.fixedDeltaTime * speed);
    }

    public void OnPlayerLeftClicked()
    {
        whenPlayerDigged?.Invoke(playerCursor.transform.position);
    }

    public void OnPlayerRightClicked()
    {
        whenPlayerFilled?.Invoke(playerCursor.transform.position);
    }

}
