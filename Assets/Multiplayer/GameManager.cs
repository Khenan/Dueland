using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<bool> IsGameStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<ulong> playerIds = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
