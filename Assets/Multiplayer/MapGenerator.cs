using System.Linq;
using UnityEngine;

public class MapGenerator
{
    public byte[] GenerateMap(int _mapSize)
    {
        // 1 = grass, 2 = dirt, 3 = water
        byte[] _map = Enumerable.Repeat((byte)1, _mapSize * _mapSize).ToArray();
        System.Random rand = new System.Random();
        int _riverPosition = 4;

        for (int _y = 0; _y < _mapSize; _y++)
        {
            for (int _x = 0; _x < _mapSize; _x++)
            {
                if (_x == _riverPosition || _x == _riverPosition + 1) // River with noise
                {
                    _map[_y * _mapSize + _x] = 3; // Water
                }
                else if (_x == _riverPosition - 1 || _x == _riverPosition + 2) // Dirt along the river
                {
                    _map[_y * _mapSize + _x] = 2; // Dirt
                }
                else
                {
                    _map[_y * _mapSize + _x] = 1; // Grass
                }
            }
            // Add noise to the river position
            _riverPosition += rand.Next(-1, 2);
            _riverPosition = Mathf.Clamp(_riverPosition, 1, _mapSize - 2); // Keep river within bounds
        }
        return _map;
    }
}