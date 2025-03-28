using System;
using Unity.Netcode;
using UnityEngine;

public class InitiativeCanvas : MonoBehaviour
{
    [SerializeField] private InitiativeSlot initiativeSlotPrefab;
    [SerializeField] private Transform slotsParent;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            Logger.Log("Start InitiativeCanvas GameManager.Instance.characters.Count: " + GameManager.Instance.characters.Count);
            if (GameManager.Instance.characters.Count == GameManager.Instance.MaxPlayers)
            {
                Init();
            }
            else
            {
                GameManager.Instance.characters.OnListChanged += InitIfAllCharactersSpawned;
            }
        }
        else
        {
            Logger.Log("Start InitiativeCanvas GameManager.Instance is null");
            GameManager.onGameManagerSpawned += (GameManager _gameManager) =>
            {
                Logger.Log("Start InitiativeCanvas GameManager.onGameManagerSpawned");
                if (_gameManager.characters.Count == _gameManager.MaxPlayers)
                {
                    Init();
                }
                else
                {
                    _gameManager.characters.OnListChanged += InitIfAllCharactersSpawned;
                }
            };
        }
    }

    private void InitIfAllCharactersSpawned(NetworkListEvent<NetworkObjectReference> _changeEvent)
    {
        Logger.Log("InitIfAllCharactersSpawned GameManager.Instance.characters.Count: " + GameManager.Instance.characters.Count);
        if (GameManager.Instance.characters.Count == GameManager.Instance.MaxPlayers)
        {
            Init();
        }
    }

    private void Init()
    {
        Logger.Log("Init GameManager.Instance.characters.Count: " + GameManager.Instance.characters.Count);
        for (int i = 0; i < GameManager.Instance.characters.Count; i++)
        {
            InitiativeSlot _slot = Instantiate(initiativeSlotPrefab, slotsParent);

            if (GameManager.Instance.characters[i].TryGet(out NetworkObject _networkObject))
            {
                if (_networkObject.TryGetComponent(out Character _character))
                {
                    _slot.Init(_character);
                }
            }
        }
    }
}