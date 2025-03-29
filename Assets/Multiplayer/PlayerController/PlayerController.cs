using System;
using System.Collections.Generic;
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
            SpawnCharacterServerRpc(MultiplayerManager.Instance.SelectedCharacterId);
        }
    }

    private void OnMapInstantiated()
    {
        ownerCharacter.TeleporteToTile(MapManager.Instance.GetRandomTile());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCharacterServerRpc(int _characterId)
    {
        Character _character = Instantiate(characterPrefab);

        NetworkObject _characterNetworkObject = _character.GetComponent<NetworkObject>();
        _characterNetworkObject.SpawnWithOwnership(OwnerClientId);
        ownerCharacter = GameManager.Instance.GetCharacterById(OwnerClientId);
    }

    public void Update()
    {
        if (IsOwner)
        {
            UpdateHoverTile();

            if (TurnManager.Instance != null && TurnManager.Instance.IsMyTurn)
            {
                UpdateTurnHoverTile();
                MyTurnUpdate();
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

                    // Check if the tile has TileData
                    TileDatasCheck(currentHoverTile.MatrixPosition);
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

    private void TileDatasCheck(Vector2Int _matrixPosition)
    {
        if (MapManager.Instance.tileDatas != null)
        {
            MapManager.Instance.GetAllTileDatasByMatrixPosition(_matrixPosition, out List<TileData> _tileDatas);

            // DO CODE

            // Log if the tile has a tileData with the same NetworkObjectId as the ownerCharacter
            if (_tileDatas != null && _tileDatas.Count > 0)
            {
                foreach (TileData _tileData in _tileDatas)
                {
                    if (_tileData.NetworkObjectId == ownerCharacter.NetworkObjectId)
                    {
                        Logger.Log("Tile has a tileData with the same NetworkObjectId as the ownerCharacter.");
                        return;
                    }
                }
            }
            else Logger.Log("Tile does not have any tileData.");
        }
    }

    private List<Tile> currentPath = new List<Tile>();
    private bool canMoveOnCurrentPath = false;
    private int currentPathCost = 0;

    private void UpdateTurnHoverTile()
    {
        canMoveOnCurrentPath = false;
        currentPathCost = 0;
        if (currentPath.Count > 0)
        {
            MapManager.Instance.HidePathVisual();
            currentPath.Clear();
        }

        if (currentHoverTile != null && MapManager.Instance != null)
        {
            MapManager.Instance.FindPath(ownerCharacter.MatrixPosition.Value, currentHoverTile.MatrixPosition, out Tile[] _path, out int _pathCost);
            if (_path != null && _path.Length > 0 && _pathCost <= ownerCharacter.Data.Value.MovePoints)
            {
                MapManager.Instance.ShowPathVisual(_path);
                currentPath.AddRange(_path);
                canMoveOnCurrentPath = true;
                currentPathCost = _pathCost;
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
                    if (canMoveOnCurrentPath)
                    {
                        MoveCharacter(_tile, currentPathCost);
                        currentHoverTile = null;
                    }
                }
                else Logger.LogWarning("Hit object does not have a Tile component.");
            }
            else Logger.LogWarning("Raycast did not hit any object.");
        }
    }

    private void MoveCharacter(Tile _tile, int _pathCost)
    {
        // Check if the current turnCharacter has the same owner as this player
        if (TurnManager.Instance.characterReferenceTurn.Value.TryGet(out NetworkObject _networkObject))
        {
            if (_networkObject.OwnerClientId == OwnerClientId)
            {
                if (_networkObject.TryGetComponent(out Character _character))
                {
                    _character.MoveToTile(_tile, _pathCost);
                }
            }
            else Logger.LogWarning("Not your turn.");
        }
        else Logger.LogError("Character is null");
    }
}
