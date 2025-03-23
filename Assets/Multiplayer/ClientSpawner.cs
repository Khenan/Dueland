using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientSpawner : MonoBehaviour
{
    [SerializeField] List<NetworkObject> networkObjects = new List<NetworkObject>(); // Ensure that the objects in this list implement IDependantToNetworkCanvas
    List<NetworkObject> m_bufferUI = new List<NetworkObject>();
    void Start()
    {
        NetworkCanvas.onNetworkCanvasSpawned += OnNetworkCanvasSpawned;

        foreach (var networkObject in networkObjects)
        {
            NetworkObject _networkObject = Instantiate(networkObject);
            _networkObject.Spawn();

            if (_networkObject.GetComponent<IDependantToNetworkCanvas>() != null)
            {
                if (NetworkCanvas.Instance != null)
                {
                    _networkObject.transform.SetParent(NetworkCanvas.Instance.transform, false);
                }
                else
                {
                    m_bufferUI.Add(_networkObject);
                    Debug.Log($"{_networkObject.name} added to buffer");
                }
            }
        }
    }

    private void OnNetworkCanvasSpawned(NetworkCanvas canvas)
    {
        foreach (var networkObject in m_bufferUI)
        {
            networkObject.transform.SetParent(canvas.transform, false);
            Debug.Log($"{networkObject.name} added to canvas");
        }
        m_bufferUI.Clear();
    }
}

