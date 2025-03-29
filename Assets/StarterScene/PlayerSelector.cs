using TMPro;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown playerDropdown;
    private CharacterDataBaseSO characterDataBase;
    private SpriteDataBaseSO spriteDataBase;

    void Start()
    {
        characterDataBase = MultiplayerManager.Instance.CharacterDataBase;
        spriteDataBase = MultiplayerManager.Instance.SpriteDataBase;

        InitDropdown();
    }

    private void InitDropdown()
    {
        playerDropdown.ClearOptions();
        playerDropdown.AddOptions(characterDataBase.CharacterDataList.ConvertAll(_characterData => _characterData.Name));
        // Set values on all options
        for (int i = 0; i < playerDropdown.options.Count; i++)
        {
            playerDropdown.options[i].text = characterDataBase.CharacterDataList[i].Name;
            playerDropdown.options[i].image = spriteDataBase.GetSpriteById(characterDataBase.CharacterDataList[i].SpriteId);
            playerDropdown.options[i].color = characterDataBase.CharacterDataList[i].Color;
        }
        playerDropdown.value = 0;
        playerDropdown.onValueChanged.AddListener(OnPlayerSelected);
    }

    private void OnPlayerSelected(int _selected)
    {
        MultiplayerManager.Instance.SelectedCharacterId = _selected;
        Debug.Log("Selected character: " + MultiplayerManager.Instance.CharacterDataBase.GetCharacterDataById(_selected).Name);
    }
}
