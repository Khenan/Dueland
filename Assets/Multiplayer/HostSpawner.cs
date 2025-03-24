using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class HostSpawner : MonoBehaviour
{
    [SerializeField] List<NetworkObject> networkObjects = new List<NetworkObject>();

    async Task Start()
    {
        if (LoadingPanel.Instance != null)
        {
            LoadingPanel.Instance.AddLoadingObjects(networkObjects.Count);
        }

        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var networkObject in networkObjects)
            {
                if (LoadingPanel.Instance != null)
                {
                    LoadingPanel.Instance.SetLoadingText($"Loading {networkObject.name}");
                }

                NetworkObject[] _networkObject = await InstantiateAsync(networkObject);

                if (LoadingPanel.Instance != null)
                {
                    LoadingPanel.Instance.ObjectLoaded();
                }
                _networkObject[0].Spawn();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
