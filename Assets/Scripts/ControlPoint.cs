using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    [SerializeField] float smoothSpeed = 0.2f;
    [Header("Rotation Constraints")]
    [SerializeField] bool constraintRotation;
    [SerializeField] bool isRotationTrackingContinuous;
    [SerializeField] Transform rotationTarget;
    [SerializeField] float angle;

    [Header("Position Constraints")]
    [SerializeField] bool constraintPosition;
    [SerializeField] bool isPositionTrackingContinuous;
    [SerializeField] Transform positionTarget;
    [SerializeField] float distance;

    bool interpolateInProgress = false;
    float interpolateProgress = 0f;

    void Update()
    {
        if (constraintRotation)
            UpdateRotation();
        if (constraintPosition)
            UpdatePosition();
    }

    void UpdateRotation()
    {
        if (rotationTarget == null)
            return;

        Vector3 direction = rotationTarget.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float desiredZ = targetAngle + angle - 90f;

        // Smoothly interpolate the rotation
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, desiredZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    void UpdatePosition()
    {
        if (positionTarget == null)
            return;

        Vector3 direction = (transform.position - positionTarget.position).normalized;
        transform.position = positionTarget.position + direction * distance;

    }
}
