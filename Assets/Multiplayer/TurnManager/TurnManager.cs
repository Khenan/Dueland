using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class TurnManager : NetworkBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private float timeTurnDuration = 30;
    [SerializeField] private TextMeshProUGUI timerText;
    private NetworkVariable<float> turnTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<ulong> turnPlayerId = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private GameManager gameManager;

    public static TurnManager Instance { get; private set; }
    public bool IsMyTurn => NetworkManager.Singleton.LocalClientId == turnPlayerId.Value;

    void Awake()
    {
        Instance = this;

        if (NetworkManager.Singleton.IsServer)
        {
            if (GameManager.Instance != null)
            {
                gameManager = GameManager.Instance;
                gameManager.OnAllPlayersConnected += Init;
            }
            else
            {
                GameManager.onGameManagerSpawned += (GameManager _gameManager) =>
                {
                    gameManager = _gameManager;
                    _gameManager.OnAllPlayersConnected += Init;
                };
            }
        }
    }

    void OnEnable()
    {
        if (NetworkManager.Singleton.LocalClientId == turnPlayerId.Value)
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
            if (gameManager.IsGameStarted.Value && turnTime.Value < timeTurnDuration)
            {
                turnTime.Value += Time.deltaTime;
                if (turnTime.Value >= timeTurnDuration)
                {
                    NextTurnServerRpc();
                }
            }
        }
        timerText.text = $"{timeTurnDuration - turnTime.Value:0}";

        if (Input.GetKeyDown(KeyCode.T))
        {
            Logger.Log($"my id: {NetworkManager.Singleton.LocalClientId}");
            Logger.Log($"Turn id: {turnPlayerId.Value}");
        }
    }

    public override void OnNetworkSpawn()
    {
        LoadingPanel.Instance.ObjectLoaded();
        if (NetworkManager.Singleton.IsServer)
        {
            turnPlayerId.Value = gameManager.playerIds[0];
            turnTime.Value = 0;
        }
    }

    private void Init()
    {
        turnTime.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void NextTurnServerRpc()
    {
        turnTime.Value = 0;
        // Next player turn
        int currentPlayerIndex = gameManager.playerIds.IndexOf(turnPlayerId.Value);
        int nextPlayerIndex = (currentPlayerIndex + 1) % gameManager.playerIds.Count;
        turnPlayerId.Value = gameManager.playerIds[nextPlayerIndex];
        NextTurnClientRpc(turnPlayerId.Value);
    }

    public void EndTurn()
    {
        if (NetworkManager.Singleton.LocalClientId == turnPlayerId.Value)
        {
            endTurnButton.interactable = false;
            EndTurnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndTurnServerRpc()
    {
        NextTurnServerRpc();
    }

    [ClientRpc]
    private void NextTurnClientRpc(ulong _turnPlayerId)
    {
        if (NetworkManager.Singleton.LocalClientId == _turnPlayerId)
        {
            endTurnButton.interactable = true;
        }
    }
}