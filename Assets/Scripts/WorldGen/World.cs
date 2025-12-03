using UnityEngine;

public class World : MonoBehaviour
{
    public static World instance { get; private set; }
    [field: SerializeField] public int seed { get; private set; } = 888;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int worldResolution = 4; // a grid of 4 by 4 chunks
    [field: SerializeField] public int chunkResolution { get; private set; } = 10; // chunk is 16 cells by 16 cells
    [field: SerializeField] public float cellSize { get; private set; } = 1f; // in units
    Chunk[,] chunks;
    [field: SerializeField] public float noiseScale { get; private set; } = 0.07f;
    [field: SerializeField] public float noiseThreshold { get; private set; } = 0.5f;
    [SerializeField] Transform floor;
    EdgeCollider2D boundsCollider;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        boundsCollider = GetComponent<EdgeCollider2D>();
    }

    void Start()
    {
        CreateChunks();
        CreateBounds();
        // aca poner generateStructures()
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
        foreach (Chunk chunk in chunks)
        {
            chunk.GenerateTerrain();
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
}