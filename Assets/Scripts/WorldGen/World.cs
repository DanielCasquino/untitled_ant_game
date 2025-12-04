using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StructureInfo
{
    public int id;

    [HideInInspector]
    public Bounds boundingBox;

    public GameObject prefab;

    public StructureInfo() { }

    public void UpdateBoundingBox(float padding = 5.0f)
    {
        if (prefab == null)
        {
            this.boundingBox = new Bounds(Vector3.zero, Vector3.one);
            return;
        }

        SpriteRenderer spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            boundingBox = spriteRenderer.bounds;
            boundingBox.Expand(new Vector3(padding, padding, 0f));
        }
        else
        {
            this.boundingBox = new Bounds(Vector3.zero, Vector3.one);
            this.boundingBox.Expand(new Vector3(padding, padding, 0f));
        }
    }


}

public class PlacedStructure
{
    public int id;
    public Vector2 center;
    public Bounds boundingBox;
}

public class World : MonoBehaviour
{
    public static World instance { get; private set; }
    [field: SerializeField] public int seed { get; private set; } = 888;
    [SerializeField] GameObject chunkPrefab;
    public int worldResolution = 4; // a grid of 4 by 4 chunks
    [field: SerializeField] public int chunkResolution { get; private set; } = 10; // chunk is 16 cells by 16 cells
    [field: SerializeField] public float cellSize { get; private set; } = 1f; // in units
    Chunk[,] chunks;
    [field: SerializeField] public float noiseScale { get; private set; } = 0.07f;
    [field: SerializeField] public float noiseThreshold { get; private set; } = 0.5f;
    [field: SerializeField] public float padding = 5.0f;
    [SerializeField] Transform floor;
    EdgeCollider2D boundsCollider;

    // Atributos para la estructura
    [SerializeField] StructureInfo[] structureList;
    [SerializeField] int structuresToSpawn = 20;
    [SerializeField] int minDistance = 10;

    Dictionary<int, StructureInfo> structures = new Dictionary<int, StructureInfo>();
    List<PlacedStructure> placedStructures = new List<PlacedStructure>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        boundsCollider = GetComponent<EdgeCollider2D>();
    }

    void OnValidate()
    {
        if (structureList == null) return;

        foreach (var s in structureList)
        {
            if (s.prefab != null)
            {
                s.UpdateBoundingBox(padding);
            }
        }
    }

    void Start()
    {
        seed = Random.Range(-256, 256);
        CreateChunks();
        CreateBounds();
        SpawnStructures();
        GenerateChunks();
    }

    void CreateChunks()
    {
        chunks = new Chunk[worldResolution, worldResolution];
        for (int j = 0; j < worldResolution; ++j)
        {
            for (int i = 0; i < worldResolution; ++i)
            {
                int chunkId = j * worldResolution + i;
                Vector3 chunkPosition = new Vector3(i * chunkResolution * cellSize, j * chunkResolution * cellSize, 0);
                GameObject chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity, transform);
                chunk.name = "Chunk_" + chunkId;
                Chunk chunkComponent = chunk.GetComponent<Chunk>();
                chunkComponent.Initialize(chunkId);
                chunks[i, j] = chunkComponent;
            }
        }
    }

    public void GenerateChunks()
    {
        for (int i = 0; i < worldResolution; ++i)
        {
            for (int j = 0; j < worldResolution; ++j)
            {
                chunks[i, j].GenerateTerrain();
            }
        }
    }

    public void GetNeighbour(int chunkId, out Chunk left, out Chunk right, out Chunk top, out Chunk bottom)
    {
        int x = chunkId % worldResolution;
        int y = chunkId / worldResolution;

        left = (x > 0) ? chunks[x - 1, y] : null;
        right = (x < worldResolution - 1) ? chunks[x + 1, y] : null;
        top = (y < worldResolution - 1) ? chunks[x, y + 1] : null;
        bottom = (y > 0) ? chunks[x, y - 1] : null;
    }

    int PositionToChunkId(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x / (chunkResolution * cellSize));
        int y = Mathf.FloorToInt(position.y / (chunkResolution * cellSize));

        if (x < 0 || x >= worldResolution || y < 0 || y >= worldResolution)
            return -1;

        return y * worldResolution + x;
    }

    Vector2Int PositionToChunkSpace(Vector2 position, int chunkId)
    {
        // todo: get closest node
        int chunkX = chunkId % worldResolution;
        int chunkY = chunkId / worldResolution;

        float chunkOriginX = chunkX * chunkResolution * cellSize;
        float chunkOriginY = chunkY * chunkResolution * cellSize;

        int i = Mathf.RoundToInt((position.x - chunkOriginX) / cellSize);
        int j = Mathf.RoundToInt((position.y - chunkOriginY) / cellSize);

        i = Mathf.Clamp(i, 0, chunkResolution);
        j = Mathf.Clamp(j, 0, chunkResolution);

        return new Vector2Int(i, j);
    }

    public void ModifyTerrain(Vector2 position, bool value)
    {
        int chunkId = PositionToChunkId(position);
        Debug.Log(chunkId);
        if (chunkId == -1)
            return;
        Vector2Int chunkCoords = PositionToChunkSpace(position, chunkId);
        Debug.Log($"{chunkId}, {chunkCoords}");

        int x = chunkId % worldResolution;
        int y = chunkId / worldResolution;
        chunks[x, y].ModifyNode(chunkCoords.x, chunkCoords.y, value);
    }

    void CreateBounds()
    {
        float worldDimensions = worldResolution * chunkResolution * cellSize;
        floor.localScale = Vector3.one * worldDimensions;
        floor.position = Vector3.one * floor.localScale.x / 2;

        Vector2[] points = new Vector2[5];
        points[0] = Vector2.zero;
        points[1] = Vector2.up * worldDimensions;
        points[2] = Vector2.one * worldDimensions;
        points[3] = Vector2.right * worldDimensions;
        points[4] = Vector2.zero;

        boundsCollider.points = points;
    }
    // Para generar las estructuras
    void SpawnStructures()
    {

        if (structureList.Length <= 0) return;

        float worldSize = worldResolution * chunkResolution * cellSize;

        for (int i = 0; i < structuresToSpawn; ++i)
        {
            Vector2 point = Vector2.zero;
            Bounds bb = new Bounds();
            StructureInfo info = new StructureInfo();

            bool es_valido = false;
            while (!es_valido)
            {

                point = new Vector2(Random.Range(0, worldSize), Random.Range(0, worldSize));
                info = PickRandomStructure();
                bb = new Bounds(
                    new Vector3(point.x, point.y, 0),
                    info.boundingBox.size
                );

                es_valido = true;

                // L�mite del mundo
                if (bb.min.x < 0 || bb.max.x > worldSize) es_valido = false;
                else if (bb.min.y < 0 || bb.max.y > worldSize) es_valido = false;

                // Superposici�n con otras estructuras
                else if (IsOverlapping(bb)) es_valido = false;

                // Distancia m�nima
                else if (!HasMinBoxDistance(bb, info.boundingBox, minDistance)) es_valido = false;

            }

            // funcion que setea los lugares de la malla es 0.
            DisableMeshChunks(bb);

            Instantiate(info.prefab, point, Quaternion.identity, transform);

            placedStructures.Add(new PlacedStructure()
            {
                id = info.id,
                center = point,
                boundingBox = bb
            });
        }
    }

    bool IsOverlapping(Bounds newBB)
    {
        foreach (var s in placedStructures)
        {
            if (newBB.Intersects(s.boundingBox))
                return true;
        }

        return false;
    }

    void DisableMeshChunks(Bounds bb)
    {
        int startX = Mathf.FloorToInt(bb.min.x);
        int endX = Mathf.CeilToInt(bb.max.x);

        int startY = Mathf.FloorToInt(bb.min.y);
        int endY = Mathf.CeilToInt(bb.max.y);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector2 cord = new Vector2(x, y);

                int chunkId = PositionToChunkId(cord);
                Vector2Int pos_in_chunk = PositionToChunkSpace(cord, chunkId);

                int chunkX = chunkId % worldResolution;
                int chunkY = chunkId / worldResolution;

                chunks[chunkX, chunkY].ModifyNode(pos_in_chunk.x, pos_in_chunk.y, false, false);
            }
        }
    }

    StructureInfo PickRandomStructure()
    {
        return structureList[Random.Range(0, structureList.Length)];
    }

    bool HasMinBoxDistance(Bounds a, Bounds b, float minDist)
    {
        float dx = 0f;

        if (a.max.x < b.min.x)
            dx = b.min.x - a.max.x;
        else if (b.max.x < a.min.x)
            dx = a.min.x - b.max.x;

        float dy = 0f;

        if (a.max.y < b.min.y)
            dy = b.min.y - a.max.y;
        else if (b.max.y < a.min.y)
            dy = a.min.y - b.max.y;

        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        return dist >= minDist;
    }

    // Para ver los mbb
    void OnDrawGizmos()
    {
        if (placedStructures == null) return;

        Gizmos.color = Color.yellow;

        foreach (var ps in placedStructures)
        {
            Gizmos.DrawWireCube(ps.boundingBox.center, ps.boundingBox.size);
        }
    }
}