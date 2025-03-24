using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    public static LoadingPanel Instance { get; private set; }
    [SerializeField] private Transform root;
    [SerializeField] private TextMeshProUGUI loadingText;

    private int objectsToLoadCount;
    private int objectsLoadedCount;

    void Awake()
    {
        Instance = this;
        if (NetworkManager.Singleton.IsServer)
        {
            loadingText.text = "Initialization...";
        }
        else
        {
            loadingText.text = "Connecting to host...";
        }
    }

    public void SetLoadingText(string _text)
    {
        loadingText.text = _text;
    }

    internal void AddLoadingObjects(int count)
    {
        objectsToLoadCount += count;
    }

    public void ObjectLoaded()
    {
        objectsLoadedCount++;
        if (objectsLoadedCount == objectsToLoadCount)
        {
            root.gameObject.SetActive(false);
        }
    }
}
