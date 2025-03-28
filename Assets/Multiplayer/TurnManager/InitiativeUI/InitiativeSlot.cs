using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeSlot : MonoBehaviour
{
    [SerializeField] private Image avatar;
    [SerializeField] private Image lifebar;
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private Transform outlineImage;

    private Character character;
    public Character Character => character;

    public void Init(Character _character)
    {
        character = _character;
        character.Data.OnLifeChanged += UpdateLife;
        avatar.color = character.Color;
        UpdateLife();
        SetOutline(false);
    }

    public void SetOutline(bool _isActive)
    {
        outlineImage.gameObject.SetActive(_isActive);
    }

    void UpdateLife()
    {
        // lifeText.text = character.Data.Life.ToString();
        // lifebar.fillAmount = (float)character.Data.Life / character.Data.MaxLife;
    }
}
