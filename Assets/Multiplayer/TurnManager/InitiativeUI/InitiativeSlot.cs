using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeSlot : MonoBehaviour
{
    [SerializeField] private Image avatar;
    [SerializeField] private Transform outlineImage;

    private Character character;
    public Character Character => character;

    public void Init(Character _character)
    {
        character = _character;
        avatar.color = character.Color;
        SetOutline(false);
    }

    public void SetOutline(bool _isActive)
    {
        outlineImage.gameObject.SetActive(_isActive);
    }
}
