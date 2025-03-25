using Unity.Netcode;
using UnityEngine;


public class NotifyManager : MonoBehaviour
{
    [SerializeField] private NotifyText notifyTextPrefab;
    [SerializeField] private Transform notifyTextParent;
    public static NotifyManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        TurnManager.onEndTurn += SendNotifyToPlayer;

    }

    public void ShowNotifyText(string _text)
    {
        NotifyText _notifyText = Instantiate(notifyTextPrefab, notifyTextParent);
        _notifyText.SetText(_text);
    }

    public void SendNotifyToPlayer(ulong _playerId)
    {
        if (NetworkManager.Singleton.LocalClientId == _playerId)
        {
            ShowNotifyText("It's your turn!");
        }
    }
}
