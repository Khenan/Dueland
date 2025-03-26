using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Character characterPrefab;
    private Character character;

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network PlayerController Spawned and playerId is: " + OwnerClientId);
        if (IsOwner)
        {
            SpawnCharacterServerRpc();
        }
    }

    [ServerRpc]
    public void SpawnCharacterServerRpc()
    {
        Character _character = Instantiate(characterPrefab);
        character = _character;

        NetworkObject _characterNetworkObject = _character.GetComponent<NetworkObject>();
        _characterNetworkObject.SpawnWithOwnership(OwnerClientId);

        SpawnCharacterClientRpc();
    }

    [ClientRpc]
    public void SpawnCharacterClientRpc()
    {
        Character[] _characters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (var _character in _characters)
        {
            if (_character.OwnerClientId == OwnerClientId)
            {
                character = _character;
                break;
            }
        }
    }

    public void Update()
    {
        if (IsOwner)
        {
            if (TurnManager.Instance != null)
            {
                if (TurnManager.Instance.IsMyTurn)
                {
                    MyTurnUpdate();
                }
            }
        }
    }

    private void MyTurnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Logger.Log("Mouse Clicked");

            if (Camera.main == null)
            {
                Logger.LogError("Main Camera is not found.");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D _hit2D = Physics2D.Raycast(ray.origin, ray.direction);
            if (_hit2D.collider != null)
            {
                Logger.Log("2D Collider hit: " + _hit2D.collider.name);
                if (_hit2D.collider.TryGetComponent(out Tile _tile))
                {
                    MoveCharacter(_tile);
                }
                else
                {
                    Logger.LogWarning("Hit object does not have a Tile component.");
                }

                Debug.DrawLine(ray.origin, _hit2D.point, Color.red, 2f);
            }
            else
            {
                Logger.LogWarning("Raycast did not hit any object.");
            }
        }
    }

    private void MoveCharacter(Tile tile)
    {
        if (character != null)
        {
            character.MoveToTile(tile);
        }
        else
        {
            Logger.LogError("Character is null");
        }
    }
}
