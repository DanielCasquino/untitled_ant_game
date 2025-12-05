using UnityEngine;
using UnityEngine.Events;

public enum PlayerState
{
    IDLE = 0, MOVING
}

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    [SerializeField] PlayerCursor cursor;
    [SerializeField] PlayerInput input;
    Rigidbody2D rb;
    public UnityEvent<Vector3> whenDigged;
    public UnityEvent<Vector3> whenFilled;
    public UnityEvent<PlayerState> whenStateChanged;
    [SerializeField] float speed = 4f;
    PlayerState state = PlayerState.IDLE;
    [SerializeField] float accel = 20f;
    [SerializeField] float deccel = 30f;
    public int health;
    public UnityEvent playerDamaged;
    public UnityEvent playerDied;

    public bool isInvis = false;
    float invisTime = 3f;
    [SerializeField] Timer invisTimer;
    public SoundController sound;

    public void Damage()
    {
        if (isInvis)
            return;

        health--;
        if (health > 0)
        {
            sound.DañoAnt();
            playerDamaged?.Invoke();
            isInvis = true;
            invisTimer.Play();
            Debug.Log("Player damaged");
        }
        else
        {
            sound.DeadAnt();
            playerDied?.Invoke();
            Debug.Log("Player died");
        }
    }

    void OnInvisTimerTimeout()
    {
        isInvis = false;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        invisTimer.Initialize(false, invisTime);
    }

    void OnEnable()
    {
        invisTimer.whenTimeout.AddListener(OnInvisTimerTimeout);
        input.whenPressedDig += OnDig;
        input.whenPressedFill += OnFill;
    }

    void OnDisable()
    {
        invisTimer.whenTimeout.RemoveListener(OnInvisTimerTimeout);
        input.whenPressedDig -= OnDig;
        input.whenPressedFill -= OnFill;
    }

    void Update()
    {
        Vector2 m = input.playerMovement;
        cursor.SetInputAxis(m);
    }

    void FixedUpdate()
    {
        Vector2 m = input.playerMovement;
        Vector2 targetVelocity = m.normalized * speed;
        Vector2 velocityDelta = targetVelocity - rb.linearVelocity;
        float fac = (m.magnitude > 0.01f) ? accel : deccel;
        Vector2 force = Vector2.ClampMagnitude(velocityDelta * rb.mass / Time.fixedDeltaTime, fac * rb.mass);
        rb.AddForce(force);

        PlayerState newState = GetState();

        if (state == newState)
            return;

        whenStateChanged?.Invoke(newState);
        state = newState;
    }

    public void OnDig()
    {
        whenDigged?.Invoke(cursor.transform.position);
        World.instance.ModifyTerrain(cursor.transform.position, false);
    }

    public void OnFill()
    {
        whenFilled?.Invoke(cursor.transform.position);
        World.instance.ModifyTerrain(cursor.transform.position, true);
    }

    PlayerState GetState()
    {
        if (rb.linearVelocity.magnitude > 0){
            sound.RunAnt();
            return PlayerState.MOVING;
        }
        else
            return PlayerState.IDLE;
    }
}
