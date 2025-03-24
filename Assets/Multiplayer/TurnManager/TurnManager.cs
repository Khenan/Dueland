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

    void Awake()
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
            if (gameManager.IsGameStarted && turnTime.Value < timeTurnDuration)
            {
                turnTime.Value += Time.deltaTime;
                if (turnTime.Value >= timeTurnDuration)
                {
                    NextTurn();
                }
            }
        }
        timerText.text = $"{timeTurnDuration - turnTime.Value:0}";
    }

    private void Init()
    {
        turnTime.Value = timeTurnDuration;
    }

    private void NextTurn()
    {
        turnTime.Value = 0;
        // Next player turn
        int currentPlayerIndex = gameManager.playerIds.IndexOf(turnPlayerId.Value);
        int nextPlayerIndex = (currentPlayerIndex + 1) % gameManager.playerIds.Count;
        turnPlayerId.Value = gameManager.playerIds[nextPlayerIndex];
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
        NextTurn();
        NextTurnClientRpc();
    }

    [ClientRpc]
    private void NextTurnClientRpc()
    {
        if (NetworkManager.Singleton.LocalClientId == turnPlayerId.Value)
        {
            endTurnButton.interactable = true;
        }
    }
}