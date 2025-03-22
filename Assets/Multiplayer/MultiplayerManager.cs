using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{

    #region Encryption

    [Serializable]
    public enum EncryptionType
    {
        DTLS, // Datagram Transport Layer Security
        WSS // Web Socket Secure
    }

    private const string KEY_JOIN_CODE = "RelayJoinCode";
    private const string DTLS_ENCRYPTION = "dtls"; // Datagram Transport Layer Security
    private const string WSS_ENCRYPTION = "wss"; // Web Socket Secure, use for WebGl builds

    private EncryptionType encryption = EncryptionType.DTLS;
    private string ConnectionType => encryption == EncryptionType.DTLS ? DTLS_ENCRYPTION : WSS_ENCRYPTION;

    #endregion

    // Player
    public string playerName = string.Empty;
    public string PlayerId { get; private set; }

    private int maxPlayers = 2;

    // Lobby
    private Lobby joinedLobby;
    public Lobby JoinedLobby => joinedLobby;

    // UI
    [SerializeField] private GameObject lobbyUI;

    #region Singleton

    private MultiplayerManager instance;
    public MultiplayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<MultiplayerManager>();
                if (instance == null)
                {
                    instance = new GameObject("MultiplayerManager").AddComponent<MultiplayerManager>();
                }
            }
            return instance;
        }
    }

    #endregion

    public bool Initialized { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private async void Start()
    {
        Debug.Log("Initializing MultiplayerManager...");
        await Authenticate();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (joinedLobby != null)
            {
                PrintPlayers();
            }
        }
    }
    
    public void PrintPlayers()
    {
        Lobby _lobby = joinedLobby;
        foreach (Player _player in _lobby.Players)
        {
            Debug.Log("Player: " + _player.Id + " | " + _player.Data["PlayerName"].Value);
        }
    }

    private async Task Authenticate()
    {
        await Authenticate("Player");
    }

    private async Task Authenticate(string _playerName)
    {
        Debug.Log("Authenticating...");
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions _options = new();
            _options.SetProfile(_playerName);

            await UnityServices.InitializeAsync(_options);
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as" + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (playerName == string.Empty)
            {
                playerName = "Anonymous#" + UnityEngine.Random.Range(1, 1000);
            }
            PlayerId = AuthenticationService.Instance.PlayerId;
        }
        Debug.Log("Authenticating done!");

        Initialized = true;
    }

    public void HostLobby()
    {
        Debug.Log("Creating lobby...");
        CreateLobbyOptions _lobbyOptions = new CreateLobbyOptions()
        {
            Player = GetPlayer(),
            Data = new()
        };
        CreateCustomLobby("Lobby", maxPlayers, _lobbyOptions);
        lobbyUI.SetActive(false);
    }

    public void JoinLobby()
    {
        Debug.Log("Joining lobby...");
        QuickJoinLobby();
        lobbyUI.SetActive(false);
    }

    public Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerName",
                        new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                    },
                    {
                        "Ready",
                        new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "false")
                    }
                }
        };
    }

    public async Task<bool> CreateCustomLobby(string _lobbyName, int _maxPlayers, CreateLobbyOptions _options, Action _callback = null)
    {
        bool _result = false;
        try
        {
            Allocation _allocation = await AllocateRelay();
            string _relayJoinCode = await GetRelayJoinCode(_allocation);

            _options.Data.Add(KEY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, _relayJoinCode));
            Lobby _lobby = await LobbyService.Instance.CreateLobbyAsync(
                _lobbyName,
                _maxPlayers,
                _options
            );
            Debug.Log("Lobby created: " + _lobby.Name + " with code: " + _lobby.LobbyCode);

            RelayServerData _relayServerData = AllocationUtils.ToRelayServerData(
                _allocation,
                ConnectionType
            );
            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(_relayServerData);

            NetworkManager.Singleton.StartHost();
            _result = true;
            joinedLobby = _lobby;
            _callback?.Invoke();
        }
        catch (LobbyServiceException _e)
        {
            Debug.LogError("Failed to create lobby: " + _e.Message);
        }
        return Task.FromResult(_result).Result;
    }

    public async Task<bool> QuickJoinLobby()
    {
        if (joinedLobby != null)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                await DeleteLobby();
            }
            else
            {
                await LeaveLobby();
            }
        }
        bool _result = false;
        try
        {
            QuickJoinLobbyOptions _options =
                new()
                {
                    Filter = new List<QueryFilter>()
                    {
                            new QueryFilter(
                                QueryFilter.FieldOptions.AvailableSlots,
                                "0",
                                QueryFilter.OpOptions.GT
                            )
                    },
                    Player = GetPlayer()
                };
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(_options);

            string _relayJoinCode = joinedLobby.Data[KEY_JOIN_CODE].Value;
            JoinAllocation _joinAllocation = await JoinRelay(_relayJoinCode);

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    AllocationUtils.ToRelayServerData(_joinAllocation, ConnectionType)
                );
            NetworkManager.Singleton.StartClient();
            Debug.Log("Quick joined lobby: " + joinedLobby.Name + " with code: " + joinedLobby.LobbyCode + (joinedLobby.IsLocked ? "Locked" : "Unlocked"));
            _result = true;
        }
        catch (LobbyServiceException _e)
        {
            Debug.LogError("Failed to quick join lobby: " + _e.Message);
        }
        return Task.FromResult(_result).Result;
    }

    public async Task<bool> LeaveLobby()
    {
        bool _result = false;
        try
        {
            bool _isPublicLobby = false;
            if (joinedLobby != null)
            {
                _isPublicLobby = joinedLobby.Data["Type"].Value == "public";
                if (NetworkManager.Singleton.IsHost)
                {
                    await DeleteLobby();
                }
                else
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, PlayerId);
                }
            }
            NetworkManager.Singleton.Shutdown();
            joinedLobby = null;
            Debug.Log("Left lobby");
            _result = true;
        }
        catch (LobbyServiceException _exception)
        {
            Debug.Log("Failed to leave lobby: " + _exception.Message);
        }
        return Task.FromResult(_result).Result;
    }

    private async Task DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            NetworkManager.Singleton.Shutdown();
            joinedLobby = null;
            Debug.Log("Deleted lobby !");
        }
        catch (LobbyServiceException _exception)
        {
            Debug.LogError("Failed to delete lobby: " + _exception.Message);
        }
    }

    #region Relay

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation _allocation = await RelayService.Instance.CreateAllocationAsync(
                maxPlayers > 1 ? maxPlayers - 1 : maxPlayers
            );
            return _allocation;
        }
        catch (RelayServiceException _e)
        {
            Debug.LogError("Failed to allocate relay: " + _e.Message);
            return default;
        }
    }


    private async Task<string> GetRelayJoinCode(Allocation _allocation)
    {
        try
        {
            string _relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(
                _allocation.AllocationId
            );
            return _relayJoinCode;
        }
        catch (RelayServiceException _e)
        {
            Debug.LogError("Failed to get relay join code: " + _e.Message);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string _relayJoinCode)
    {
        try
        {
            JoinAllocation _joinAllocation = await RelayService.Instance.JoinAllocationAsync(
                _relayJoinCode
            );
            return _joinAllocation;
        }
        catch (RelayServiceException _e)
        {
            Debug.LogError("Failed to join relay: " + _e.Message);
            return default;
        }
    }

    #endregion

}
