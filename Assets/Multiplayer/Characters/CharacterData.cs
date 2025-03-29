using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct CharacterData : INetworkSerializable
{
    public string Name;
    public string Description;
    public int Life;
    public int LifeMax;
    public int MovePoints;
    public int MovePointsMax;
    public Color Color;
    public int SpriteId;

    public void TakeDamage(int damage)
    {
        Life -= damage;
        if (Life < 0)
        {
            Life = 0;
        }
    }

    public void ResetMovePoints()
    {
        MovePoints = MovePointsMax;
    }

    // Obligatoire pour `INetworkSerializable`
    public void NetworkSerialize<T>(BufferSerializer<T> _serializer) where T : IReaderWriter
    {
        _serializer.SerializeValue(ref Name);
        _serializer.SerializeValue(ref Description);
        _serializer.SerializeValue(ref Life);
        _serializer.SerializeValue(ref LifeMax);
        _serializer.SerializeValue(ref MovePoints);
        _serializer.SerializeValue(ref MovePointsMax);
        _serializer.SerializeValue(ref Color);
        _serializer.SerializeValue(ref SpriteId);
    }
}
