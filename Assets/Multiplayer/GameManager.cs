using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float GameTime => gameTime.Value;

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
