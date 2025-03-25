using Unity.Netcode;

public class Character : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Logger.Log("Network Character Spawned and playerId is: " + OwnerClientId);
    }
}