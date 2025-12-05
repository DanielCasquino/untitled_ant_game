#if UNITY_EDITOR
using UnityEditor;
#endif

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
        checkedVertices.Clear();
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    MeshData meshData;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    int chunkId;
    bool[,] density;
    float cellSize;
    int chunkResolution;
    [SerializeField] EdgeColliderPool edgeColliderPool;
    public SoundController sound;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(int _chunkId)
    {
        chunkId = _chunkId;
        cellSize = World.instance.cellSize;
        chunkResolution = World.instance.chunkResolution;
        GenerateDensity();
        sound = Player.instance.sound;
    }

    public void GenerateTerrain()
    {
        // ty pmarini && lague
        GenerateMesh();
        GenerateOutlines();
        GenerateEdgeColliders();
    }

    float NoiseProvider(float x, float y)
    {
        // perlin octaves + thresh
        float noise = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float persistence = 0.5f;
        int octaves = 4;

        for (int i = 0; i < octaves; i++)
        {
            noise += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            amplitude *= persistence;
            frequency *= 2f;
        }

        // invert and sharpen noise
        noise = 1f - Mathf.Abs(noise - 0.5f) * 2f;
        return noise;

        // ty gpt
    }

    public void GenerateDensity()
    {
        density = new bool[chunkResolution + 1, chunkResolution + 1];
        for (int j = 0; j <= chunkResolution; ++j)
        {
            for (int i = 0; i <= chunkResolution; ++i)
            {
                if (chunkId == 0 || chunkId == World.instance.worldResolution * World.instance.worldResolution - 1)
                {
                    density[i, j] = false;
                    continue;
                }
                float x = transform.position.x + i * cellSize;
                float y = transform.position.y + j * cellSize;
                float noiseValue = NoiseProvider((x + World.instance.seed) * World.instance.noiseScale, (y + World.instance.seed) * World.instance.noiseScale);
                density[i, j] = noiseValue <= World.instance.noiseThreshold;
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
                bool a = density[j, i];
                bool b = density[j + 1, i];
                bool c = density[j + 1, i + 1];
                bool d = density[j, i + 1];
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
            }
        }
        meshData.mesh.vertices = meshData.vertices.ToArray();
        meshData.mesh.triangles = meshData.triangles.ToArray();
        meshData.mesh.RecalculateBounds();
        meshData.mesh.RecalculateNormals();
        meshFilter.mesh = meshData.mesh;
    }

    public void GenerateEdgeColliders()
    {
        edgeColliderPool.Reset();
        foreach (List<int> outline in meshData.outlines)
        {
            EdgeCollider2D edgeCollider = edgeColliderPool.GetNextCollider();
            if (edgeCollider == null)
                continue; // shouldnt happen, hardcoded upper bound
            Vector2[] edgePoints = new Vector2[outline.Count + 1];
            for (int i = 0; i < outline.Count; i++)
            {
                Vector3 vertex = meshData.vertices[outline[i]];
                edgePoints[i] = new Vector2(transform.position.x + vertex.x, transform.position.y + vertex.y);
            }
            edgePoints[outline.Count] = edgePoints[0];
            edgeCollider.points = edgePoints;
            edgeCollider.enabled = true;
        }
    }

    public void GenerateOutlines()
    {
        // i guess this could be faster if the first vertex is always the top left vertex of the chunk
        for (int i = 0; i < meshData.vertices.Count; i++)
        {
            if (meshData.checkedVertices.Contains(i))
                continue;

            int connectedVertex = GetConnectedOutlineVertex(i);
            if (connectedVertex == -1)
                continue;

            List<int> newOutline = new List<int>() { i };
            meshData.checkedVertices.Add(i);

            int currentVertex = connectedVertex;
            while (currentVertex != -1)
            {
                newOutline.Add(currentVertex);
                meshData.checkedVertices.Add(currentVertex);
                currentVertex = GetConnectedOutlineVertex(currentVertex);
                if (currentVertex == i)
                    break;
            }

            meshData.outlines.Add(newOutline);
        }
    }

    int GetConnectedOutlineVertex(int a)
    {
        List<Triangle> trianglesA = meshData.triangleMap[a];
        foreach (Triangle triangle in trianglesA)
        {
            int[] verts = new int[] { triangle.a, triangle.b, triangle.c };
            foreach (int vert in verts)
            {
                if (vert == a || meshData.checkedVertices.Contains(vert))
                    continue;
                if (!IsOutlineEdge(a, vert))
                    continue;
                return vert;
            }
        }
        return -1;
    }

    bool IsOutlineEdge(int a, int b)
    {
        List<Triangle> trianglesA = meshData.triangleMap[a];
        int count = 0;
        foreach (Triangle tri in trianglesA)
        {
            if (tri.a == b || tri.b == b || tri.c == b)
            {
                count++;
                if (count > 1)
                    break;
            }
        }
        return count == 1;
    }

    public void SetDensity(int i, int j, bool value)
    {
        density[i, j] = value;
        GenerateTerrain();
    }

    public void OverrideDensity(int i, int j, bool value)
    {
        density[i, j] = value;
    }


    public void densityAction(int i, int j, bool value, bool isSet = true)
    {
        if (isSet) SetDensity(i, j, value);
        else OverrideDensity(i, j, value);
    }

    public void ModifyNode(int i, int j, bool value, bool isSet = true)
    {
        if (i < 0 || i > chunkResolution || j < 0 || j > chunkResolution)
            return;
        if (density[i, j] == value)
            return;

        if(isSet){
            if (value) sound.ColocarBloque();
            else sound.RomperBloque();
}
            bool updateLeft = i == 0;
        bool updateRight = i == chunkResolution;
        bool updateBottom = j == 0;
        bool updateTop = j == chunkResolution;

        Chunk left, right, top, bottom;
        World.instance.GetNeighbour(chunkId, out left, out right, out top, out bottom);

        densityAction(i, j, value);

        if (updateLeft && left != null)
            left.densityAction(chunkResolution, j, value);
        if (updateRight && right != null)
            right.densityAction(0, j, value);
        if (updateBottom && bottom != null)
            bottom.densityAction(i, chunkResolution, value);
        if (updateTop && top != null)
            top.densityAction(i, 0, value);

        // Corners
        if (updateLeft && updateBottom && left != null && bottom != null)
        {
            Chunk bottomLeft;
            World.instance.GetNeighbour(left.chunkId, out _, out _, out _, out bottomLeft);
            if (bottomLeft != null)
                bottomLeft.densityAction(chunkResolution, chunkResolution, value);
        }
        if (updateRight && updateBottom && right != null && bottom != null)
        {
            Chunk bottomRight;
            World.instance.GetNeighbour(right.chunkId, out _, out _, out _, out bottomRight);
            if (bottomRight != null)
                bottomRight.densityAction(0, chunkResolution, value);
        }
        if (updateLeft && updateTop && left != null && top != null)
        {
            Chunk topLeft;
            World.instance.GetNeighbour(left.chunkId, out _, out _, out topLeft, out _);
            if (topLeft != null)
                topLeft.densityAction(chunkResolution, 0, value);
        }
        if (updateRight && updateTop && right != null && top != null)
        {
            Chunk topRight;
            World.instance.GetNeighbour(right.chunkId, out _, out _, out topRight, out _);
            if (topRight != null)
                topRight.densityAction(0, 0, value);
        }
        // this is awful
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (density == null)
            return;

        float radius = cellSize * 0.15f;
        for (int i = 0; i <= chunkResolution; ++i)
        {
            for (int j = 0; j <= chunkResolution; ++j)
            {
                float x = transform.position.x + i * cellSize;
                float y = transform.position.y + j * cellSize;
                Vector3 pos = new Vector3(x, y, 0);

                if (density[i, j])
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.green;

                Handles.color = Gizmos.color;
                Handles.DrawSolidDisc(pos, Vector3.forward, radius);
            }
        }
    }
#endif
}
