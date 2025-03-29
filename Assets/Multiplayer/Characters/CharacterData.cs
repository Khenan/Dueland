using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class CharacterData
{
    public int Life;
    public int LifeMax;
    public Action OnLifeChanged;

    public int MovePoints = 3;
    public int MovePointsMax = 3;

    public CharacterData(int _lifeMax, int _movePointsMax)
    {
        Life = _lifeMax;
        LifeMax = _lifeMax;

        MovePoints = _movePointsMax;
        MovePointsMax = _movePointsMax;

        TurnManager.onNextTurn += ResetMovePoints;
    }

    private void ResetMovePoints(ulong _previousClientId, ulong _currentClientId)
    {
        if (_previousClientId == NetworkManager.Singleton.LocalClientId)
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