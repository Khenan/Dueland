using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance { get; private set; }

    private NetworkVariable<FixedString4096Bytes> compressedMap = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> mapIsGenerated = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private MapGenerator mapGeneratorManager = new MapGenerator();
    private List<GameObject> tiles = new List<GameObject>();

    [SerializeField] private Sprite tileSprite;

    void Awake()
    {
        Instance = this;
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
            CreateMap(ConvertStringToTiles(newValue));
        };

        if (mapIsGenerated.Value && compressedMap.Value.Length > 0)
        {
            CreateMap(ConvertStringToTiles(compressedMap.Value));
        }
    }

    private void GenerateMap()
    {
        byte[] _map = mapGeneratorManager.GenerateMap();
        compressedMap.Value = ConvertTilesToString(_map);
        mapIsGenerated.Value = true;
    }

    private void CreateMap(byte[] map)
    {
        ClearMap();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject _tile = new GameObject("Tile");
                _tile.transform.position = new Vector3(i, j, 0);

                SpriteRenderer _spriteRenderer = _tile.AddComponent<SpriteRenderer>();
                _spriteRenderer.sprite = tileSprite;
                switch (map[i * 10 + j])
                {
                    case 1: // Grass
                        _spriteRenderer.color = new Color(43f / 255f, 172f / 255f, 65f / 255f); // rgb(43, 172, 65)
                        break;
                    case 2: // Dirt
                        _spriteRenderer.color = new Color(141f / 255f, 102f / 255f, 80f / 255f); // rgb(141, 102, 80)
                        break;
                    case 3: // Water
                        _spriteRenderer.color = new Color(39 / 255f, 130 / 255f, 210 / 255f); // rgb(39, 130, 210)
                        break;
                }
                tiles.Add(_tile);
            }
        }
    }

    private void ClearMap()
    {
        foreach (var tile in tiles)
        {
            Destroy(tile);
        }
        tiles.Clear();
    }

    private FixedString4096Bytes ConvertTilesToString(byte[] tiles)
    {
        return new FixedString4096Bytes(string.Join(",", tiles));
    }

    private byte[] ConvertStringToTiles(FixedString4096Bytes map)
    {
        string[] _tiles = map.ToString().Split(',');
        byte[] _map = new byte[_tiles.Length];
        for (int i = 0; i < _tiles.Length; i++)
        {
            _map[i] = byte.Parse(_tiles[i]);
        }
        return _map;
    }
}
