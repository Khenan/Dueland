using UnityEngine;

public class SizeCopy : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    RectTransform rectTransform;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Update()
    {
        rectTransform.sizeDelta = target.sizeDelta;
    }
}
