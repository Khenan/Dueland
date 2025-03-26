using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector2Int matrixPosition;

    public Vector2Int MatrixPosition => matrixPosition;

    public void Init(Vector2Int _matrixPosition)
    {
        matrixPosition = _matrixPosition;
    }
}
