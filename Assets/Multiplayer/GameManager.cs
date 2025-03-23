using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    NetworkObject networkObject;

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        if (NetworkManager.Singleton.IsServer)
        {
            networkObject.Spawn();
        }
    }
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Network Object Spawned");
    }
}
