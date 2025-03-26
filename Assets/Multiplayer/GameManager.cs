using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<bool> IsGameStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<ulong> playerIds = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float GameTime => gameTime.Value;

    [SerializeField] private int maxPlayers = 2;

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
    public bool allPlayersConnected;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PingServerRpc();
        }

        if (NetworkManager.Singleton.IsServer)
        {
            gameTime.Value += Time.deltaTime;
        }
    }

    private void AllPlayersConnected()
    {
        Logger.Log("All players connected");
        OnAllPlayersConnected?.Invoke();
        allPlayersConnected = true;
        IsGameStarted.Value = true;
    }

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network GameManager Spawned");
        LoadingPanel.Instance.ObjectLoaded();
        onGameManagerSpawned?.Invoke(this);
        AddClientIdServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddClientIdServerRpc(ulong _clientId)
    {
        playerIds.Add(_clientId);
        if(playerIds.Count == maxPlayers)
        {
            AllPlayersConnected();
        }
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
