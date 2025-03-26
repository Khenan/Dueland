using System;
using Unity.Netcode;

public class Character : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);
    }

    internal void MoveToTile(Tile _tile)
    {
        Logger.Log("Character MoveToTile: " + _tile.MatrixPosition);
        transform.position = _tile.transform.position;
    }
}