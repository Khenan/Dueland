using UnityEngine;
using TMPro;

public class NotifyText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private float timeToDestroy = 1.5f;

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
    }
    void Start()
    {
        Invoke("DestroyObject", timeToDestroy);
    }
    void DestroyObject() => Destroy(gameObject);
    public void SetText(string _text)
    {
        textComponent.text = _text;
    }
}
