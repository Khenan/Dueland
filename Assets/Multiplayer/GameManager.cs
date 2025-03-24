using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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

    private List<ulong> playerIds = new List<ulong>();
    public List<ulong> PlayerIds => playerIds;
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
        playerIds.Add(_clientId);
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