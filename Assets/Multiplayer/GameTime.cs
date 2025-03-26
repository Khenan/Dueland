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
            GameManager.onGameManagerSpawned += (GameManager _gameManager) =>
            {
                Logger.Log("GameTime Getting GameManager from event");
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