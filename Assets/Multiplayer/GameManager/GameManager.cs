using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<bool> IsGameStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> allPlayersConnected = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> allCharactersSpawned = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<ulong> playerIds = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<NetworkObjectReference> characters = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float GameTime => gameTime.Value;

    [SerializeField] private int maxPlayers = 2;
    public int MaxPlayers => maxPlayers;

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
    public Action OnAllCharactersSpawned { get; set; }

    #region Colors Characters

    public Color[] colors = new Color[4]
    {
        new Color(0.8f, 0.2f, 0.2f),
        new Color(0.2f, 0.2f, 0.8f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.8f, 0.8f, 0.2f)
    };

    #endregion

    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            gameTime.Value += Time.deltaTime;
        }
    }

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network GameManager Spawned");
        LoadingPanel.Instance.ObjectLoaded();
        onGameManagerSpawned?.Invoke(this);
        AddClientIdServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public Character GetCharacterById(ulong _clientId)
    {
        foreach (NetworkObjectReference _characterReference in characters)
        {
            if (_characterReference.TryGet(out NetworkObject _networkObject))
            {
                if (_networkObject.OwnerClientId == _clientId)
                {
                    Debug.Log("GetCharacterById: " + _clientId);
                    return _networkObject.GetComponent<Character>();
                }
            }
        }
        Debug.Log($"Any Character found for clientId: {_clientId}");
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddClientIdServerRpc(ulong _clientId)
    {
        playerIds.Add(_clientId);
        if (playerIds.Count == maxPlayers)
        {
            allPlayersConnected.Value = true;
            AllPlayersConnectedClientRpc();
        }
    }

    [ClientRpc]
    private void AllPlayersConnectedClientRpc()
    {
        Logger.Log("All players connected");
        OnAllPlayersConnected?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void AddCharacterServerRpc(NetworkObjectReference _character)
    {
        characters.Add(_character);
        if (characters.Count == maxPlayers)
        {
            allCharactersSpawned.Value = true;
            IsGameStarted.Value = true;
            OnAllCharactersSpawned?.Invoke();
        }
    }

    #region TextChat

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

    #endregion
}
