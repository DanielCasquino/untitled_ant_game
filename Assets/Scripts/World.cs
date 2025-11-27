using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class World : MonoBehaviour
{
    [SerializeField] int worldSize = 4; // a grid of 4 by 4 chunks
    [SerializeField] int chunkSize = 10; // chunk is 10 units by 10 units
    [SerializeField] float cellSize = 1f; // size of each cell in units
    float[,] density;
    [SerializeField][Range(0f, 1f)] float noiseThreshold = 0.6f;
    [SerializeField] float noiseScale = 0.1f;
    int mapResolution; // total number of cells per map side

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        mapResolution = worldSize * chunkSize;
        InitializeDensity();
        MarchingSquares();
    }

    // void OnDrawGizmos()
    // {
    //     if (density == null)
    //         return;

    //     float mapWidth = size * chunkSize;
    //     float mapHeight = size * chunkSize;

    //     float cellWidth = mapWidth / mapResolution;
    //     float cellHeight = mapHeight / mapResolution;

    //     for (int i = 0; i <= mapResolution; i++)
    //     {
    //         for (int j = 0; j <= mapResolution; j++)
    //         {
    //             float x = i * cellWidth;
    //             float y = j * cellHeight;
    //             float discreteNoise = density[i, j] > noiseThreshold ? 1f : 0f;
    //             Gizmos.color = new Color(discreteNoise, discreteNoise, discreteNoise);
    //             Gizmos.DrawSphere(new Vector3(x, y, 0), 0.1f);
    //         }
    //     }
    // }

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
        density = new float[mapResolution + 1, mapResolution + 1];

        float randomOffsetX = Random.Range(-256f, 256f);
        float randomOffsetY = Random.Range(-256f, 256f);

        for (int i = 0; i <= mapResolution; ++i)
        {
            for (int j = 0; j <= mapResolution; ++j)
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
            
            Vector2[] edgePoints = new Vector2[3];
            edgePoints[0] = r;
            edgePoints[1] = q;
            edgePoints[2] = p;

            EdgeCollider2D ec = gameObject.AddComponent<EdgeCollider2D>();
            ec.points = edgePoints;
        }

        for (int i = 0; i < mapResolution; ++i)
        {
            for (int j = 0; j < mapResolution; ++j)
            {
                bool a = density[i, j] <= noiseThreshold;
                bool b = density[i + 1, j] <= noiseThreshold;
                bool c = density[i + 1, j + 1] <= noiseThreshold;
                bool d = density[i, j + 1] <= noiseThreshold;

                //d--c
                //|  |
                //a--b


                float x = i * cellSize; float y = j * cellSize;
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
        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapResolution);
        int y = Mathf.Clamp(Mathf.RoundToInt(worldPos.y), 0, mapResolution);
        density[x, y] = 1f;
        MarchingSquares(); // recalculate 4 neighbours only, same for fill
    }

    public void Fill(Vector3 worldPos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapResolution);
        int y = Mathf.Clamp(Mathf.RoundToInt(worldPos.y), 0, mapResolution);
        density[x, y] = 0f;
        MarchingSquares();
    }
}
