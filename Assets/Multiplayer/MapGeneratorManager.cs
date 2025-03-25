using UnityEngine;

public class MapGenerator
{
    public byte[] GenerateMap()
    {
        // 1 = grass, 2 = dirt, 3 = water
        byte[] _map = new byte[100];
        System.Random rand = new System.Random();
        int _riverPosition = 4;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (j == _riverPosition || j == _riverPosition + 1) // River with noise
                {
                    _map[i * 10 + j] = 3; // Water
                }
                else if (j == _riverPosition - 1 || j == _riverPosition + 2) // Dirt along the river
                {
                    _map[i * 10 + j] = 2; // Dirt
                }
                else
                {
                    _map[i * 10 + j] = 1; // Grass
                }
            }
            // Add noise to the river position
            _riverPosition += rand.Next(-1, 2);
            _riverPosition = Mathf.Clamp(_riverPosition, 1, 8); // Keep river within bounds
        }
        return _map;
    }
}