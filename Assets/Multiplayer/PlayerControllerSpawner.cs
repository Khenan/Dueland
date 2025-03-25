using Unity.Netcode;
using UnityEngine;

public class PlayerControllerSpawner : MonoBehaviour
{
    public NetworkObject playerControllerPrefab;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (GameManager.Instance != null && GameManager.Instance.allPlayersConnected && GameManager.Instance.playerIds.Count == 2)
            {
                foreach (var playerId in GameManager.Instance.playerIds)
                {
                    SpawnPlayer(playerId);
                }
            }
            else
            {
                GameManager.onGameManagerSpawned += (gameManager) =>
                {
                    gameManager.OnAllPlayersConnected += () =>
                    {
                        foreach (var playerId in gameManager.playerIds)
                        {
                            SpawnPlayer(playerId);
                        }
                    };
                };
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnPlayer(ulong _clientId)
    {
        NetworkObject _playerController = Instantiate(playerControllerPrefab);
        _playerController.SpawnWithOwnership(_clientId);
    }
}