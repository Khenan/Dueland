using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InitiativeCanvas : MonoBehaviour
{
    [SerializeField] private InitiativeSlot initiativeSlotPrefab;
    [SerializeField] private Transform slotsParent;

    private List<InitiativeSlot> slots = new List<InitiativeSlot>();

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            PrepareToInit(GameManager.Instance);
        }
        else
        {
            GameManager.onGameManagerSpawned += (GameManager _gameManager) =>
            {
                PrepareToInit(_gameManager);
            };
        }
    }

    private void PrepareToInit(GameManager _gameManager)
    {
        if (_gameManager.characters.Count == _gameManager.MaxPlayers)
        {
            Init();
        }
        else
        {
            _gameManager.characters.OnListChanged += InitIfAllCharactersSpawned;
        }
    }

    private void InitIfAllCharactersSpawned(NetworkListEvent<NetworkObjectReference> _changeEvent)
    {
        if (GameManager.Instance.characters.Count == GameManager.Instance.MaxPlayers)
        {
            Init();
        }
    }

    private void Init()
    {
        for (int i = 0; i < GameManager.Instance.characters.Count; i++)
        {
            InitiativeSlot _slot = Instantiate(initiativeSlotPrefab, slotsParent);
            slots.Add(_slot);
            if (GameManager.Instance.characters[i].TryGet(out NetworkObject _networkObject))
            {
                if (_networkObject.TryGetComponent(out Character _character))
                {
                    _slot.Init(_character);
                }
            }
        }

        TurnManager.onNextTurn += UpdateSlots;
    }

    private void UpdateSlots(ulong _clientId)
    {
        Logger.Log("Update Slots: " + _clientId);
        foreach (InitiativeSlot _slot in slots)
        {
            Logger.Log("Slot ClientId: " + _slot.Character.OwnerClientId);
            Logger.Log("Current ClientId: " + _clientId);
            _slot.SetOutline(_slot.Character.OwnerClientId == _clientId);
        }
    }
}