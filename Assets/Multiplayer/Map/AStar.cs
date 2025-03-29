using System;
using System.Collections.Generic;

public class AStar
{
    public List<Tile> FindPath(Tile _start, Tile _end)
    {
        List<Tile> _openList = new List<Tile>();
        HashSet<Tile> _closedList = new HashSet<Tile>();
        _openList.Add(_start);

        // Reset all tiles
        foreach (var _tile in MapManager.Instance.Tiles)
        {
            _tile.Reset();
        }

        List<List<Tile>> _allPaths = new List<List<Tile>>(); // List to store all the possible paths

        while (_openList.Count > 0)
        {
            Tile _currentTile = GetLowestFCostTile(_openList);
            _openList.Remove(_currentTile);
            _closedList.Add(_currentTile);

            if (_currentTile == _end)
            {
                List<Tile> _path = RetracePath(_start, _end);
                _allPaths.Add(_path); // Store the path

                // Continue searching for other paths
                continue;
            }

            foreach (var _neighbour in GetNeighbours(_currentTile))
            {
                if (_closedList.Contains(_neighbour) || !IsWalkable(_neighbour))
                {
                    continue;
                }

                int _newCostToNeighbour = GetGCost(_currentTile) + GetDistance(_currentTile, _neighbour);
                if (_newCostToNeighbour < GetGCost(_neighbour) || !_openList.Contains(_neighbour))
                {
                    SetGCost(_neighbour, _newCostToNeighbour);
                    SetHCost(_neighbour, GetDistance(_neighbour, _end));
                    SetParent(_neighbour, _currentTile);

                    if (!_openList.Contains(_neighbour))
                    {
                        _openList.Add(_neighbour);
                    }
                }
            }
        }

        // Once all paths are collected, find the shortest one
        List<Tile> shortestPath = null;
        int shortestPathCost = int.MaxValue;

        foreach (var path in _allPaths)
        {
            int totalCost = GetFCost(path[path.Count - 1]); // Get cost of last tile in the path (the destination)
            if (totalCost < shortestPathCost)
            {
                shortestPathCost = totalCost;
                shortestPath = path;
            }
        }

        return shortestPath;
    }

    private void SetParent(Tile _neighbour, Tile _tile)
    {
        _neighbour.Parent = _tile;
    }

    private int GetDistance(Tile _tile, Tile _neighbour)
    {
        int _xDistance = Math.Abs(_tile.MatrixPosition.x - _neighbour.MatrixPosition.x);
        int _yDistance = Math.Abs(_tile.MatrixPosition.y - _neighbour.MatrixPosition.y);
        return _xDistance + _yDistance; // Manhattan distance
    }

    private bool IsWalkable(Tile _tile)
    {
        return _tile.IsWalkable && !_tile.IsOccupied;
    }

    private List<Tile> GetNeighbours(Tile _tile)
    {
        List<Tile> _neighbours = new List<Tile>();
        if (_tile.Up != null) _neighbours.Add(_tile.Up);
        if (_tile.Down != null) _neighbours.Add(_tile.Down);
        if (_tile.Left != null) _neighbours.Add(_tile.Left);
        if (_tile.Right != null) _neighbours.Add(_tile.Right);
        return _neighbours;
    }

    private List<Tile> RetracePath(Tile _start, Tile _end)
    {
        List<Tile> _path = new List<Tile>();
        Tile _currentTile = _end;
        while (_currentTile != _start)
        {
            _path.Add(_currentTile);
            _currentTile = GetParent(_currentTile);
        }
        _path.Reverse();
        return _path;
    }

    private Tile GetParent(Tile _tile)
    {
        return _tile.Parent;
    }

    private Tile GetLowestFCostTile(List<Tile> _openList)
    {
        Tile _lowestFCostTile = _openList[0];
        foreach (var _tile in _openList)
        {
            if (GetFCost(_tile) < GetFCost(_lowestFCostTile))
            {
                _lowestFCostTile = _tile;
            }
        }
        return _lowestFCostTile;
    }

    private int GetFCost(Tile _tile)
    {
        return GetGCost(_tile) + GetHCost(_tile);
    }

    private int GetGCost(Tile _tile)
    {
        return _tile.GCost;
    }

    private int GetHCost(Tile _tile)
    {
        return _tile.HCost;
    }

    private void SetGCost(Tile _tile, int _cost)
    {
        _tile.GCost = _cost + _tile.Cost; // Add tile cost to G cost
    }

    private void SetHCost(Tile _tile, int _cost)
    {
        _tile.HCost = _cost + _tile.GCost; // Add tile GCost to H cost
    }
}
