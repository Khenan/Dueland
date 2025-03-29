using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance { get; private set; }

    private NetworkVariable<FixedString4096Bytes> compressedMap = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> mapIsGenerated = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int mapSize = 10;
    private MapGenerator mapGenerator = new MapGenerator();
    public List<Tile> Tiles { get; private set; } = new List<Tile>();
    private AStar aStar = new AStar();

    [SerializeField] private Sprite tileSprite;
    [SerializeField] private Tile tilePrefab;
    public static Action onMapGenerated;
    public static Action onMapInstantiated;

    private Tile startTile;
    private Tile endTile;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            Logger.Log("Shift + Left Mouse Button pressed, Get start Tile.");
            startTile = GetTileUnderMouse();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1))
        {
            Logger.Log("Shift + Right Mouse Button pressed, Get end Tile.");
            endTile = GetTileUnderMouse();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(2))
        {
            Logger.Log("Shift + Middle Mouse Button pressed, Calculate path.");
            if (startTile != null && endTile != null)
            {
                List<Tile> _tilePath = aStar.FindPath(startTile, endTile);
                Logger.Log("Path found: " + _tilePath.Count + " tiles.");
                for (int i = 0; i < _tilePath.Count; i++)
                {
                    float t = (float)i / (_tilePath.Count - 1); // Normalized value between 0 and 1
                    Color gradientColor = Color.Lerp(Color.red, Color.blue, t); // Interpolate between red and blue
                    _tilePath[i].GetComponent<SpriteRenderer>().color = gradientColor;
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            GetTileUnderMouse().Test();
        }
    }

    public Tile GetTileUnderMouse()
    {
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D _hit2D = Physics2D.Raycast(_ray.origin, _ray.direction);
        if (_hit2D.collider != null)
        {
            if (_hit2D.collider.TryGetComponent(out Tile _tile))
            {
                return _tile;
            }
        }
        return null;
    }

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network MapManager Spawned");
        LoadingPanel.Instance.ObjectLoaded();

        if (NetworkManager.Singleton.IsServer)
        {
            GenerateMap();
        }

        compressedMap.OnValueChanged += (previousValue, newValue) =>
        {
            InstantiateMap(ConvertStringToTiles(newValue));
        };

        if (mapIsGenerated.Value && compressedMap.Value.Length > 0)
        {
            InstantiateMap(ConvertStringToTiles(compressedMap.Value));
        }
    }

    private void GenerateMap()
    {
        byte[] _map = mapGenerator.GenerateMap(mapSize);
        compressedMap.Value = ConvertTilesToString(_map);
        mapIsGenerated.Value = true;
        onMapGenerated?.Invoke();
    }

    private void InstantiateMap(byte[] _map)
    {
        ClearMap();
        for (int _y = 0; _y < mapSize; _y++)
        {
            for (int _x = 0; _x < mapSize; _x++)
            {
                Tile _tile = Instantiate(tilePrefab);
                _tile.transform.position = new Vector3(_x, _y, 0);

                _tile.Init(new Vector2Int(_x, _y));
                _tile.gameObject.name = $"Tile {_x} {_y}";

                SpriteRenderer _spriteRenderer = _tile.tileSpriteRenderer;
                _spriteRenderer.sprite = tileSprite;
                byte _tileType = _map[_y * mapSize + _x];
                switch (_tileType)
                {
                    case 1: // Grass
                        _tile.Color = new Color(43f / 255f, 172f / 255f, 65f / 255f); // rgb(43, 172, 65)
                        break;
                    case 2: // Dirt
                        _tile.Color = new Color(141f / 255f, 102f / 255f, 80f / 255f); // rgb(141, 102, 80)
                        break;
                    case 3: // Water
                        _tile.Color = new Color(39 / 255f, 130 / 255f, 210 / 255f); // rgb(39, 130, 210)
                        break;
                }
                _tile.Reset();
                _tile.Cost = _tileType;
                Tiles.Add(_tile);
            }
        }

        // set neighbours
        foreach (var _tile in Tiles)
        {
            int _x = _tile.MatrixPosition.x;
            int _y = _tile.MatrixPosition.y;

            // Neighbours
            Tile _up = _y < mapSize - 1 ? Tiles[_x + (_y + 1) * mapSize] : null;
            Tile _down = _y > 0 ? Tiles[_x + (_y - 1) * mapSize] : null;
            Tile _left = _x > 0 ? Tiles[(_x - 1) + _y * mapSize] : null;
            Tile _right = _x < mapSize - 1 ? Tiles[(_x + 1) + _y * mapSize] : null;

            _tile.SetNeighbours(_up, _down, _left, _right);
        }
        onMapInstantiated?.Invoke();
    }

    private void ClearMap()
    {
        foreach (var _tile in Tiles)
        {
            Destroy(_tile.gameObject);
        }
        Tiles.Clear();
    }

    private FixedString4096Bytes ConvertTilesToString(byte[] _tiles)
    {
        return new FixedString4096Bytes(string.Join(",", _tiles));
    }

    private byte[] ConvertStringToTiles(FixedString4096Bytes _mapString)
    {
        string[] _tiles = _mapString.ToString().Split(',');
        byte[] _map = new byte[_tiles.Length];
        for (int i = 0; i < _tiles.Length; i++)
        {
            _map[i] = byte.Parse(_tiles[i]);
        }
        return _map;
    }

    internal Tile GetTileByMatrixPosition(int _x, int _y)
    {
        return Tiles[_x + _y * mapSize];
    }

    internal Tile GetRandomTile()
    {
        int _x = Random.Range(0, mapSize);
        int _y = Random.Range(0, mapSize);
        return GetTileByMatrixPosition(_x, _y);
    }
}
