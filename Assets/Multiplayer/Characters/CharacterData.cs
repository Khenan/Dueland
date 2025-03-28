using System;
using UnityEngine;

public class CharacterData
{
    public int Life { get; private set; }
    public int MaxLife { get; private set; }
    public Action OnLifeChanged;

    public CharacterData(int _maxLife)
    {
        Life = _maxLife;
        MaxLife = _maxLife;
    }

    public void TakeDamage(int _damage)
    {
        Life -= _damage;
        if(Life < 0)
        {
            Life = 0;
            Dead();
        }
        OnLifeChanged?.Invoke();
    }

    private void Dead()
    {
        Debug.Log("Character is dead!");
    }
}