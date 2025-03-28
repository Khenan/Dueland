using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TurnManager : NetworkBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private float timeTurnDuration = 30;
    [SerializeField] private TextMeshProUGUI timerText;

    public NetworkVariable<float> turnTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isInitialized = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<NetworkObjectReference> characterReferenceTurn = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameManager gameManager;
    public bool IsMyTurn
    {
        get
        {
            bool _isMyTurn = false;
            if (characterReferenceTurn != null && characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject))
            {
                _isMyTurn = NetworkManager.Singleton.LocalClientId == _networkObject.OwnerClientId;
            }
            return _isMyTurn;
        }
    }
    public static Action<ulong> onEndTurn;
    public static TurnManager Instance { get; private set; }
    public static Action onInitialization;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        if (IsMyTurn)
        {
            endTurnButton.interactable = true;
        }
        else
        {
            endTurnButton.interactable = false;
        }
        endTurnButton.onClick.AddListener(EndTurn);
    }

    void OnDisable()
    {
        endTurnButton.onClick.RemoveListener(EndTurn);
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost && gameManager != null)
        {
            if (gameManager.IsGameStarted.Value && turnTime.Value > 0)
            {
                turnTime.Value -= Time.deltaTime;
                if (turnTime.Value <= 0)
                {
                    NextTurnServerRpc();
                }
            }
        }
        timerText.text = $"{turnTime.Value:0}";

        if (Input.GetKeyDown(KeyCode.T))
        {
            Logger.Log($"my id: {NetworkManager.Singleton.LocalClientId}");
            if (characterReferenceTurn != null && characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject))
            {
                Logger.Log($"Turn id: {_networkObject.OwnerClientId}");
            }
            // Log characters count
            Logger.Log("gameManager.characters.Count: " + GameManager.Instance.characters.Count);
        }
    }

    public override void OnNetworkSpawn()
    {
        LoadingPanel.Instance.ObjectLoaded();

        Logger.Log("TurnManager OnNetworkSpawn");

        if (NetworkManager.Singleton.IsServer)
        {
            Logger.Log("TurnManager OnNetworkSpawn IsServer");
            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance;

                if (GameManager.Instance.allCharactersSpawned.Value)
                {
                    Logger.Log("TurnManager GameManager.Instance.allCharactersSpawned.Value");
                    Init();
                }
                else
                {
                    Logger.Log("TurnManager GameManager.Instance.allCharactersSpawned.Value else");
                    gameManager.OnAllCharactersSpawned += Init;
                }
            }
            else
            {
                Logger.Log("TurnManager OnNetworkSpawn IsServer");
                GameManager.onGameManagerSpawned += (GameManager _gameManager) =>
                {
                    Logger.Log("TurnManager _gameManager OnAllCharactersSpawned");
                    gameManager = _gameManager;
                    _gameManager.OnAllCharactersSpawned += Init;
                };
            }
        }
    }

    private void Init()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("TurnManager Init IsServer");
            characterReferenceTurn.Value = gameManager.characters[Random.Range(0, gameManager.characters.Count)];
            turnTime.Value = timeTurnDuration;
            isInitialized.Value = true;
            StartTurnClientRpc();
        }
    }

    [ClientRpc]
    private void StartTurnClientRpc()
    {
        Logger.Log("StartTurnClientRpc gameManager.characters.Count: " + GameManager.Instance.characters.Count);
        Logger.Log($"my id: {NetworkManager.Singleton.LocalClientId}");
        if (characterReferenceTurn != null && characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject))
        {
            Logger.Log($"Turn id: {_networkObject.OwnerClientId}");
        }
        Logger.Log("TurnManager StartTurnClientRpc IsMyTurn: " + IsMyTurn);
        endTurnButton.interactable = IsMyTurn;
        onInitialization?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void NextTurnServerRpc()
    {
        turnTime.Value = timeTurnDuration;

        Logger.Log("gameManager.characters.Count: " + gameManager.characters.Count);

        // Next player turn

        int currentCharacterIndex = gameManager.characters.IndexOf(characterReferenceTurn.Value);
        int nextCharacterIndex = (currentCharacterIndex + 1) % gameManager.characters.Count;
        characterReferenceTurn.Value = gameManager.characters[nextCharacterIndex];
        ulong _clientId = characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject) ? _networkObject.OwnerClientId : 0;
        NextTurnClientRpc(_clientId);
    }

    public void EndTurn()
    {
        if (IsMyTurn)
        {
            endTurnButton.interactable = false;
            NextTurnServerRpc();
        }
    }

    [ClientRpc]
    private void NextTurnClientRpc(ulong _clientId)
    {
        endTurnButton.interactable = NetworkManager.Singleton.LocalClientId == _clientId;
        onEndTurn?.Invoke(_clientId);
    }
}