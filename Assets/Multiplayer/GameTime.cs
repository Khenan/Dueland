using TMPro;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    GameManager gameManager;
    TextMeshProUGUI text;

    void Start()
    {
        gameManager = GameManager.Instance;
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        text.text = gameManager.GameTime.ToString();
    }
}