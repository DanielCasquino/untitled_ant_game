#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Spider : MonoBehaviour
{
    public enum AnimationState
    {
        IDLE,
        MOVING
    }

    public enum BehaviourState
    {
        WANDER, // picks a waypoint every x secs
        CHASE, // follow player
        HUNT // follows player trail
    }

    [Header("Movement")]
    [SerializeField] float speed = 3.5f;
    [SerializeField] float accel = 20f;
    [SerializeField] float deccel = 30f;
    [SerializeField] float rotationSpeed = 10f;
    Rigidbody2D rb;

    [Header("Pathfinding")]
    [SerializeField] float waypointAckThreshold = 0.4f;
    [SerializeField] float waypointSeparation = 1f;
    [SerializeField] LayerMask terrainLayerMask;
    Player playerReference;
    [SerializeField] LinkedList<Vector2> waypoints = new();

    [Header("Wander Waypoint Choice")]
    [SerializeField] Timer waypointChoiceTimer;
    [SerializeField] Vector2 waypointChoiceTime = new(0.5f, 6);
    [SerializeField] Vector2 wanderDistance = new(3, 6);

    [Header("Aggro")]
    [SerializeField] Timer beforeAggroTimer;
    [SerializeField] float beforeAggroTime = 0.2f;
    [SerializeField] float aggroRadius = 7f;
    [SerializeField] CircleCollider2D aggroTrigger;
    [SerializeField] int maxBreadcrumbs = 30;
    int breadcrumbs = 0;

    [Header("Terrain Interaction")]
    [SerializeField] Transform cursor;
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
        aggroTrigger.radius = aggroRadius;
        TransitionState(BehaviourState.WANDER);
    }

    void Update()
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER:
                Wander();
                break;
            case BehaviourState.CHASE:
                Chase();
                break;
            case BehaviourState.HUNT:
                Hunt();
                break;

        }

        ComputeAnimationState();

        if (waypoints.Count == 0) return;

        Vector2 position2D = (Vector2)transform.position;
        Vector2 direction = (waypoints.First.Value - position2D).normalized;
        float angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector2 position2D = (Vector2)transform.position;
        Vector2 direction = (waypoints.First.Value - position2D).normalized;
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

    void TransitionState(BehaviourState nextState)
    {
        Vector2 playerPosition2D;
        switch (nextState)
        {
            case BehaviourState.WANDER:
                waypointChoiceTimer.SetRandomWaitTime(waypointChoiceTime); // sets random between x and y
                waypointChoiceTimer.Play();
                break;
            case BehaviourState.CHASE:
                playerPosition2D = (Vector2)playerReference.transform.position;
                breadcrumbs = maxBreadcrumbs;
                waypoints.Clear();
                waypoints.AddFirst(playerPosition2D);
                break;
            case BehaviourState.HUNT:
                playerPosition2D = (Vector2)playerReference.transform.position;
                waypoints.Clear();
                waypoints.AddFirst(playerPosition2D);
                break;
        }
        behaviourState = nextState;
    }

    public void OnWaypointChoiceTimeout()
    {
        Vector2 position2D = (Vector2)transform.position;
        Vector2 direction = Random.insideUnitCircle.normalized;
        float distance = Random.Range(wanderDistance.x, wanderDistance.y);

        while (Physics2D.Raycast(position2D, direction, distance, terrainLayerMask) || Physics2D.OverlapPoint(position2D + direction * distance, terrainLayerMask))
        {
            direction = Random.insideUnitCircle.normalized;
            distance = Random.Range(wanderDistance.x, wanderDistance.y);
        }

        waypoints.AddLast(position2D + direction * distance);
    }

    void Wander()
    {
        Vector2 position2D = (Vector2)transform.position;

        if (waypoints.Count > 0 && Vector2.Distance(waypoints.Last.Value, position2D) < waypointAckThreshold)
        {
            waypoints.RemoveFirst();
            TransitionState(BehaviourState.WANDER);
        }

        if (playerReference == null)
            return;

        Vector2 playerPosition2D = (Vector2)playerReference.transform.position;
        Vector2 playerDirection = playerPosition2D - position2D;
        float distanceToPlayer = playerDirection.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(position2D, playerDirection.normalized, distanceToPlayer, terrainLayerMask);
        if (!hit && beforeAggroTimer.paused)
            beforeAggroTimer.Play();
        else if (hit)
            beforeAggroTimer.Stop();
    }

    public void OnAggroTimeout()
    {
        beforeAggroTimer.Stop();
        waypointChoiceTimer.Stop();
        if (playerReference == null)
            return;
        TransitionState(BehaviourState.CHASE);
    }

    void Chase()
    {
        Vector2 position2D = (Vector2)transform.position;
        Vector2 playerPosition2D = (Vector2)playerReference.transform.position;
        Vector2 playerDirection = playerPosition2D - position2D;
        float playerDistance = playerDirection.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(position2D, playerDirection.normalized, playerDistance, terrainLayerMask);

        if (hit || playerDistance > aggroRadius)
        {
            TransitionState(BehaviourState.HUNT);
            return;
        }

        waypoints.Clear();
        waypoints.AddFirst(playerPosition2D);
    }

    void Hunt()
    {
        Vector2 position2D = (Vector2)transform.position;
        Vector2 playerPosition2D = (Vector2)playerReference.transform.position;
        Vector2 playerDirection = playerPosition2D - position2D;
        float playerDistance = playerDirection.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(position2D, playerDirection.normalized, playerDistance, terrainLayerMask);

        if (!hit && playerDistance <= aggroRadius)
        {
            TransitionState(BehaviourState.CHASE);
            return;
        }

        if (waypoints.Count == 0)
        {
            if (breadcrumbs > 0)
            {
                --breadcrumbs;
                waypoints.AddFirst(playerPosition2D);
            }
            else
            {
                if (playerDistance > aggroRadius)
                    playerReference = null;
                TransitionState(BehaviourState.WANDER);
                return;
            }

        }

        if (Vector2.Distance(waypoints.Last.Value, playerPosition2D) > waypointSeparation && breadcrumbs > 0)
        {
            waypoints.AddLast(playerPosition2D);
            --breadcrumbs;
        }

        if (waypoints.Count > 0 && Vector2.Distance(waypoints.First.Value, position2D) < waypointAckThreshold)
            waypoints.RemoveFirst();
    }

    public void OnPlayerEnteredAggroZone(Player _playerReference)
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER: playerReference = _playerReference; break;
            case BehaviourState.CHASE: break;
            case BehaviourState.HUNT: break;
        }
    }

    public void OnPlayerExitedAggroZone()
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER:
                playerReference = null;
                beforeAggroTimer.Stop();
                break;
            case BehaviourState.CHASE: TransitionState(BehaviourState.HUNT); break;
            case BehaviourState.HUNT: break;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        float radius = 0.3f;
        foreach (Vector2 w in waypoints)
        {
            Handles.color = w == waypoints.Last.Value ? Color.green : Color.red;
            Handles.DrawSolidDisc(w, Vector3.forward, radius);
        }
    }

    void OnDrawGizmos()
    {
        switch (behaviourState)
        {
            case BehaviourState.WANDER:
                Handles.color = playerReference ? Color.yellow : Color.green;
                break;
            case BehaviourState.CHASE:
                Handles.color = Color.red;
                break;
            case BehaviourState.HUNT:
                Handles.color = new Color(1f, 0.5f, 0f);
                break;

        }
        Handles.DrawWireDisc(transform.position, Vector3.forward, aggroRadius);
    }
#endif
}
