using Unity.Netcode;
using System;
public class NetworkCanvas : NetworkBehaviour
{
    private static NetworkCanvas instance;
    public static Action<NetworkCanvas> onNetworkCanvasSpawned;
    public static NetworkCanvas Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<NetworkCanvas>();
            }
            return instance;
        }
    }
    public override void OnNetworkSpawn()
    {
        onNetworkCanvasSpawned?.Invoke(this);
    }
}
