using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CarSpawnInfo
{
    public Vector2Int gridPosition; // Coordinates (col, row) on the grid
    public float rotationY;         // Facing direction: 0 = Up, 90 = Right, 180 = Down, 270 = Left
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    
    [Header("Custom Grid Layout")]
    public int gridColumns = 5;
    public int gridRows = 5;
    
    [Header("Spawn Locations")]
    public List<CarSpawnInfo> carSpawns = new List<CarSpawnInfo>();

    [Header("Fallback (Legacy Level Data)")]
    public int carsToSpawn; 
}
