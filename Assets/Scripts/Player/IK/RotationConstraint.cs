using UnityEngine;

public class RotationConstraint : MonoBehaviour
{
    public Transform target;
    public float maxAngle = 30f;
    public bool isContinous;
    public bool interpolating = false;
    float progress = 0f;
    public float smoothSpeed = 32f;
    float desiredZ;
    public ForeignConstraint foreignConstraint;

    void Update()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        float targetZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        if (isContinous)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZ);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
            return;
        }

        float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, targetZ);
        if (Mathf.Abs(angleDifference) > maxAngle && !interpolating && (foreignConstraint == null || foreignConstraint.Evaluate()))
        {
            interpolating = true;
            progress = 0f;
            desiredZ = targetZ;
        }

        if (interpolating)
        {
            progress += Time.deltaTime * smoothSpeed;
            float clampedProgress = Mathf.Clamp01(progress);
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, desiredZ);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, clampedProgress);
            if (clampedProgress >= 1f)
            {
                interpolating = false;
            }
        }
    }

    public void Initialize(Transform _target, float _maxAngle, bool _isContinous)
    {
        target = _target;
        maxAngle = _maxAngle;
        isContinous = _isContinous;
    }
}