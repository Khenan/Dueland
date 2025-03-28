using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeSlot : MonoBehaviour
{
    [SerializeField] private Image avatar;
    [SerializeField] private Image lifebar;
    [SerializeField] private TextMeshProUGUI lifeText;

    private Character character;

    public void Init(Character _character)
    {
        character = _character;
        character.Data.OnLifeChanged += UpdateLife;
        avatar.color = character.Color;
        UpdateLife();
    }

    void UpdateLife()
    {
        // lifeText.text = character.Data.Life.ToString();
        // lifebar.fillAmount = (float)character.Data.Life / character.Data.MaxLife;
    }
}
