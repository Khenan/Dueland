using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector2 matrixPosition;

    public Vector2 MatrixPosition => matrixPosition;

    public void Init(Vector2 _matrixPosition)
    {
        matrixPosition = _matrixPosition;
    }
}
