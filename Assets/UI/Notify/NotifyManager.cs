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
        TurnManager.onNextTurn += SendNotifyToPlayer;

    }

    public void ShowNotifyText(string _text, float _time)
    {
        NotifyText _notifyText = Instantiate(notifyTextPrefab, notifyTextParent);
        _notifyText.Initalize(_text, _time);
    }

    public void SendNotifyToPlayer(ulong _previousClientId, ulong _currentClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _currentClientId)
        {
            ShowNotifyText("It's your turn!", 1.5f);
        }
    }
}
