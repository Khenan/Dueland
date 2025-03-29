using UnityEngine;
using Unity.Netcode;
using System;

[Serializable]
public class Character : NetworkBehaviour
{
    private NetworkVariable<Vector2Int> matrixPosition = new NetworkVariable<Vector2Int>(new Vector2Int(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector2Int> MatrixPosition => matrixPosition;
    public Color Color { get; private set; }

    public SpriteRenderer spriteRenderer;

    public NetworkVariable<CharacterData> Data = new NetworkVariable<CharacterData>(new CharacterData(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);

        if (IsOwner)
        {
            GameManager.Instance.AddCharacterServerRpc(GetComponent<NetworkObject>());
            TurnManager.onNextTurn += OnNextTurn;
            Data.Value = MultiplayerManager.Instance.CharacterDataBase.GetCharacterDataById(MultiplayerManager.Instance.SelectedCharacterId);
        }

        ColorizeCharacter(Data.Value.Color);
    }

    private void OnNextTurn(ulong _previousClientId, ulong _currentClientId)
    {
        Logger.Log("Character -> OnNextTurn called. Previous ClientId: " + _previousClientId + ", Current ClientId: " + _currentClientId);
        if (IsOwner && OwnerClientId == _currentClientId)
        {
            Logger.Log("MovePoints: " + Data.Value.MovePoints);
            CharacterData _data = Data.Value;
            _data.MovePoints = _data.MovePointsMax;
            Data.Value = _data;
            Logger.Log("MovePoints: " + Data.Value.MovePoints);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void MoveToTileServerRpc(int _x, int _y)
    {
        Tile _tile = MapManager.Instance.GetTileByMatrixPosition(_x, _y);
        transform.position = _tile.transform.position;
        MapManager.Instance.SetTileDataServerRpc(_tile.MatrixPosition.x, _tile.MatrixPosition.y, GetComponent<NetworkObject>().NetworkObjectId, false);
    }

    public void TeleporteToTile(Tile _tile)
    {
        if (IsOwner)
        {
            Logger.Log("TeleporteToTile called by owner.");
            MoveToTileSynchronized(_tile);
        }
        else
        {
            Logger.Log("TeleporteToTile called by non-owner.");
        }
    }

    private void MoveToTileSynchronized(Tile _tile)
    {
        MapManager.Instance.RemoveTileDataServerRpc(matrixPosition.Value.x, matrixPosition.Value.y);
        matrixPosition.Value = new Vector2Int(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
        MoveToTileServerRpc(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
    }

    private bool ConsumeMovePoints(int _distance)
    {
        if (Data.Value.MovePoints >= _distance)
        {
            CharacterData _data = Data.Value;
            _data.MovePoints -= _distance;
            Data.Value = _data;
            return true;
        }
        else
        {
            Logger.Log("Not enough move points to move.");
            return false;
        }
    }

    public bool MoveToTile(Tile _tile, int _pathCost)
    {
        bool _success = false;
        if (IsOwner)
        {
            Logger.Log("MoveToTile called by owner.");
            if (ConsumeMovePoints(_pathCost))
            {
                _success = true;
                MoveToTileSynchronized(_tile);
            }
        }
        else
        {
            Logger.Log("MoveToTile called by non-owner.");
        }
        return _success;
    }

    internal void ColorizeCharacter(Color _color)
    {
        Color = _color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = _color;
        }
        else
        {
            Logger.LogError("SpriteRenderer not found on character.");
        }
    }
}
