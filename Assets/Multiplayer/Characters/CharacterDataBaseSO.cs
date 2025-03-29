using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataBase", menuName = "ScriptableObjects/CharacterDataBase", order = 1)]
public class CharacterDataBaseSO : ScriptableObject
{
    public List<CharacterData> CharacterDataList = new List<CharacterData>();

    internal CharacterData GetCharacterDataById(int _characterId)
    {
        if (_characterId > 0 || _characterId < CharacterDataList.Count)
        {
            return CharacterDataList[_characterId];
        }
        else
        {
            Debug.LogError("Character ID is out of bounds: " + _characterId);
            return CharacterDataList[0]; // Return the first character data or handle the error as needed
        }
    }
}