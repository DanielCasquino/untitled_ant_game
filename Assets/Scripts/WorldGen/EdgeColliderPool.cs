using System.Collections.Generic;
using UnityEngine;

public class EdgeColliderPool : MonoBehaviour
{
    int poolSize = 36; // hehe
    List<EdgeCollider2D> colliderPool;
    int currentIndex = 0;
    [SerializeField] LayerMask layerMask;

    void Awake()
    {
        colliderPool = new List<EdgeCollider2D>(poolSize);
        for (int i = 0; i < poolSize; ++i)
        {
            GameObject colliderObj = new GameObject("EdgeCollider_" + colliderPool.Count);
            int layerNumber = 0;
            int layerMaskValue = layerMask.value;
            while (layerMaskValue > 0 && (layerMaskValue & 1) == 0)
            {
                layerNumber++;
                layerMaskValue = layerMaskValue >> 1;
            }
            colliderObj.layer = layerNumber;
            colliderObj.transform.parent = transform;
            EdgeCollider2D edgeCollider = colliderObj.AddComponent<EdgeCollider2D>();
            edgeCollider.enabled = false;
            colliderPool.Add(edgeCollider);
        }
    }

    public void Reset()
    {
        DisableAllColliders();
        currentIndex = 0;
    }

    public void DisableAllColliders()
    {
        foreach (var collider in colliderPool)
            collider.enabled = false;
    }

    public EdgeCollider2D GetNextCollider()
    {
        if (currentIndex >= colliderPool.Count)
            return null;

        EdgeCollider2D collider = colliderPool[currentIndex];
        currentIndex++;
        return collider;
    }
}