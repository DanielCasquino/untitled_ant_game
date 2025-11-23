using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    PlayerInput playerInput;
    Rigidbody2D rb;
    [SerializeField] Transform digPoint;

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
        playerInput.whenPlayerLeftClicked += OnPlayerLeftClicked;
        playerInput.whenPlayerRightClicked += OnPlayerRightClicked;
    }

    void OnDisable()
    {
        playerInput.whenPlayerLeftClicked -= OnPlayerLeftClicked;
        playerInput.whenPlayerRightClicked -= OnPlayerRightClicked;
    }

    void Update()
    {
        Vector2 mousePos = playerInput.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        Debug.DrawLine(transform.position, worldPos, Color.red);

        Vector3 direction = worldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);


    }

    void FixedUpdate()
    {
        Vector2 movement = playerInput.playerMovement;
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime * speed);
    }

    public void OnPlayerLeftClicked()
    {
        whenPlayerDigged?.Invoke(digPoint.position);
    }

    public void OnPlayerRightClicked()
    {
        whenPlayerFilled?.Invoke(digPoint.position);
    }

}
