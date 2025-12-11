using UnityEngine;

public class DistanceConstraint : MonoBehaviour
{
    public Transform target;
    public float distance = 0.5f;

    void Update()
    {
        if (!target)
            return;
        Vector2 position2D = (Vector2)transform.position;
        Vector2 targetPosition2D = (Vector2)target.position;
        Vector2 direction = (position2D - targetPosition2D).normalized;
        transform.position = targetPosition2D + direction * distance;
    }
}
