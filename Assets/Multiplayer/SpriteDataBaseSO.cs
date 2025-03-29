using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDataBase", menuName = "ScriptableObjects/SpriteDataBase", order = 1)]
public class SpriteDataBaseSO : ScriptableObject
{
    public List<Sprite> spritesList = new List<Sprite>();

    public Sprite GetSpriteById(int _id)
    {
        if (_id >= 0 && _id < spritesList.Count)
        {
            return spritesList[_id];
        }
        else
        {
            Logger.LogError("Sprite ID out of range: " + _id);
            return null;
        }
    }
}