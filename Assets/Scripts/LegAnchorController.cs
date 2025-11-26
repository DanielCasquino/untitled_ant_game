using UnityEngine;

public class LegAnchorController : MonoBehaviour
{
    [SerializeField] Transform[] anchors;
    Transform[] worldAnchors;
    [SerializeField] RotationConstraint[] legs;
    [SerializeField] float[] maxDistances;

    void Start()
    {
        worldAnchors = new Transform[anchors.Length];
        for (int i = 0; i < anchors.Length; i++)
        {
            GameObject worldAnchor = new GameObject(anchors[i].name + "_WORLD");
            worldAnchor.transform.position = anchors[i].position;
            worldAnchor.AddComponent<PositionConstraint>().Initialize(anchors[i], maxDistances[i], false);
            worldAnchors[i] = worldAnchor.transform;
            legs[i].target = worldAnchors[i];
        }
    }
}
