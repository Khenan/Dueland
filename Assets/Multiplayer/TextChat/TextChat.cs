using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextChat : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI chatText;
    [SerializeField] private ScrollRect scrollRect;

     [SerializeField] private Color32 timeColor;
     [SerializeField] private Color32 playerColor;

    public static TextChat Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string _text)
    {
        Debug.Log(MultiplayerManager.Instance.PlayerName);
        GameManager.Instance.AddTextChatServerRpc(_text, MultiplayerManager.Instance.PlayerName);
    }

    internal void AddText(string _text, string _playerName)
    {
        if (string.IsNullOrEmpty(_text))
        {
            return;
        }

        DateTime _now = DateTime.Now;
        string _time = _now.ToString("HH:mm:ss");
        chatText.text += $"<color=#{ColorUtility.ToHtmlStringRGBA(timeColor)}>[{_time}]</color> <color=#{ColorUtility.ToHtmlStringRGBA(playerColor)}>{_playerName}</color>: {_text} {Environment.NewLine}";
        inputField.text = string.Empty;

        Canvas.ForceUpdateCanvases();
    }
}
