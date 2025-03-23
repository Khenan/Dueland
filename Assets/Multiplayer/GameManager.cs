using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<float> gameTime = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float GameTime => gameTime.Value;

    public static Action<GameManager> onGameManagerSpawned;

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
        if (NetworkManager.Singleton.IsServer)
        {
            gameTime.Value += Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            PingServer();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Network GameManager Spawned");
        onGameManagerSpawned?.Invoke(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PingServer()
    {
        Debug.Log("A client ping Server");
        PingClient();
    }

    [ClientRpc]
    private void PingClient()
    {
        Debug.Log("Server ping Client");
    }
}