using Unity.Netcode;
using UnityEngine;

public class PlayerControllerSpawner : MonoBehaviour
{
    public NetworkObject playerControllerPrefab;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (GameManager.Instance != null && GameManager.Instance.allPlayersConnected.Value)
            {
                SpawnPlayers();
            }
            else
            {
                GameManager.onGameManagerSpawned += (gameManager) =>
                {
                    gameManager.OnAllPlayersConnected += () =>
                    {
                        SpawnPlayers();
                    };
                };
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnPlayers()
    {
        foreach (var _playerId in GameManager.Instance.playerIds)
        {
            SpawnPlayer(_playerId);
        }
    }

    private void SpawnPlayer(ulong _playerId)
    {
        NetworkObject _playerController = Instantiate(playerControllerPrefab);
        _playerController.SpawnWithOwnership(_playerId);
    }
}