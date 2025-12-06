#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AnimationState
{
    IDLE = 0, MOVING
}

public enum BehaviourState
{
    WANDER, AGGRO
}


[RequireComponent(typeof(Rigidbody2D))]
public class Spider : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 3.5f;
    [SerializeField] float accel = 20f;
    [SerializeField] float deccel = 30f;
    [SerializeField] float rotationSpeed = 10f;
    Rigidbody2D rb;

    [Header("Pathfinding")]
    [SerializeField] float waypointAckThreshold = 0.1f;
    [SerializeField] float waypointRecordThreshold = 1f;
    [SerializeField] LayerMask terrainLayerMask;
    [field: SerializeField] public Player playerReference { get; private set; }
    [SerializeField] List<Vector2> waypoints = new List<Vector2>();
    bool hasLineOfSight = false;

    [Header("Wander State")]
    [SerializeField] Timer waypointChoiceTimer;
    [SerializeField] Vector2 waypointChoiceTime = new Vector2(2, 4);
    [SerializeField] Vector2 wanderDistance = new Vector2(1, 4);

    [Header("Aggro State")]
    [SerializeField] Timer beforeAggroTimer;
    [SerializeField] float beforeAggroTime = 1f;

    [Header("Terrain Interaction")]
    [SerializeField] Transform cursor;

    // behaviour
    AnimationState animationState = AnimationState.IDLE;
    BehaviourState behaviourState = BehaviourState.WANDER;

    [Header("Events")]
    public UnityEvent<AnimationState> whenAnimationStateChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        beforeAggroTimer.SetWaitTime(beforeAggroTime);
    }

    void Start()
    {
        OnWander();
    }

    void Update()
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER:
                Wander();
                break;
            case BehaviourState.AGGRO:
                Aggro();
                break;
        }

        ComputeAnimationState();

        if (waypoints.Count == 0) return;

        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (waypoints[0] - position2D).normalized;

        float angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // tldr: always moving towards waypoint 0
        if (waypoints.Count == 0) return;

        // update pos
        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (waypoints[0] - position2D).normalized;

        Vector2 targetVelocity = direction.normalized * speed;
        float fac = (direction.magnitude > 0.01f) ? accel : deccel;
        Vector2 velocityDelta = targetVelocity - rb.linearVelocity;
        Vector2 force = Vector2.ClampMagnitude(velocityDelta * rb.mass / Time.fixedDeltaTime, fac * rb.mass);
        rb.AddForce(force);
    }

    void ComputeAnimationState()
    {
        AnimationState nextAnimationState;

        if (rb.linearVelocity.magnitude > 0)
            nextAnimationState = AnimationState.MOVING;
        else
            nextAnimationState = AnimationState.IDLE;

        if (animationState == nextAnimationState)
            return;

        whenAnimationStateChanged?.Invoke(nextAnimationState);
        animationState = nextAnimationState;
    }
    #region Wander
    void Wander()
    {
        if (waypoints.Count == 0) return;

        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);

        if (Vector2.Distance(waypoints[0], position2D) < waypointAckThreshold)
        {
            waypoints.RemoveAt(0);
            rb.linearVelocity = Vector2.zero;
            OnWander();
        }
    }

    void OnWander()
    {
        if (waypointChoiceTimer.paused)
        {
            waypointChoiceTimer.SetRandomWaitTime(waypointChoiceTime); // sets random between x and y
            waypointChoiceTimer.Play();
        }
    }

    public void OnWaypointChoiceTimeout()
    {
        Vector2 randomWaypoint = new Vector2(wanderDistance.x, wanderDistance.x) + Random.insideUnitCircle * wanderDistance.y;
        while (Physics2D.Raycast(transform.position, randomWaypoint, Vector2.Distance(transform.position, randomWaypoint), terrainLayerMask))
            randomWaypoint = Random.insideUnitCircle * wanderDistance;
        waypoints.Add(randomWaypoint);
    }
    #endregion
    #region Aggro
    public void OnPlayerEnteredAggroZone(Player _playerReference)
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER:
                {
                    playerReference = _playerReference;
                    Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
                    Vector2 playerPosition2D = new Vector2(playerReference.transform.position.x, playerReference.transform.position.y);
                    Vector2 playerDirection = playerPosition2D - position2D;
                    float distanceToPlayer = playerDirection.magnitude;
                    playerDirection.Normalize();

                    if (Physics2D.Raycast(transform.position, playerDirection, distanceToPlayer, terrainLayerMask))
                        break;

                    beforeAggroTimer.Play();
                    break;
                }

        }
    }

    public void OnPlayerExitedAggroZone()
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER:
                beforeAggroTimer.Stop();
                break;
            case BehaviourState.AGGRO:
                playerReference = null;
                break;
        }
    }

    void Aggro()
    {
        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPosition2D = new Vector2(playerReference.transform.position.x, playerReference.transform.position.y);
        Vector2 playerDirection = playerPosition2D - position2D;
        float distanceToPlayer = playerDirection.magnitude;
        playerDirection.Normalize();

        if (!Physics2D.Raycast(transform.position, playerDirection, distanceToPlayer, terrainLayerMask))
        {
            waypoints.Clear();
            waypoints.Add(playerPosition2D);
        }
        else
        {
            if (Vector2.Distance(waypoints[waypoints.Count - 1], playerPosition2D) > waypointRecordThreshold)
            {
                waypoints.Add(playerPosition2D);
            }

            if (Vector2.Distance(waypoints[0], position2D) < waypointAckThreshold)
            {
                waypoints.RemoveAt(0);
            }
        }
    }

    void OnAggro()
    {
        waypoints.Clear();
    }

    public void OnAggroTimeout()
    {
        behaviourState = BehaviourState.AGGRO;
        OnAggro();
    }
    #endregion

    public void SetPlayerReference(Player _playerReference)
    {
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        float radius = 0.35f;
        for (int i = 0; i < waypoints.Count; ++i)
        {
            Gizmos.color = i == waypoints.Count - 1 ? Color.green : Color.red;

            Handles.color = Gizmos.color;
            Handles.DrawSolidDisc(waypoints[i], Vector3.forward, radius);
        }
    }
#endif
}
