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
    public static Action<ulong> onNextTurn;
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
    }

    public override void OnNetworkSpawn()
    {
        LoadingPanel.Instance.ObjectLoaded();

        if (NetworkManager.Singleton.IsServer)
        {
            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance;

                if (GameManager.Instance.allCharactersSpawned.Value)
                {
                    Init();
                }
                else
                {
                    gameManager.OnAllCharactersSpawned += Init;
                }
            }
            else
            {
                GameManager.onGameManagerSpawned += (GameManager _gameManager) =>
                {
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
            int _randomIndex = Random.Range(0, gameManager.characters.Count);
            characterReferenceTurn.Value = gameManager.characters[_randomIndex];
            turnTime.Value = timeTurnDuration;
            isInitialized.Value = true;

            // Get the client ID of the character that starts the turn
            ulong _clientId = characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject) ? _networkObject.OwnerClientId : 0;
            NextTurnClientRpc(_clientId);
            StartTurnClientRpc();
        }
    }

    [ClientRpc]
    private void StartTurnClientRpc()
    {
        if (isInitialized.Value)
        {
            endTurnButton.interactable = IsMyTurn;
            onInitialization?.Invoke();
        }
        else
        {
            characterReferenceTurn.OnValueChanged += (NetworkObjectReference _previousValue, NetworkObjectReference _newValue) =>
            {
                if (_newValue.TryGet(out NetworkObject _networkObject))
                {
                    endTurnButton.interactable = NetworkManager.Singleton.LocalClientId == _networkObject.OwnerClientId;
                }
                onInitialization?.Invoke();
            };
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NextTurnServerRpc()
    {
        turnTime.Value = timeTurnDuration;

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
        onNextTurn?.Invoke(_clientId);
    }
}