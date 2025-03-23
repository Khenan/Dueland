using UnityEngine;
using TMPro;

public class AddInt : MonoBehaviour
{
    private TMP_Text m_text;
    private int value = 0;
    public int Value { get { return value; } }

    void Awake()
    {
        m_text = GetComponentInChildren<TMP_Text>();
    }
    public void AddValue()
    {
        value++;
        UpdateText();
    }
    private void UpdateText()
    {
        m_text.text = value.ToString();
    }
}
