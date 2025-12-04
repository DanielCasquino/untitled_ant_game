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

    bool isAggro;
    // once sees player
    [SerializeField] Timer aggroTimer;
    [SerializeField] float aggroTime = 2f;
    [SerializeField] float aggroRange = 5f;
    // once loses los
    [SerializeField] Timer deAggroTimer;
    [SerializeField] float deAggroTime = 5f;
    [SerializeField] int maxCachedPositions = 10;

    [SerializeField] Timer wanderTimer;
    [SerializeField] float wanderInterval = 3f;
    [SerializeField] float wanderRadius = 5f;
    Vector3 wanderTarget;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aggroTimer.Initialize(false, aggroTime);
        deAggroTimer.Initialize(false, deAggroTime);
        wanderTimer.Initialize(true, wanderInterval);
        PickNewWanderTarget();
    }

    void OnEnable()
    {
        deAggroTimer.whenTimeout.AddListener(OnDeAggroTimerTimeout);
        aggroTimer.whenTimeout.AddListener(OnAggroTimerTimeout);
        wanderTimer.whenTimeout.AddListener(PickNewWanderTarget);
    }

    void OnDisable()
    {
        deAggroTimer.whenTimeout.RemoveListener(OnDeAggroTimerTimeout);
        aggroTimer.whenTimeout.RemoveListener(OnAggroTimerTimeout);
        wanderTimer.whenTimeout.RemoveListener(PickNewWanderTarget);
    }

    void Update()
    {
        Vector3 playerPos = Player.instance.transform.position;
        Vector3 dir = (playerPos - transform.position).normalized;
        float playerDistance = (playerPos - transform.position).magnitude;
        hasLineOfSight = !Physics2D.Raycast(transform.position, dir, playerDistance, layerMask);

        if (isAggro)
        {
            ChaseLogic(playerPos, playerDistance);
        }
        else
        {
            WanderLogic(playerDistance);
        }
    }

    void ChaseLogic(Vector3 playerPos, float playerDistance)
    {
        if (playerDistance <= aggroRange && hasLineOfSight)
        {
            deAggroTimer.Stop();
            cachedPositions.Clear();
            cachedPositions.Add(playerPos);
        }
        else
        {
            if (deAggroTimer.paused)
                deAggroTimer.Play();
        }

        if (cachedPositions.Count > 0)
        {
            Vector2 dirToCache = cachedPositions[0] - transform.position;
            if (dirToCache.magnitude < forgetThreshold)
                cachedPositions.RemoveAt(0);

            if (cachedPositions.Count > 0)
            {
                float angle = Mathf.Atan2(dirToCache.normalized.y, dirToCache.normalized.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (!hasLineOfSight)
        {
            isAggro = false;
            deAggroTimer.Stop();
            aggroTimer.Stop();
        }
    }
    // ty imphenzia

    void WanderLogic(float playerDistance)
    {
        if (playerDistance <= aggroRange && hasLineOfSight)
        {
            if (aggroTimer.paused)
                aggroTimer.Play();
        }
        else
        {
            aggroTimer.Stop();
        }
    }

    void OnDeAggroTimerTimeout()
    {
        isAggro = false;
        cachedPositions.Clear();
        PickNewWanderTarget();
    }

    void OnAggroTimerTimeout()
    {
        isAggro = true;
        cachedPositions.Clear();
        cachedPositions.Add(Player.instance.transform.position);
        deAggroTimer.Stop();
    }

    void PickNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        while (Physics2D.Raycast(transform.position, randomCircle.normalized, randomCircle.magnitude, layerMask))
        {
            randomCircle = Random.insideUnitCircle * wanderRadius;
        }
        wanderTarget = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);
        cachedPositions.Clear();
        cachedPositions.Add(wanderTarget);
    }

    void FixedUpdate()
    {
        if (cachedPositions.Count == 0)
        {
            PickNewWanderTarget();
            return;
        }

        if (!isAggro && (cachedPositions[0] - transform.position).magnitude < forgetThreshold)
        {
            PickNewWanderTarget();
        }

        Vector3 dir = (cachedPositions[0] - transform.position).normalized;

        if (!isAggro && dir.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

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
        else
        {
            Gizmos.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.forward, aggroRange);
        }

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
