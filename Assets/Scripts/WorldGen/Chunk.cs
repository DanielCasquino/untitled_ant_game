using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public int a;
    public int b;
    public int c;
    public Triangle(int _a, int _b, int _c)
    {
        a = _a;
        b = _b;
        c = _c;
    }
}

public class MeshData
{
    public Mesh mesh;
    public List<Vector3> vertices;
    public List<int> triangles;
    public Dictionary<Vector3, int> vertexMap;
    public Dictionary<int, List<Triangle>> triangleMap;
    public List<List<int>> outlines;
    public HashSet<int> checkedVertices;
    public MeshData()
    {
        mesh = new Mesh();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        vertexMap = new Dictionary<Vector3, int>();
        triangleMap = new Dictionary<int, List<Triangle>>();
        outlines = new List<List<int>>();
        checkedVertices = new HashSet<int>();
    }

    int TryAddTriangle(Vector3 v)
    {
        if (vertexMap.ContainsKey(v))
            return vertexMap[v];

        int index = vertices.Count;
        vertexMap[v] = index;
        vertices.Add(v);
        return index;
    }

    public void AddTriangle(Vector3 p, Vector3 q, Vector3 r)
    {
        int a = TryAddTriangle(r);
        int b = TryAddTriangle(q);
        int c = TryAddTriangle(p);

        Triangle tri = new Triangle(a, b, c);
        if (!triangleMap.ContainsKey(a))
            triangleMap[a] = new List<Triangle>();
        triangleMap[a].Add(tri);

        if (!triangleMap.ContainsKey(b))
            triangleMap[b] = new List<Triangle>();
        triangleMap[b].Add(tri);

        if (!triangleMap.ContainsKey(c))
            triangleMap[c] = new List<Triangle>();
        triangleMap[c].Add(tri);

        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    public void Clear()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        vertexMap.Clear();
        triangleMap.Clear();
        outlines.Clear();
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    MeshData meshData;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    float[,] density;
    int chunkId;
    float cellSize;
    int chunkResolution;
    [SerializeField] EdgeColliderPool edgeColliderPool;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(int _chunkId)
    {
        chunkId = _chunkId;
        cellSize = World.Instance.cellSize;
        chunkResolution = World.Instance.chunkResolution;
        GenerateDensity();
        GenerateMesh();
        GenerateOutlines();
        // GenerateEdgeColliders();
    }

    public void GenerateDensity()
    {
        density = new float[chunkResolution + 1, chunkResolution + 1];
        for (int j = 0; j <= chunkResolution; ++j)
        {
            for (int i = 0; i <= chunkResolution; ++i)
            {
                float x = transform.position.x + i * cellSize;
                float y = transform.position.y + j * cellSize;
                float noiseValue = Mathf.PerlinNoise((x + World.Instance.seed) * World.Instance.noiseScale, (y + World.Instance.seed) * World.Instance.noiseScale);
                density[i, j] = noiseValue;
            }
        }
    }

    public void GenerateMesh()
    {
        if (meshData == null)
            meshData = new MeshData();
        else
            meshData.Clear();

        for (int j = 0; j < chunkResolution; ++j)
        {
            for (int i = 0; i < chunkResolution; ++i)
            {
                bool a = density[j, i] <= World.Instance.noiseThreshold;
                bool b = density[j + 1, i] <= World.Instance.noiseThreshold;
                bool c = density[j + 1, i + 1] <= World.Instance.noiseThreshold;
                bool d = density[j, i + 1] <= World.Instance.noiseThreshold;
                //d--c
                //|  |
                //a--b

                float x = j * cellSize; float y = i * cellSize;
                Vector3 bottomLeft = new Vector3(x, y, 0);
                Vector3 bottomRight = new Vector3(x + cellSize, y, 0);
                Vector3 topRight = new Vector3(x + cellSize, y + cellSize, 0);
                Vector3 topLeft = new Vector3(x, y + cellSize, 0);
                Vector3 centreBottom = Vector3.Lerp(bottomLeft, bottomRight, 0.5f);
                Vector3 centreRight = Vector3.Lerp(bottomRight, topRight, 0.5f);
                Vector3 centreTop = Vector3.Lerp(topRight, topLeft, 0.5f);
                Vector3 centreLeft = Vector3.Lerp(topLeft, bottomLeft, 0.5f);

                int config = 0;

                if (a)
                    config |= 1;
                if (b)
                    config |= 2;
                if (c)
                    config |= 4;
                if (d)
                    config |= 8;

                switch (config)
                {
                    default:
                    case 0:
                        break;
                    case 1:
                        // only a
                        meshData.AddTriangle(bottomLeft, centreBottom, centreLeft);
                        break;
                    case 2:
                        // only b
                        meshData.AddTriangle(centreBottom, bottomRight, centreRight);
                        break;
                    case 3:
                        // a and b
                        meshData.AddTriangle(bottomLeft, bottomRight, centreLeft);
                        meshData.AddTriangle(bottomRight, centreRight, centreLeft);
                        break;
                    case 4:
                        // only c
                        meshData.AddTriangle(centreRight, topRight, centreTop);
                        break;
                    case 5:
                        // a and c
                        meshData.AddTriangle(bottomLeft, centreBottom, centreLeft);
                        meshData.AddTriangle(centreRight, topRight, centreTop);
                        break;
                    case 6:
                        // b and c
                        meshData.AddTriangle(centreBottom, bottomRight, topRight);
                        meshData.AddTriangle(centreBottom, topRight, centreTop);
                        break;
                    case 7:
                        // a, b, and c
                        meshData.AddTriangle(bottomLeft, bottomRight, centreLeft);
                        meshData.AddTriangle(bottomRight, topRight, centreTop);
                        meshData.AddTriangle(bottomRight, centreTop, centreLeft);
                        break;
                    case 8:
                        // only d
                        meshData.AddTriangle(centreLeft, centreTop, topLeft);
                        break;
                    case 9:
                        // a and d
                        meshData.AddTriangle(bottomLeft, centreBottom, topLeft);
                        meshData.AddTriangle(centreBottom, centreTop, topLeft);
                        break;
                    case 10:
                        // b and d
                        meshData.AddTriangle(centreBottom, bottomRight, centreRight);
                        meshData.AddTriangle(centreLeft, centreTop, topLeft);
                        break;
                    case 11:
                        // a, b and d
                        meshData.AddTriangle(bottomLeft, bottomRight, centreRight);
                        meshData.AddTriangle(bottomLeft, centreTop, topLeft);
                        meshData.AddTriangle(bottomLeft, centreRight, centreTop);
                        break;
                    case 12:
                        // c and d;
                        meshData.AddTriangle(centreRight, topRight, topLeft);
                        meshData.AddTriangle(centreLeft, centreRight, topLeft);
                        break;
                    case 13:
                        // a, c, and d
                        meshData.AddTriangle(bottomLeft, centreBottom, topLeft);
                        meshData.AddTriangle(centreRight, topRight, topLeft);
                        meshData.AddTriangle(centreBottom, centreRight, topLeft);
                        break;
                    case 14:
                        // b, c, and d
                        meshData.AddTriangle(centreBottom, bottomRight, topRight);
                        meshData.AddTriangle(centreLeft, topRight, topLeft);
                        meshData.AddTriangle(centreBottom, topRight, centreLeft);
                        break;
                    case 15:
                        // a, b, c, and d
                        meshData.AddTriangle(bottomLeft, bottomRight, topLeft);
                        meshData.AddTriangle(bottomRight, topRight, topLeft);
                        break;
                }

                meshData.mesh.vertices = meshData.vertices.ToArray();
                meshData.mesh.triangles = meshData.triangles.ToArray();
                meshData.mesh.RecalculateBounds();
                meshData.mesh.RecalculateNormals();
                meshFilter.mesh = meshData.mesh;
            }
        }
    }

    public void GenerateEdgeColliders()
    {
        edgeColliderPool.DisableAllColliders();
        for (int i = 0; i < meshData.outlines.Count; i++)
        {
            List<int> outline = meshData.outlines[i];
            EdgeCollider2D edgeCollider = edgeColliderPool.GetNextCollider();
            if (edgeCollider == null)
                continue; // 10 for now

            Vector2[] edgePoints = new Vector2[outline.Count];
            for (int j = 0; j < outline.Count; j++)
            {
                Vector3 vertex = meshData.vertices[outline[j]];
                edgePoints[j] = new Vector2(vertex.x, vertex.y);
            }
            edgeCollider.points = edgePoints;
            edgeCollider.enabled = true;
        }
    }

    public void GenerateOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < meshData.vertices.Count; vertexIndex++)
        {
            if (meshData.checkedVertices.Contains(vertexIndex))
                continue;

            int connectedVertex = GetConnectedOutlineVertex(vertexIndex);
            if (connectedVertex != -1)
            {
                List<int> newOutline = new List<int>();
                newOutline.Add(vertexIndex);
                meshData.checkedVertices.Add(vertexIndex);

                int currentVertex = connectedVertex;
                while (currentVertex != -1)
                {
                    newOutline.Add(currentVertex);
                    meshData.checkedVertices.Add(currentVertex);
                    currentVertex = GetConnectedOutlineVertex(currentVertex);
                    if (currentVertex == vertexIndex)
                        break;
                }

                meshData.outlines.Add(newOutline);
            }
        }
    }

    int GetConnectedOutlineVertex(int a)
    {
        List<Triangle> trianglesA = meshData.triangleMap[a];
        foreach (Triangle tri in trianglesA)
        {
            int[] verts = new int[] { tri.a, tri.b, tri.c };
            foreach (int vert in verts)
                if (vert != a && IsOutlineEdge(a, vert))
                    return vert;
        }
        return -1;
    }

    bool IsOutlineEdge(int a, int b)
    {
        List<Triangle> trianglesA = meshData.triangleMap[a];
        int sharedTriangleCount = 0;
        foreach (Triangle tri in trianglesA)
        {
            if (tri.a == b || tri.b == b || tri.c == b)
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                    return false;
            }
        }
        return true;
    }
}
