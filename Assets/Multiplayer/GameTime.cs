using TMPro;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    GameManager gameManager;
    TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (GameManager.Instance != null)
        {
            gameManager = GameManager.Instance;
        }
        else
        {
            MultiplayerManager.Instance.onGameManagerSpawned += (GameManager _gameManager) =>
            {
                Debug.Log("Game Manager Spawned");
                gameManager = _gameManager;
            };
        }
    }

    void Update()
    {
        if (gameManager != null)
        {
            text.text = gameManager.GameTime.ToString("F2") + "s";
        }
        else
        {
            text.text = "0.00s";
        }
    }
}