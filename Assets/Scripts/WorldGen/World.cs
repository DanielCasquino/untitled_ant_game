using UnityEngine;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }
    [field: SerializeField] public int seed { get; private set; } = 888;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] int worldResolution = 4; // a grid of 4 by 4 chunks
    [field: SerializeField] public int chunkResolution { get; private set; } = 10; // chunk is 16 cells by 16 cells
    [field: SerializeField] public float cellSize { get; private set; } = 1f; // in units
    Chunk[,] chunks;
    [field: SerializeField] public float noiseScale { get; private set; } = 0.07f;
    [field: SerializeField] public float noiseThreshold { get; private set; } = 0.5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        CreateChunks();
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

    public void GetNeighbour(int chunkId, out Chunk left, out Chunk right, out Chunk top, out Chunk bottom)
    {
        int x = chunkId % worldResolution;
        int y = chunkId / worldResolution;

        left = (x > 0) ? chunks[x - 1, y] : null;
        right = (x < worldResolution - 1) ? chunks[x + 1, y] : null;
        top = (y < worldResolution - 1) ? chunks[x, y + 1] : null;
        bottom = (y > 0) ? chunks[x, y - 1] : null;
    }
}