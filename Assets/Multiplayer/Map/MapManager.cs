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
    private NetworkList<TileData> tileDatas = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int mapSize = 10;
    private MapGenerator mapGenerator = new MapGenerator();
    public List<Tile> Tiles { get; private set; } = new List<Tile>();
    public AStar aStar = new AStar();

    [SerializeField] private Sprite tileSprite;
    [SerializeField] private Tile tilePrefab;
    public static Action onMapGenerated;
    public static Action onMapInstantiated;

    // Path
    [SerializeField] private Transform tilePathVisualPrefab;
    private List<Transform> tilePathVisualPool = new List<Transform>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
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

    internal void FindPath(Vector2Int _startPosition, Vector2Int _endPosition, out Tile[] _path, out int _pathCost)
    {
        if (_startPosition == _endPosition)
        {
            _path = null;
            _pathCost = 0;
            return;
        }

        Tile _startTile = GetTileByMatrixPosition(_startPosition.x, _startPosition.y);
        Tile _endTile = GetTileByMatrixPosition(_endPosition.x, _endPosition.y);
        List<Tile> _tilePath = aStar.FindPath(_startTile, _endTile, tileDatas, out _pathCost);
        if (_tilePath != null)
        {
            _path = _tilePath.ToArray();
        }
        else
        {
            _path = null;
        }
    }

    internal void ShowPathVisual(Tile[] _path)
    {
        HidePathVisual();

        CreateVisualPathIfNeeded(_path);

        // Show and set position for each path visual
        for (int i = 0; i < _path.Length; i++)
        {
            Transform _pathVisual = tilePathVisualPool[i];
            _pathVisual.gameObject.SetActive(true);
            _pathVisual.position = new Vector3(_path[i].MatrixPosition.x, _path[i].MatrixPosition.y, 0);
        }
    }

    private void CreateVisualPathIfNeeded(Tile[] _path)
    {
        if (tilePathVisualPrefab != null)
        {
            if (tilePathVisualPool.Count < _path.Length)
            {
                for (int i = 0; i < _path.Length - tilePathVisualPool.Count; i++)
                {
                    Transform _pathVisual = Instantiate(tilePathVisualPrefab);
                    tilePathVisualPool.Add(_pathVisual);
                }
            }
        }
    }

    internal void HidePathVisual()
    {
        if (tilePathVisualPool.Count > 0)
        {
            foreach (var _pathVisual in tilePathVisualPool)
            {
                _pathVisual.gameObject.SetActive(false);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetTileDataServerRpc(int _x, int _y, ulong _networkObjectId, bool _isWalkable)
    {
        TileData _tileData = new TileData()
        {
            MatrixPosition = new Vector2Int(_x, _y),
            NetworkObjectId = _networkObjectId,
            IsWalkable = _isWalkable
        };
        tileDatas.Add(_tileData);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void RemoveTileDataServerRpc(int _x, int _y)
    {
        for (int _i = 0; _i < tileDatas.Count; _i++)
        {
            if (tileDatas[_i].MatrixPosition == new Vector2Int(_x, _y))
            {
                tileDatas.RemoveAt(_i);
                break;
            }
        }
    }
}
