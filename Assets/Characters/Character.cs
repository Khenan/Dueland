using UnityEngine;
using Unity.Netcode;

public class Character : NetworkBehaviour
{
    private NetworkVariable<Vector2Int> matrixPosition = new NetworkVariable<Vector2Int>(new Vector2Int(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void MoveToTileServerRpc(int _x, int _y)
    {
        matrixPosition.Value = new Vector2Int(_x, _y);
        Tile _tile = MapManager.Instance.GetTileByMatrixPosition(_x, _y);
        transform.position = _tile.transform.position;
    }

    public void MoveToTile(Tile _tile)
    {
        if (IsOwner)
        {
            Logger.Log("MoveToTile called by owner.");
            MoveToTileServerRpc(_tile.MatrixPosition.x, _tile.MatrixPosition.y);
        }
        else
        {
            Logger.Log("MoveToTile called by non-owner.");
        }
    }
}