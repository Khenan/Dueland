using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Character characterPrefab;
    private Character ownerCharacter;
    private Tile currentHoverTile;
    public override void OnNetworkSpawn()
    {
        Logger.Log("Network PlayerController Spawned and playerId is: " + OwnerClientId);
        if (IsOwner)
        {
            MapManager.onMapInstantiated += OnMapInstantiated;
            SpawnCharacterServerRpc();
        }
    }

    private void OnMapInstantiated()
    {
        ownerCharacter.MoveToTile(MapManager.Instance.GetRandomTile());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCharacterServerRpc()
    {
        Character character = Instantiate(characterPrefab);

        NetworkObject _characterNetworkObject = character.GetComponent<NetworkObject>();
        _characterNetworkObject.SpawnWithOwnership(OwnerClientId);
        ownerCharacter = GameManager.Instance.GetCharacterById(OwnerClientId);
    }

    public void Update()
    {
        if (IsOwner)
        {
            UpdateHoverTile();

            if (TurnManager.Instance != null)
            {
                if (TurnManager.Instance.IsMyTurn)
                {
                    MyTurnUpdate();
                }
            }
        }
    }

    private void UpdateHoverTile()
    {
        if (MapManager.Instance != null)
        {
            Tile _tile = MapManager.Instance.GetTileUnderMouse();
            if (_tile != null)
            {
                if (_tile != currentHoverTile)
                {
                    if (currentHoverTile != null)
                    {
                        currentHoverTile.OnMouseExit();
                    }
                    currentHoverTile = _tile;
                    currentHoverTile.OnMouseEnter();
                }
            }
            else
            {
                if (currentHoverTile != null)
                {
                    currentHoverTile.OnMouseExit();
                    currentHoverTile = null;
                }
            }
        }
    }

    private void MyTurnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Logger.Log("Mouse Clicked");

            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D _hit2D = Physics2D.Raycast(_ray.origin, _ray.direction);
            if (_hit2D.collider != null)
            {
                Logger.Log("2D Collider hit: " + _hit2D.collider.name);
                if (_hit2D.collider.TryGetComponent(out Tile _tile))
                {
                    Vector2Int _currentPosition = ownerCharacter.MatrixPosition.Value;
                    if (MapManager.Instance.GetTileByMatrixPosition(_currentPosition.x, _currentPosition.y).IsAdjacentTo(_tile))
                    {
                        MoveCharacter(_tile);
                    }
                    else
                    {
                        Logger.LogWarning("Selected tile is not adjacent to the current tile.");
                    }
                }
                else Logger.LogWarning("Hit object does not have a Tile component.");
            }
            else Logger.LogWarning("Raycast did not hit any object.");
        }
    }

    private void MoveCharacter(Tile tile)
    {
        // Check if the current turnCharacter has the same owner as this player
        if (TurnManager.Instance.characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject))
        {
            if (_networkObject.OwnerClientId == OwnerClientId)
            {
                if (_networkObject.TryGetComponent(out Character _character))
                {
                    _character.MoveToTile(tile);
                }
            }
            else Logger.LogWarning("Not your turn.");
        }
        else Logger.LogError("Character is null");
    }
}
