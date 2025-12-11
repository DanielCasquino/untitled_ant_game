using UnityEngine;

public class Centipede : MonoBehaviour
{
    [SerializeField] GameObject segmentPrefab;
    [SerializeField] int segments = 5;
    [SerializeField] float segmentDistance = 0.5f;

    void Start()
    {
        Transform t = transform;
        for (int i = 0; i < segments; ++i)
        {
            Vector3 offset = t.position + Vector3.left * (i + 1) * segmentDistance;
            GameObject instance = Instantiate(segmentPrefab, offset, Quaternion.identity, null);
            DistanceConstraint dc = instance.GetComponent<DistanceConstraint>();
            dc.target = t;
            dc.distance = segmentDistance;
            t = instance.transform;
        }
    }
}
