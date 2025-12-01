using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerCursor playerCursor;
    PlayerInput playerInput;
    Rigidbody2D rb;
    public UnityEvent<Vector3> whenPlayerDigged;
    public UnityEvent<Vector3> whenPlayerFilled;
    [SerializeField] float speed = 4f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        playerInput.whenPlayerPressedDig += OnPlayerDigged;
        playerInput.whenPlayerPressedFill += OnPlayerFilled;
    }

    void OnDisable()
    {
        playerInput.whenPlayerPressedDig -= OnPlayerDigged;
        playerInput.whenPlayerPressedFill -= OnPlayerFilled;
    }

    void FixedUpdate()
    {
        Vector2 m = playerInput.playerMovement;
        playerCursor.SetInputAxis(m);
        rb.MovePosition(rb.position + new Vector2(m.x, m.y) * Time.fixedDeltaTime * speed);
    }

    public void OnPlayerDigged()
    {
        whenPlayerDigged?.Invoke(playerCursor.transform.position);
        World.Instance.ModifyTerrain(playerCursor.transform.position, false);
    }

    public void OnPlayerFilled()
    {
        whenPlayerFilled?.Invoke(playerCursor.transform.position);
        World.Instance.ModifyTerrain(playerCursor.transform.position, true);
    }
}
