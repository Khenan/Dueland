using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Character characterPrefab;

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
        NetworkObject _character = Instantiate(characterPrefab).GetComponent<NetworkObject>();
        _character.SpawnWithOwnership(OwnerClientId);
    }
}
