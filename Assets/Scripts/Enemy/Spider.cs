#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EnemyState
{
    IDLE = 0, MOVING
}


[RequireComponent(typeof(Rigidbody2D))]
public class Spider : MonoBehaviour
{
    [SerializeField] Transform target;
    Rigidbody2D rb;
    List<Vector3> cachedPositions = new List<Vector3>();
    [SerializeField] float cacheThreshold = 1f;
    [SerializeField] float forgetThreshold = 0.1f;
    [SerializeField] float speed = 3.5f;
    [SerializeField] float accel = 20f;
    [SerializeField] float deccel = 30f;
    [SerializeField] LayerMask layerMask;
    bool hasLineOfSight = false;
    [SerializeField] Transform cursor;
    [SerializeField] float rotationSpeed = 10f;
    EnemyState state = EnemyState.IDLE;
    public UnityEvent<EnemyState> whenStateChanged;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (target == null)
            return;

        Vector3 playerPos = Player.instance.transform.position;
        Vector3 dir = (playerPos - transform.position).normalized;
        float distanceToPlayer = (playerPos - transform.position).magnitude;
        hasLineOfSight = !Physics2D.Raycast(transform.position, dir, distanceToPlayer, layerMask);

        if (hasLineOfSight)
        {
            cachedPositions.Clear();
            cachedPositions.Add(playerPos);
        }

        if (cachedPositions.Count == 0)
            return;

        float separation = (playerPos - cachedPositions[cachedPositions.Count - 1]).magnitude;

        if (cachedPositions.Count > 0 && separation > cacheThreshold)
            cachedPositions.Add(playerPos);

        Vector2 dirToCache = cachedPositions[0] - transform.position;

        if (dirToCache.magnitude < forgetThreshold)
            cachedPositions.RemoveAt(0);

        float angle = Mathf.Atan2(dirToCache.normalized.y, dirToCache.normalized.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // ty imphenzia
    }

    void FixedUpdate()
    {
        if (cachedPositions.Count == 0)
            return;
        Vector3 dir = (cachedPositions[0] - transform.position).normalized;
        Vector2 targetVelocity = dir.normalized * speed;
        float fac = (dir.magnitude > 0.01f) ? accel : deccel;
        Vector2 velocityDelta = targetVelocity - rb.linearVelocity;

        Vector2 force = Vector2.ClampMagnitude(velocityDelta * rb.mass / Time.fixedDeltaTime, fac * rb.mass);

        rb.AddForce(force);


        EnemyState newState = GetState();
        if (state == newState)
            return;

        whenStateChanged?.Invoke(newState);
        state = newState;
    }

    EnemyState GetState()
    {
        if (rb.linearVelocity.magnitude > 0)
            return EnemyState.MOVING;
        else
            return EnemyState.IDLE;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (hasLineOfSight)
            Gizmos.DrawLine(transform.position, Player.instance.transform.position);

        float radius = 0.35f;
        for (int i = 0; i < cachedPositions.Count; ++i)
        {
            Gizmos.color = i == cachedPositions.Count - 1 ? Color.green : Color.red;


            Handles.color = Gizmos.color;
            Handles.DrawSolidDisc(cachedPositions[i], Vector3.forward, radius);
        }
    }
#endif
}
