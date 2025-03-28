using UnityEngine;
using TMPro;

public class NotifyText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void Initalize(string _text, float _time)
    {
        SetText(_text);
        Invoke(nameof(DestroyObject), _time);
    }
    public void SetText(string _text)
    {
        textComponent.text = _text;
    }
    void DestroyObject() => Destroy(gameObject);
}
