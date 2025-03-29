using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector2Int matrixPosition;

    public Vector2Int MatrixPosition => matrixPosition;

    public SpriteRenderer tileSpriteRenderer;
    public Transform hoverTransform;

    // Neighbours
    public Tile Up { get; private set; }
    public Tile Down { get; private set; }
    public Tile Left { get; private set; }
    public Tile Right { get; private set; }

    public bool IsWalkable = true;
    public bool IsOccupied { get; private set; } = false;

    public Color Color = Color.white; // Color of the tile

    public int Cost = 1; // Cost to move through this tile

    // A* Pathfinding
    public Tile Parent { get; set; }
    public int GCost = 0;
    public int HCost = 0;
    public int PathCost = 0;

    void Awake()
    {
        hoverTransform.gameObject.SetActive(false);
    }

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

    public void SetNeighbours(Tile _up, Tile _down, Tile _left, Tile _right)
    {
        Up = _up ?? null;
        Down = _down ?? null;
        Left = _left ?? null;
        Right = _right ?? null;
    }

    internal void Reset()
    {
        // Reset the tile's A* properties
        GCost = Cost;
        HCost = 0;
        PathCost = Cost;
        Parent = null;
        tileSpriteRenderer.color = Color;
    }

    public void Test()
    {
        Cost = 100;
        Color = Color.gray;
        tileSpriteRenderer.color = Color;
    }

    internal void OnMouseExit()
    {
        hoverTransform.gameObject.SetActive(false);
    }

    internal void OnMouseEnter()
    {
        hoverTransform.gameObject.SetActive(true);
    }
}
