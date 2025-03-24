using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public Tile[,] tiles;
    public Vector2Int mapSize = new(10, 10);
    public GameObject tilePrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        tiles = new Tile[mapSize.x, mapSize.y];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2 tilePosition = new(x, y);
                Tile tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity).GetComponent<Tile>();
                tile.SetTileData(new TileData(x + y, tilePosition, new Size3D(1, 1, 1)));
                tiles[x, y] = tile;
            }
        }
    }
}
