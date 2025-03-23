using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float GameTime => gameTime.Value;
    NetworkObject networkObject;

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        if (NetworkManager.Singleton.IsServer)
        {
            networkObject.Spawn();
        }
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            gameTime.Value += Time.deltaTime;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Network Object Spawned");
    }
}
