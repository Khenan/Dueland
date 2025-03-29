using UnityEngine;
using Unity.Netcode;
using System;

[Serializable]
public class Character : NetworkBehaviour
{
    private NetworkVariable<Vector2Int> matrixPosition = new NetworkVariable<Vector2Int>(new Vector2Int(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector2Int> MatrixPosition => matrixPosition;
    public Color Color { get; private set; }

    public CharacterData Data { get; private set; }

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);
        Data = new CharacterData(10, 3);

        if (IsOwner)
        {
            GameManager.Instance.AddCharacterServerRpc(GetComponent<NetworkObject>());
        }

        if ((int)OwnerClientId < GameManager.Instance.colors.Length)
            ColorizeCharacter(GameManager.Instance.colors[OwnerClientId]);
        else
            Logger.LogError("OwnerClientId is out of bounds for colors array: " + OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void MoveToTileServerRpc(int _x, int _y)
    {
        Tile _tile = MapManager.Instance.GetTileByMatrixPosition(_x, _y);
        transform.position = _tile.transform.position;
    }

    public void TeleporteToTile(Tile _tile)
    {
        if (IsOwner)
        {
            Logger.Log("TeleporteToTile called by owner.");
            matrixPosition.Value = new Vector2Int(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
            MoveToTileServerRpc(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
        }
        else
        {
            Logger.Log("TeleporteToTile called by non-owner.");
        }
    }

    public bool MoveToTile(Tile _tile)
    {
        bool _success = false;
        if (IsOwner)
        {
            Logger.Log("MoveToTile called by owner.");
            // Get distance to tile
            int _heuristicDistance = Mathf.Abs(_tile.MatrixPosition.x - matrixPosition.Value.x) + Mathf.Abs(_tile.MatrixPosition.y - matrixPosition.Value.y);
            if (Data.Move(_heuristicDistance))
            {
                _success = true;
                matrixPosition.Value = new Vector2Int(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
                MoveToTileServerRpc(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
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
        SpriteRenderer _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _color;
        }
        else
        {
            Logger.LogError("SpriteRenderer not found on character.");
        }
    }
}
