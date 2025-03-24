using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<FixedString4096Bytes> compressedString = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<List<ulong>> playerIds = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float GameTime => gameTime.Value;

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

    public List<ulong> PlayerIds => playerIds.Value;

    public bool IsGameStarted { get; private set; }
    public Action OnAllPlayersConnected { get; set; }

    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            gameTime.Value += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PingServerRpc();
        }

        if (!IsGameStarted && MultiplayerManager.Instance.JoinedLobby.Players.Count == 1)
        {
            IsGameStarted = true;
            OnAllPlayersConnected?.Invoke();
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
            Debug.Log("Compressed string: " + compressedString.Value.ToString());
            Debug.Log("Compressed string: " + compressedString.Value);
            
            GenerateMap(map);
        }
    }

    private void GenerateMap(byte[] map)
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.position = new Vector3(i, 0, j);

                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                switch (map[i * 10 + j])
                {
                    case 1: // Grass
                        renderer.material.color = Color.green;
                        break;
                    case 2: // Dirt
                        renderer.material.color = new Color(0.545f, 0.271f, 0.075f); // Brown color
                        break;
                    case 3: // Water
                        renderer.material.color = Color.blue;
                        break;
                }
            }
        }
    }

    private FixedString4096Bytes ConvertTilesToString(byte[] tiles)
    {
        return new FixedString4096Bytes(string.Join(",", tiles));
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Network GameManager Spawned");
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
        playerIds.Value.Add(_clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PingServerRpc()
    {
        Debug.Log("A client ping Server");
        PingClientRpc();
    }

    [ClientRpc]
    private void PingClientRpc()
    {
        Debug.Log("Server ping Client");
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