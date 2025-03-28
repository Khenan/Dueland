using UnityEngine;
using Unity.Netcode;
using System;

[Serializable]
public class Character : NetworkBehaviour
{
    private NetworkVariable<Vector2Int> matrixPosition = new NetworkVariable<Vector2Int>(new Vector2Int(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Color Color { get; private set; }

    public CharacterData Data { get; private set; }

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);
        Data = new CharacterData(10);

        if (IsOwner)
        {
            GameManager.Instance.AddCharacterServerRpc(GetComponent<NetworkObject>());
        }

        if((int)OwnerClientId < GameManager.Instance.colors.Length)
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

    public void MoveToTile(Tile _tile)
    {
        if (IsOwner)
        {
            matrixPosition.Value = new Vector2Int(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
            Logger.Log("MoveToTile called by owner.");
            MoveToTileServerRpc(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
        }
        else
        {
            Logger.Log("MoveToTile called by non-owner.");
        }
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
