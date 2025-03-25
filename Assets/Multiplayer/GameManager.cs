using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<bool> IsGameStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<FixedString4096Bytes> compressedString = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<ulong> playerIds = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float GameTime => gameTime.Value;

    [SerializeField] private Sprite tileSprite;

    public static Action<GameManager> onGameManagerSpawned;

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
            }
            return instance;
        }
    }
    public Action OnAllPlayersConnected { get; set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PingServerRpc();
        }

        if (NetworkManager.Singleton.IsServer)
        {
            gameTime.Value += Time.deltaTime;

            if (!IsGameStarted.Value && MultiplayerManager.Instance.JoinedLobby.Players.Count == 2)
            {
                IsGameStarted.Value = true;
                OnAllPlayersConnected?.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            // 1 = grass, 2 = dirt, 3 = water
            byte[] map = new byte[100];
            System.Random rand = new System.Random();
            int riverPosition = 4;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (j == riverPosition || j == riverPosition + 1) // River with noise
                    {
                        map[i * 10 + j] = 3; // Water
                    }
                    else if (j == riverPosition - 1 || j == riverPosition + 2) // Dirt along the river
                    {
                        map[i * 10 + j] = 2; // Dirt
                    }
                    else
                    {
                        map[i * 10 + j] = 1; // Grass
                    }
                }
                // Add noise to the river position
                riverPosition += rand.Next(-1, 2);
                riverPosition = Mathf.Clamp(riverPosition, 1, 8); // Keep river within bounds
            }
            compressedString.Value = ConvertTilesToString(map);
            Logger.Log("Compressed string: " + compressedString.Value.ToString());
            Logger.Log("Compressed string: " + compressedString.Value);

            GenerateMap(map);
        }
    }

    private List<GameObject> tiles = new List<GameObject>();
    private void GenerateMap(byte[] map)
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

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network GameManager Spawned");
        onGameManagerSpawned?.Invoke(this);
        AddClientIdServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddClientIdServerRpc(ulong _clientId)
    {
        AddClientIdClientRpc(_clientId);
    }

    [ClientRpc]
    private void AddClientIdClientRpc(ulong _clientId)
    {
        playerIds.Add(_clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PingServerRpc()
    {
        Logger.Log("A client ping Server");
        PingClientRpc();
    }

    [ClientRpc]
    private void PingClientRpc()
    {
        Logger.Log("Server ping Client");
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddTextChatServerRpc(string _text, string _playerName)
    {
        AddTextChatClientRpc(_text, _playerName);
    }

    [ClientRpc]
    private void AddTextChatClientRpc(string text, string _playerName)
    {
        TextChat.Instance.AddText(text, _playerName);
    }
}