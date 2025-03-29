using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector2Int matrixPosition;

    public Vector2Int MatrixPosition => matrixPosition;

    public void Init(Vector2Int _matrixPosition)
    {
        matrixPosition = _matrixPosition;
    }

    public bool IsAdjacentTo(Tile tile)
    {
        if (tile == null)
            return false;
        bool _tileRowIsAdjacent = Math.Abs(tile.MatrixPosition.x - matrixPosition.x) == 1 && tile.MatrixPosition.y == matrixPosition.y;
        bool _tileColumnIsAdjacent = Math.Abs(tile.MatrixPosition.y - matrixPosition.y) == 1 && tile.MatrixPosition.x == matrixPosition.x;
        return _tileRowIsAdjacent || _tileColumnIsAdjacent;
    }
}
