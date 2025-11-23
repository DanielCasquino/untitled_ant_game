using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class World : MonoBehaviour
{
    [SerializeField] int size = 4;
    int chunkSize = 10; // world is subdivided into 10x10 chunks
    int mapSize;
    float[,] density;
    float noiseThreshold = 0.6f;
    float noiseScale = 0.1f;

    Mesh mesh;
    MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        mapSize = size * chunkSize;
        InitializeDensity();
        MarchingSquares();
    }

    void OnDrawGizmos()
    {
        if (density == null)
            return;
        for (int i = 0; i <= mapSize; i++)
        {
            for (int j = 0; j <= mapSize; j++)
            {
                float discreteNoise = density[i, j] > noiseThreshold ? 1f : 0f;
                Gizmos.color = new Color(discreteNoise, discreteNoise, discreteNoise);
                Gizmos.DrawSphere(new Vector3(i, j, 0), 0.1f);
            }
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "Regen"))
        {
            InitializeDensity();
            MarchingSquares();
        }
    }

    void InitializeDensity()
    {
        density = new float[mapSize + 1, mapSize + 1];

        float randomOffsetX = Random.Range(-256f, 256f);
        float randomOffsetY = Random.Range(-256f, 256f);

        for (int i = 0; i <= mapSize; ++i)
        {
            for (int j = 0; j <= mapSize; ++j)
            {
                density[i, j] = Mathf.PerlinNoise((randomOffsetX + i) * noiseScale, (randomOffsetY + j) * noiseScale);
            }
        }
    }
    void MarchingSquares()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        void AddTriangle(Vector3 p, Vector3 q, Vector3 r)
        {
            int startIndex = vertices.Count;
            vertices.Add(r);
            vertices.Add(q);
            vertices.Add(p);
            triangles.Add(startIndex);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
        }

        for (int i = 0; i < mapSize; ++i)
        {
            for (int j = 0; j < mapSize; ++j)
            {
                bool a = density[i, j] <= noiseThreshold;
                bool b = density[i + 1, j] <= noiseThreshold;
                bool c = density[i + 1, j + 1] <= noiseThreshold;
                bool d = density[i, j + 1] <= noiseThreshold;

                //d--c
                //|  |
                //a--b

                Vector3 bottomLeft = new Vector3(i, j, 0);
                Vector3 bottomRight = new Vector3(i + 1, j, 0);
                Vector3 topRight = new Vector3(i + 1, j + 1, 0);
                Vector3 topLeft = new Vector3(i, j + 1, 0);

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
                        AddTriangle(bottomLeft, centreBottom, centreLeft);
                        break;
                    case 2:
                        // only b
                        AddTriangle(centreBottom, bottomRight, centreRight);
                        break;
                    case 3:
                        // a and b
                        AddTriangle(bottomLeft, bottomRight, centreLeft);
                        AddTriangle(bottomRight, centreRight, centreLeft);
                        break;
                    case 4:
                        // only c
                        AddTriangle(centreRight, topRight, centreTop);
                        break;
                    case 5:
                        // a and c
                        AddTriangle(bottomLeft, centreBottom, centreLeft);
                        AddTriangle(centreRight, topRight, centreTop);
                        break;
                    case 6:
                        // b and c
                        AddTriangle(centreBottom, bottomRight, topRight);
                        AddTriangle(centreBottom, topRight, centreTop);
                        break;
                    case 7:
                        // a, b, and c
                        AddTriangle(bottomLeft, bottomRight, centreLeft);
                        AddTriangle(bottomRight, topRight, centreTop);
                        AddTriangle(bottomRight, centreTop, centreLeft);
                        break;
                    case 8:
                        // only d
                        AddTriangle(centreLeft, centreTop, topLeft);
                        break;
                    case 9:
                        // a and d
                        AddTriangle(bottomLeft, centreBottom, topLeft);
                        AddTriangle(centreBottom, centreTop, topLeft);
                        break;
                    case 10:
                        // b and d
                        AddTriangle(centreBottom, bottomRight, centreRight);
                        AddTriangle(centreLeft, centreTop, topLeft);
                        break;
                    case 11:
                        // a, b and d
                        AddTriangle(bottomLeft, bottomRight, centreRight);
                        AddTriangle(bottomLeft, centreTop, topLeft);
                        AddTriangle(bottomLeft, centreRight, centreTop);
                        break;
                    case 12:
                        // c and d;
                        AddTriangle(centreRight, topRight, topLeft);
                        AddTriangle(centreLeft, centreRight, topLeft);
                        break;
                    case 13:
                        // a, c, and d
                        AddTriangle(bottomLeft, centreBottom, topLeft);
                        AddTriangle(centreRight, topRight, topLeft);
                        AddTriangle(centreBottom, centreRight, topLeft);
                        break;
                    case 14:
                        // b, c, and d
                        AddTriangle(centreBottom, bottomRight, topRight);
                        AddTriangle(centreLeft, topRight, topLeft);
                        AddTriangle(centreBottom, topRight, centreLeft);
                        break;
                    case 15:
                        // a, b, c, and d
                        AddTriangle(bottomLeft, bottomRight, topLeft);
                        AddTriangle(bottomRight, topRight, topLeft);
                        break;
                }
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        meshFilter.mesh = mesh;
    }

    public void Dig(Vector3 worldPos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapSize);
        int y = Mathf.Clamp(Mathf.RoundToInt(worldPos.y), 0, mapSize);
        density[x, y] = 1f;
        MarchingSquares(); // recalculate 4 neighbours only, same for fill
    }

    public void Fill(Vector3 worldPos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapSize);
        int y = Mathf.Clamp(Mathf.RoundToInt(worldPos.y), 0, mapSize);
        density[x, y] = 0f;
        MarchingSquares();
    }
}
