using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Logger : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private List<Transform> objectsWhenActive;
    [SerializeField] private List<Transform> objectsWhenInactive;

    [SerializeField] private TMPro.TextMeshProUGUI logText;

    public static Logger Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        SetActive(false);
    }

    public static void Log(string _message)
    {
        Debug.Log("<b>[Logger]</b> " + _message);
        if (Instance != null) Instance.logText.text += _message + "\n";
    }

    public static void LogWarning(string _message)
    {
        Debug.LogError("<b>[Logger]</b> " + _message);
        if (Instance != null) Instance.logText.text += "<color=yellow>[Warning]</color> " + _message + "\n";
    }

    public static void LogError(string _message)
    {
        Debug.LogError("<b>[Logger]</b> " + _message);
        if (Instance != null) Instance.logText.text += "<color=red>[Error]</color> " + _message + "\n";
    }

    public void SetActive(bool active)
    {
        root.gameObject.SetActive(active);
        foreach (var obj in objectsWhenActive)
        {
            obj.gameObject.SetActive(active);
        }
        foreach (var obj in objectsWhenInactive)
        {
            obj.gameObject.SetActive(!active);
        }
    }
}
