using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct TileData : INetworkSerializable, IEquatable<TileData>
{
    public Vector2Int MatrixPosition;
    public ulong NetworkObjectId;
    public bool IsWalkable;

    public bool Equals(TileData _other)
    {
        return MatrixPosition.Equals(_other.MatrixPosition) && NetworkObjectId == _other.NetworkObjectId && IsWalkable == _other.IsWalkable;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> _serializer) where T : IReaderWriter
    {
        _serializer.SerializeValue(ref MatrixPosition);
        _serializer.SerializeValue(ref NetworkObjectId);
        _serializer.SerializeValue(ref IsWalkable);
    }
}