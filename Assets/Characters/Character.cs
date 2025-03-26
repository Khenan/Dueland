using UnityEngine;
using Unity.Netcode;

public class Character : NetworkBehaviour
{
    private NetworkVariable<Vector2> matrixPosition = new NetworkVariable<Vector2>(new Vector2(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void MoveToTileServerRpc(Vector2Int _matrixPosition)
    {
        matrixPosition.Value = _matrixPosition;
        Tile _tile = MapManager.Instance.GetTileByMatrixPosition(_matrixPosition);
        transform.position = _tile.transform.position;
        Logger.Log("Character MoveToTileServerRpc: " + _matrixPosition);
    }

    [ClientRpc]
    internal void MoveToTileClientRpc(Vector2 _matrixPosition)
    {
        matrixPosition.Value = _matrixPosition;
        Tile _tile = MapManager.Instance.GetTileByMatrixPosition(_matrixPosition);
        transform.position = _tile.transform.position;
        Logger.Log("Character MoveToTileClientRpc: " + _matrixPosition);
    }

    public void MoveToTile(Tile _tile)
    {
        if (IsOwner)
        {
            Logger.Log("MoveToTile called by owner.");
            MoveToTileServerRpc(_tile.MatrixPosition);
        }
        else
        {
            Logger.Log("MoveToTile called by non-owner.");
        }
    }
}