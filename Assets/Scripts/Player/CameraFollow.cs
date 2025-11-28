using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Rigidbody target;
    Vector3 velocity = Vector3.zero;
    public float smoothTime = 0.1f;

    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
