using System.Collections.Generic;
using UnityEngine;

public class EdgeColliderPool : MonoBehaviour
{
    [SerializeField] int poolSize = 10;
    List<EdgeCollider2D> colliderPool;
    int currentIndex = 0;

    void Awake()
    {
        colliderPool = new List<EdgeCollider2D>(poolSize);
        for (int i = 0; i < poolSize; ++i)
        {
            GameObject colliderObj = new GameObject("EdgeCollider_" + colliderPool.Count);
            colliderObj.transform.parent = transform;
            EdgeCollider2D edgeCollider = colliderObj.AddComponent<EdgeCollider2D>();
            edgeCollider.enabled = false;
            colliderPool.Add(edgeCollider);
        }
    }

    public void DisableAllColliders()
    {
        foreach (var collider in colliderPool)
            collider.enabled = false;
    }

    public EdgeCollider2D GetNextCollider()
    {
        if (currentIndex >= colliderPool.Count)
            return null; // Do not allocate more, just return null

        EdgeCollider2D collider = colliderPool[currentIndex];
        currentIndex++;
        return collider;
    }
}