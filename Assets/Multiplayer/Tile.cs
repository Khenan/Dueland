using System;
using UnityEngine;

public struct Size3D
{
    public int length;
    public int width;
    public int height;
    public Size3D(int _length = 1, int _width = 1, int _height = 1)
    {
        length = _length;
        width = _width;
        height = _height;
    }
}
public struct TileData
{
    public int id;
    public Vector3 position;
    public Size3D size;
    public bool isWalkable;
    public bool isOccupied;
    public bool isSelected;
    public TileData(int _id, Vector3 _position, Size3D _size, bool _isWalkable = true, bool _isOccupied = false, bool _isSelected = false)
    {
        id = _id;
        position = _position;
        size = _size;
        isWalkable = _isWalkable;
        isOccupied = _isOccupied;
        isSelected = _isSelected;
    }
}
public class Tile : MonoBehaviour
{
    private TileData tileData;

    public Action<Tile> OnTileSelected;
    public Action<Tile> OnTileDeselected;
    public Action<Tile> OnTileOccupied;
    public Action<Tile> OnTileUnoccupied;
    public Action<Tile> OnTileWalkable;
    public Action<Tile> OnTileUnwalkable;

    public TileData TileData => tileData;

    public void SetTileData(TileData _tileData)
    {
        tileData = _tileData;
    }

    public void SetTileOccupied(bool _isOccupied)
    {
        bool previousOccupied = tileData.isOccupied;
        tileData.isOccupied = _isOccupied;
        if (previousOccupied != tileData.isOccupied)
        {
            if (tileData.isOccupied)
            {
                OnTileOccupied?.Invoke(this);
            }
            else
            {
                OnTileUnoccupied?.Invoke(this);
            }
        }
    }

    public void SetTileSelected(bool _isSelected)
    {
        bool previousSelected = tileData.isSelected;
        tileData.isSelected = _isSelected;
        if (previousSelected != tileData.isSelected)
        {
            if (tileData.isSelected)
            {
                OnTileSelected?.Invoke(this);
            }
            else
            {
                OnTileDeselected?.Invoke(this);
            }
        }
    }

    public void SetTileWalkable(bool _isWalkable)
    {
        bool previousWalkable = tileData.isWalkable;
        tileData.isWalkable = _isWalkable;
        if (previousWalkable != tileData.isWalkable)
        {
            if (tileData.isWalkable)
            {
                OnTileWalkable?.Invoke(this);
            }
            else
            {
                OnTileUnwalkable?.Invoke(this);
            }
        }
    }
}
