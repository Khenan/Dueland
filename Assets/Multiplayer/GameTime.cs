using TMPro;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    GameManager gameManager;
    TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        MultiplayerManager.Instance.onGameManagerSpawned += (GameManager gameManager) =>
        {
            this.gameManager = gameManager;
        };
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