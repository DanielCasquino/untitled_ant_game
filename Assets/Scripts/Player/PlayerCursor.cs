using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    public Vector2 inputAxis { get; private set; }
    [field: SerializeField] public float radius { get; private set; } = 0.5f;
    Transform parentTransform;
    Vector2 lastNonZeroInput = Vector2.up;

    void Start()
    {
        parentTransform = gameObject.GetComponentInParent<Transform>();
        transform.position = parentTransform.position + new Vector3(0, radius, 0);
    }


    void Update()
    {
        if (inputAxis != Vector2.zero)
        {
            lastNonZeroInput = inputAxis.normalized;
        }

        Vector3 targetOffset = new Vector3(lastNonZeroInput.x, lastNonZeroInput.y, 0) * radius;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, Time.deltaTime * 10f);
    }

    public void SetInputAxis(Vector2 v)
    {
        inputAxis = v;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}