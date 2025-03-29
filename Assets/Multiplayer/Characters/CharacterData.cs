using System;
using Unity.Netcode;
using UnityEngine;

public class CharacterData
{
    public int Life { get; private set; }
    public int LifeMax { get; private set; }
    public Action OnLifeChanged;

    public int MovePoints { get; private set; } = 3;
    public int MovePointsMax { get; private set; } = 3;

    public CharacterData(int _lifeMax, int _movePointsMax)
    {
        Life = _lifeMax;
        LifeMax = _lifeMax;

        MovePoints = _movePointsMax;
        MovePointsMax = _movePointsMax;

        TurnManager.onNextTurn += ResetMovePoints;
    }

    private void ResetMovePoints(ulong _clientId)
    {
        if (_clientId == NetworkManager.Singleton.LocalClientId)
        {
            MovePoints = MovePointsMax;
        }
    }

    public void TakeDamage(int _damage)
    {
        Life -= _damage;
        if (Life < 0)
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

    public bool Move(int _movePoints)
    {
        if (MovePoints >= _movePoints)
        {
            MovePoints -= _movePoints;
            return true;
        }
        return false;
    }
}