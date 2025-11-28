using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    [field: SerializeField] public float radius { get; private set; } = 6f;
    [field: SerializeField] public int angularResolution { get; private set; } = 120;

    Mesh mesh;
    MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
    }

    void Update()
    {
        Vector3[] vertices = new Vector3[angularResolution + 1];
        int[] triangles = new int[angularResolution * 3];
        vertices[0] = Vector3.zero;
        for (int i = 1; i <= angularResolution; ++i)
        {
            Vector3 dir = Quaternion.Euler(0, 0, (i - 1) * (360f / angularResolution)) * Vector3.right;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, radius))
                vertices[i] = hit.point;
            else
                vertices[i] = dir * radius;
        }

        for (int i = 0; i < angularResolution; ++i)
        {
            triangles[i * 3] = (i + 1) % angularResolution + 1;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = 0;
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
