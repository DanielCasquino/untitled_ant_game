using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    Vector3 velocity = Vector3.zero;
    public float smoothTime = 0.1f;
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(target.position.x, target.position.y, transform.position.z), ref velocity, smoothTime);
    }
}
