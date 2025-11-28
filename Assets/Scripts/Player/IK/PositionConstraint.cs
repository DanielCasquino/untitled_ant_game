using UnityEngine;

public class PositionConstraint : MonoBehaviour
{
    public Transform target;
    public float maxDistance;
    public bool isContinous;
    public bool interpolating = false;
    float progress = 0f;
    public float smoothSpeed = 32f;
    Vector3 desiredPosition;


    void Update()
    {
        if (target == null)
            return;

        if (isContinous)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * smoothSpeed);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > maxDistance && !interpolating)
        {
            interpolating = true;
            progress = 0f;
            desiredPosition = target.position;
        }
        if (interpolating)
        {
            progress += Time.deltaTime * smoothSpeed;
            float clampedProgress = Mathf.Clamp01(progress);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, clampedProgress);
            if (clampedProgress >= 1f)
            {
                interpolating = false;
            }
        }
    }

    public void Initialize(Transform _target, float _maxDistance, bool _isContinous)
    {
        target = _target;
        maxDistance = _maxDistance;
        isContinous = _isContinous;
    }
}