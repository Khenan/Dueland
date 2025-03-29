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
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Description);
        serializer.SerializeValue(ref Life);
        serializer.SerializeValue(ref LifeMax);
        serializer.SerializeValue(ref MovePoints);
        serializer.SerializeValue(ref MovePointsMax);
        serializer.SerializeValue(ref Color);
        serializer.SerializeValue(ref SpriteId);
    }
}
