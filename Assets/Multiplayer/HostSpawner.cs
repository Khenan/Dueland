using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HostSpawner : MonoBehaviour
{
    [SerializeField] List<NetworkObject> networkObjects = new List<NetworkObject>();

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var networkObject in networkObjects)
            {
                NetworkObject _networkObject = Instantiate(networkObject);
                _networkObject.Spawn();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
