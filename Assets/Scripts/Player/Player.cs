using UnityEngine;
using UnityEngine.Events;

public enum PlayerState
{
    IDLE = 0, MOVING
}

public class Player : MonoBehaviour
{
    [SerializeField] PlayerCursor cursor;
    [SerializeField] PlayerInput input;
    Rigidbody2D rb;
    public UnityEvent<Vector3> whenDigged;
    public UnityEvent<Vector3> whenFilled;
    public UnityEvent<PlayerState> whenStateChanged;
    [SerializeField] float speed = 4f;
    PlayerState state = PlayerState.IDLE;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        input.whenPressedDig += OnDig;
        input.whenPressedFill += OnFill;
    }

    void OnDisable()
    {
        input.whenPressedDig -= OnDig;
        input.whenPressedFill -= OnFill;
    }

    void FixedUpdate()
    {
        Vector2 m = input.playerMovement;
        cursor.SetInputAxis(m);
        // rb.MovePosition(rb.position + new Vector2(m.x, m.y) * Time.fixedDeltaTime * speed);

        float acceleration = 20f;
        float deceleration = 30f;

        Vector2 desiredVelocity = m.normalized * speed;
        Vector2 velocityChange = desiredVelocity - rb.linearVelocity;

        float factor = (m.magnitude > 0.01f) ? acceleration : deceleration;
        Vector2 force = Vector2.ClampMagnitude(velocityChange * rb.mass / Time.fixedDeltaTime, factor * rb.mass);

        rb.AddForce(force);

        PlayerState newState = GetState();

        if (state != newState)
        {
            Debug.Log("Player state changed to: " + newState);
            whenStateChanged?.Invoke(newState);
            state = newState;
        }
    }

    public void OnDig()
    {
        whenDigged?.Invoke(cursor.transform.position);
        World.Instance.ModifyTerrain(cursor.transform.position, false);
    }

    public void OnFill()
    {
        whenFilled?.Invoke(cursor.transform.position);
        World.Instance.ModifyTerrain(cursor.transform.position, true);
    }

    PlayerState GetState()
    {
        if (rb.linearVelocity.magnitude > 0)
            return PlayerState.MOVING;
        else
            return PlayerState.IDLE;
    }
}
