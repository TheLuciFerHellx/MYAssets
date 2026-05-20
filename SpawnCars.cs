// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Video;

// public class SpawnCars : MonoBehaviour
// {
//     public List<CarMover> carPrefabs;

//     public Vector3 CenterArea;
//     public Vector3 AreaSize;

//     public SpawnPassengers spawnPassengers;

//     // public int howManyCars = 5;

//     public LayerMask carLayer;

//     public List<Transform> carSpawnPoints;

//     public LevelData currentLevelData; 

//     void Awake()
//     {
//         // SpawnCarAsPerLevelNeed();
//     }

//         // public void SpawnCarAsPerLevelNeed()
//         // {
//         //         int carsToSpawn = currentLevelData.carsToSpawn;
//         //         for (int i = 0; i < carsToSpawn; i++)
//         //         {
//         //                 int index = Random.Range(0, carPrefabs.Count);
//         //                 int carSpawnSlots = Random.Range(0, carSpawnPoints.Count);

//         //                 // Define the allowed angles
//         //                 // float[] possibleAngles = { 0f, 30f, 60f, 90f, 180f, 270f, -30f, -60f };
//         //                 // float randomYRotation = possibleAngles[Random.Range(0, possibleAngles.Length)]; // 0 90 180 270
//         //                 // Quaternion spawnRot = Quaternion.Euler(0, randomYRotation, 0);

//         //                 var carMover = Instantiate(carPrefabs[index], carSpawnPoints[carSpawnSlots].position, Quaternion.identity);
//         //                 spawnPassengers.TotalCarsSpawn.Add(carMover);
                                
//         //                 // carSpawnPoints.RemoveAt(carSpawnSlots);
//         //         }
//         // }

//         // public void SpawnCarAsPerLevelNeed()
//         // {
//         // // Safety check: Make sure we have data and spawn points
//         // if (currentLevelData == null || carSpawnPoints.Count == 0) return;

//         // // Create a temporary list so we don't destroy the original list of points
//         // List<Transform> availablePoints = new List<Transform>(carSpawnPoints);

//         // int carsToSpawn = currentLevelData.carsToSpawn;

//         // for (int i = 0; i < carsToSpawn; i++)
//         // {
//         //         // Prevent crash if you try to spawn more cars than you have points
//         //         if (availablePoints.Count == 0) break; 

//         //         int prefabIndex = Random.Range(0, carPrefabs.Count);
//         //         int pointIndex = Random.Range(0, availablePoints.Count);

//         //         var carMover = Instantiate(carPrefabs[prefabIndex], availablePoints[pointIndex].position, Quaternion.identity);
//         //         spawnPassengers.TotalCarsSpawn.Add(carMover);
                        
//         //         // Remove from the TEMP list so two cars don't spawn on top of each other
//         //         availablePoints.RemoveAt(pointIndex);
//         // }
//         // }

//     public void SpawnCarAsPerLevelNeed()
//     {
//         if (currentLevelData == null || carSpawnPoints.Count == 0) return;

//         List<Transform> availablePoints = new List<Transform>(carSpawnPoints);
//         int carsToSpawn = currentLevelData.carsToSpawn;

//         for (int i = 0; i < carsToSpawn; i++)
//         {
//             if (availablePoints.Count == 0) break; 

//             int prefabIndex = Random.Range(0, carPrefabs.Count);
//             // int pointIndex = Random.Range(0, availablePoints.Count); // for random
//             int pointIndex = 0; // for index based

//             // 1. Get reference to the prefab we want
//             CarMover selectedPrefab = carPrefabs[prefabIndex];
            
//             // 2. Try to get from pool, if null then Instantiate
//             CarMover carMover = ObjectPool.Instance.GetCarFromPool(selectedPrefab); 
            
//             if (carMover != null) 
//             {
//                 Debug.Log("car found in pool");
//                 carMover.transform.position = availablePoints[pointIndex].position;
//                 carMover.transform.rotation = Quaternion.identity;
//                 carMover.ResetCapacity();
//                 carMover.ResetEnum();
//                 carMover.gameObject.SetActive(true);
//                 carMover.arrow.enabled = true;
//                 carMover.totalPassengerTxt.enabled= false;
//             } 
//             else 
//             {
//                 Debug.Log("new car added");
//                 carMover = Instantiate(selectedPrefab, availablePoints[pointIndex].position, Quaternion.identity);
//                 // Optional: Add to pool list if your GetFromPool relies on a specific list
//                 // ObjectPool.Instance.pool.Add(carMover.gameObject); 
//             }

//             // 3. Add to the list so level doesn't end early
//             spawnPassengers.TotalCarsSpawn.Add(carMover);
//             availablePoints.RemoveAt(pointIndex);
//         }
//     }


// }

// using System.Collections.Generic;
// using UnityEngine;

// public class SpawnCars : MonoBehaviour 
// {
//     [Header("Prefabs & References")]
//     [SerializeField] public GameObject carSpawnPrefab; 
//     public List<CarMover> carPrefabs;
//     public SpawnPassengers spawnPassengers;
//     public LevelData currentLevelData;

//     [Header("Grid Layout Settings")]
//     public Vector3 CarSpawnGrid; 
    
//     // Based on your car dimensions (X=3, Z=6), 6.5f forms a safe square boundary box
//     // so cars can rotate 0, 90, or -90 degrees without ever clipping.
//     private const float GridSlotSize = 6.5f; 

//     private Vector3 centerArea = new Vector3(-2.4f, 0, -8.5f); 

//     [Header("Runtime Tracked Positions")]
//     public List<GameObject> spawnedPositions = new List<GameObject>();

//     public void SpawnCarAsPerLevelNeed()
//     {
//         SpawnGridAndCars();
//     }

//     public void SpawnGridAndCars()
//     {
//         if (currentLevelData == null || carSpawnPrefab == null || carPrefabs.Count == 0) return;

//         ClearOldPositions();

//         int carsToSpawn = currentLevelData.carsToSpawn;
//         if (carsToSpawn <= 0) return;

//         // 1. Dynamically calculate grid columns based on the total car count.
//         // For 2 cars, it makes a compact layout. For 40 cars, it creates a balanced block.
//         int columns = Mathf.CeilToInt(Mathf.Sqrt(carsToSpawn));
//         int rows = Mathf.CeilToInt((float)carsToSpawn / columns);

//         int spawnedCount = 0;

//         // 2. Build the grid outward from the middle point
//         for (int row = 0; row < rows; row++)
//         {
//             for (int col = 0; col < columns; col++)
//             {
//                 if (spawnedCount >= carsToSpawn) break;

//                 // This math centers the entire block exactly on centerArea.
//                 // It ensures low car counts (like 2) stay clustered tightly in the middle,
//                 // instead of shooting out to the far corners of your Gizmo box.
//                 float offsetX = (col - (columns - 1) / 2f) * GridSlotSize;
//                 float offsetZ = (row - (rows - 1) / 2f) * GridSlotSize;

//                 Vector3 spawnPosition = centerArea + new Vector3(offsetX, 0, offsetZ);

//                 // 3. Select random direction securely: 0, 90, or -90 degrees
//                 float[] possibleAngles = { 0f, 90f};
//                 float randomYRotation = possibleAngles[Random.Range(0, possibleAngles.Length)];
//                 Quaternion randomRotation = Quaternion.Euler(0, randomYRotation, 0);

//                 // 4. Drop the layout position slot indicator
//                 GameObject slotIndicator = Instantiate(carSpawnPrefab, spawnPosition, randomRotation);
//                 spawnedPositions.Add(slotIndicator); 

//                 // 5. Fetch vehicle from pool or construct asset directly
//                 int randomCarIndex = Random.Range(0, carPrefabs.Count);
//                 CarMover chosenPrefab = carPrefabs[randomCarIndex];
//                 CarMover activeCar = ObjectPool.Instance.GetCarFromPool(chosenPrefab);
                
//                 if (activeCar != null)
//                 {
//                     activeCar.transform.position = spawnPosition;
//                     activeCar.transform.rotation = randomRotation;
//                     activeCar.ResetCapacity();
//                     activeCar.ResetEnum();
//                     activeCar.gameObject.SetActive(true);
//                     activeCar.arrow.enabled = true;
//                     activeCar.totalPassengerTxt.enabled = false;
//                 }
//                 else
//                 {
//                     activeCar = Instantiate(chosenPrefab, spawnPosition, randomRotation);
//                 }

//                 spawnPassengers.TotalCarsSpawn.Add(activeCar);
//                 spawnedCount++;
//             }
//         }
//     }

//     private void ClearOldPositions()
//     {
//         foreach (GameObject pos in spawnedPositions)
//         {
//             if (pos != null) Destroy(pos);
//         }
//         spawnedPositions.Clear();
//     }

//     void OnDrawGizmos()
//     {
//         // Outermost grid size boundaries
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireCube(centerArea, CarSpawnGrid);

//         // Preview the compact, centered grid alignment inside your Scene View
//         if (currentLevelData != null && currentLevelData.carsToSpawn > 0)
//         {
//             Gizmos.color = Color.cyan;
//             int columns = Mathf.CeilToInt(Mathf.Sqrt(currentLevelData.carsToSpawn));
//             int rows = Mathf.CeilToInt((float)currentLevelData.carsToSpawn / columns);

//             for (int r = 0; r < rows; r++)
//             {
//                 for (int c = 0; c < columns; c++)
//                 {
//                     float offsetX = (c - (columns - 1) / 2f) * GridSlotSize;
//                     float offsetZ = (r - (rows - 1) / 2f) * GridSlotSize;
//                     Gizmos.DrawWireCube(centerArea + new Vector3(offsetX, 0, offsetZ), new Vector3(GridSlotSize, 0.2f, GridSlotSize));
//                 }
//             }
//         }
//     }
// }


// using System.Collections.Generic;
// using UnityEngine;

// public class SpawnCars : MonoBehaviour 
// {
//     [Header("Prefabs & References")]
//     [SerializeField] public GameObject carSpawnPrefab; 
//     public List<CarMover> carPrefabs;
//     public SpawnPassengers spawnPassengers;
//     public LevelData currentLevelData;

//     [Header("Grid Layout Settings")]
//     public Vector3 CarSpawnGrid; 
    
//     // Constant outer dimensions for your big map arena grid configuration
//     [SerializeField] private int maxColumns = 5;
//     [SerializeField] private int maxRows = 5;

//     private const float GridSlotSize = 6.5f; 
//     private Vector3 centerArea = new Vector3(-2.4f, 0, -8.5f); 

//     [Header("Runtime Tracked Positions")]
//     public List<GameObject> spawnedPositions = new List<GameObject>();

//     public void SpawnCarAsPerLevelNeed()
//     {
//         SpawnGridAndCars();
//     }

//     public void SpawnGridAndCars()
//     {
//         if (currentLevelData == null || carSpawnPrefab == null || carPrefabs.Count == 0) return;

//         ClearOldPositions();

//         int carsToSpawn = currentLevelData.carsToSpawn;
//         if (carsToSpawn <= 0) return;

//         // 1. Calculate the exact structural center indices of the big predefined grid
//         int centerCol = maxColumns / 2;
//         int centerRow = maxRows / 2;

//         // 2. Map all slots inside the big grid and sort them based on center distance
//         List<Vector2Int> sortedSlots = new List<Vector2Int>();
//         for (int r = 0; r < maxRows; r++)
//         {
//             for (int c = 0; c < maxColumns; c++)
//             {
//                 sortedSlots.Add(new Vector2Int(c, r));
//             }
//         }

//         // Sort: Closest layout grid items to the absolute middle slot bubble up first
//         Vector2Int centerTarget = new Vector2Int(centerCol, centerRow);
//         sortedSlots.Sort((a, b) => 
//             Vector2Int.Distance(a, centerTarget).CompareTo(Vector2Int.Distance(b, centerTarget))
//         );

//         // 3. Process spawning loops strictly using the closest slots up to carsToSpawn count
//         int spawnedCount = 0;
//         for (int i = 0; i < sortedSlots.Count; i++)
//         {
//             if (spawnedCount >= carsToSpawn) break;

//             Vector2Int slot = sortedSlots[i];

//             // Center math ensures slot (centerCol, centerRow) falls perfectly onto centerArea
//             float offsetX = (slot.x - centerCol) * GridSlotSize;
//             float offsetZ = (slot.y - centerRow) * GridSlotSize;
//             Vector3 spawnPosition = centerArea + new Vector3(offsetX, 0, offsetZ);

//             // Select random direction securely: 0 or 90 degrees
//             float[] possibleAngles = { 0f, 90f };
//             float randomYRotation = possibleAngles[Random.Range(0, possibleAngles.Length)];
//             Quaternion randomRotation = Quaternion.Euler(0, randomYRotation, 0);

//             // Drop the layout position slot indicator
//             GameObject slotIndicator = Instantiate(carSpawnPrefab, spawnPosition, randomRotation);
//             spawnedPositions.Add(slotIndicator); 

//             // Fetch vehicle from pool or construct asset directly
//             int randomCarIndex = Random.Range(0, carPrefabs.Count);
//             CarMover chosenPrefab = carPrefabs[randomCarIndex];
//             CarMover activeCar = ObjectPool.Instance.GetCarFromPool(chosenPrefab);
            
//             if (activeCar != null)
//             {
//                 activeCar.transform.position = spawnPosition;
//                 activeCar.transform.rotation = randomRotation;
//                 activeCar.ResetCapacity();
//                 activeCar.ResetEnum();
//                 activeCar.gameObject.SetActive(true);
//                 activeCar.arrow.enabled = true;
//                 activeCar.totalPassengerTxt.enabled = false;
//             }
//             else
//             {
//                 activeCar = Instantiate(chosenPrefab, spawnPosition, randomRotation);
//             }

//             spawnPassengers.TotalCarsSpawn.Add(activeCar);
//             spawnedCount++;
//         }
//     }

//     private void ClearOldPositions()
//     {
//         foreach (GameObject pos in spawnedPositions)
//         {
//             if (pos != null) Destroy(pos);
//         }
//         spawnedPositions.Clear();
//     }

//     void OnDrawGizmos()
//     {
//         // Outermost boundary visualization
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireCube(centerArea, CarSpawnGrid);

//         // Preview the complete big structural map grid matrix inside your Editor
//         Gizmos.color = Color.cyan;
//         int centerCol = maxColumns / 2;
//         int centerRow = maxRows / 2;

//         for (int r = 0; r < maxRows; r++)
//         {
//             for (int c = 0; c < maxColumns; c++)
//             {
//                 float offsetX = (c - centerCol) * GridSlotSize;
//                 float offsetZ = (r - centerRow) * GridSlotSize;
                
//                 // Keep track of the middle center point visually using Red
//                 if (c == centerCol && r == centerRow) Gizmos.color = Color.red;
//                 else Gizmos.color = Color.cyan;

//                 Gizmos.DrawWireCube(centerArea + new Vector3(offsetX, 0, offsetZ), new Vector3(GridSlotSize - 0.2f, 0.2f, GridSlotSize - 0.2f));
//             }
//         }
//     }
// }


using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class SpawnCars : MonoBehaviour
{

    public static SpawnCars Instance { get; private set; }

    public class GridSlot
    {
        public Vector2Int GridIndex;
        public Vector3 WorldPosition;
        public bool IsOccupied;

        public GridSlot(Vector2Int index, Vector3 pos)
        {
            GridIndex = index;
            WorldPosition = pos;
            IsOccupied = false;
        }
    }

    [Header("Prefabs & References")]
    [SerializeField] public GameObject carSpawnPrefab;
    public List<CarMover> carPrefabs;
    public SpawnPassengers spawnPassengers;
    public LevelData currentLevelData;

    [Header("Grid Layout Settings")]
    // public Vector3 CarSpawnGrid;
    public int maxColumns = 5;
    public int maxRows = 5;
    private const float GridSlotSize = 6.5f;
    [SerializeField] private Vector3 centerArea = new Vector3(-2.4f, 0, -8.5f);

    [Header("Runtime Tracked Positions")]
    public List<GameObject> spawnedPositions = new List<GameObject>();
    
    private List<GridSlot> gridSlots = new List<GridSlot>();


    private void Awake()
    {
        // Initialize the singleton instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- GRID ACCESS METHODS FOR EXTERNAL SCRIPTS ---
    public GridSlot GetSlotAt(int x, int y)
    {
        return gridSlots.Find(slot => slot.GridIndex.x == x && slot.GridIndex.y == y);
    }

    public bool IsSlotBlocked(int x, int y)
    {
        GridSlot slot = GetSlotAt(x, y);
        if (slot == null) return false; // Grid boundaries block cars
        return slot.IsOccupied;
    }

    public void SetSlotOccupation(int x, int y, bool occupied)
    {
        GridSlot slot = GetSlotAt(x, y);
        if (slot != null)
        {
            slot.IsOccupied = occupied;
        }
    }

    public void SpawnCarAsPerLevelNeed()
    {
        SpawnGridAndCars();
    }

    public void SpawnGridAndCars()
    {
        if (currentLevelData == null || carSpawnPrefab == null || carPrefabs.Count == 0) return;
        
        ClearOldPositions();
        
        // If there are specific visual car spawns configured in the LevelData, load them!
        if (currentLevelData.carSpawns != null && currentLevelData.carSpawns.Count > 0)
        {
            maxColumns = currentLevelData.gridColumns;
            maxRows = currentLevelData.gridRows;

            InitializeGridData();

            foreach (var spawn in currentLevelData.carSpawns)
            {
                GridSlot slot = GetSlotAt(spawn.gridPosition.x, spawn.gridPosition.y);
                if (slot == null) continue;

                Quaternion carRotation = Quaternion.Euler(0, spawn.rotationY, 0);

                // Spawn the rotation indicator (arrow/direction slot indicator)
                GameObject slotIndicator = Instantiate(carSpawnPrefab, slot.WorldPosition, carRotation);
                spawnedPositions.Add(slotIndicator);

                // As per user request: spawn cars randomly from the available prefabs list at runtime
                int randomCarIndex = Random.Range(0, carPrefabs.Count);
                CarMover chosenPrefab = carPrefabs[randomCarIndex];

                CarMover activeCar = ObjectPool.Instance.GetCarFromPool(chosenPrefab);
                
                if (activeCar != null)
                {
                    activeCar.transform.position = slot.WorldPosition;
                    activeCar.transform.rotation = carRotation;
                    activeCar.ResetCapacity();
                    activeCar.ResetEnum();
                    activeCar.gameObject.SetActive(true);
                    activeCar.arrow.enabled = true;
                    activeCar.totalPassengerTxt.enabled = false;
                }
                else
                {
                    activeCar = Instantiate(chosenPrefab, slot.WorldPosition, carRotation);
                }

                // ASSIGN INITIAL GRID INDEX TO THE CAR
                Carout caroutComp = activeCar.GetComponent<Carout>();
                if (caroutComp != null)
                {
                    caroutComp.currentGridIndex = slot.GridIndex;
                }

                spawnPassengers.TotalCarsSpawn.Add(activeCar);
                slot.IsOccupied = true;
            }
        }
        else
        {
            // FALLBACK / LEGACY SYSTEM: Spawn randomly (Levels 1-8 and infinite random generation)
            maxColumns = 5;
            maxRows = 5;
            InitializeGridData();

            int carsToSpawn = currentLevelData.carsToSpawn;
            if (carsToSpawn <= 0) return;

            Vector2Int centerTarget = new Vector2Int(maxColumns / 2, maxRows / 2);
            var sortedSlots = gridSlots
                .OrderBy(slot => Vector2Int.Distance(slot.GridIndex, centerTarget))
                .ToList();

            int spawnedCount = 0;
            for (int i = 0; i < sortedSlots.Count; i++)
            {
                if (spawnedCount >= carsToSpawn) break;

                GridSlot slot = sortedSlots[i];

                if (slot.IsOccupied) continue; 

                float[] possibleAngles = { 0f, 90f }; 
                float randomYRotation = possibleAngles[Random.Range(0, possibleAngles.Length)];
                Quaternion randomRotation = Quaternion.Euler(0, randomYRotation, 0);

                GameObject slotIndicator = Instantiate(carSpawnPrefab, slot.WorldPosition, randomRotation);
                spawnedPositions.Add(slotIndicator);

                int randomCarIndex = Random.Range(0, carPrefabs.Count);
                CarMover chosenPrefab = carPrefabs[randomCarIndex];
                
                CarMover activeCar = ObjectPool.Instance.GetCarFromPool(chosenPrefab);
                
                if (activeCar != null)
                {
                    activeCar.transform.position = slot.WorldPosition;
                    activeCar.transform.rotation = randomRotation;
                    activeCar.ResetCapacity();
                    activeCar.ResetEnum();
                    activeCar.gameObject.SetActive(true);
                    activeCar.arrow.enabled = true;
                    activeCar.totalPassengerTxt.enabled = false;
                }
                else
                {
                    activeCar = Instantiate(chosenPrefab, slot.WorldPosition, randomRotation);
                }

                Carout caroutComp = activeCar.GetComponent<Carout>();
                if (caroutComp != null)
                {
                    caroutComp.currentGridIndex = slot.GridIndex;
                }

                spawnPassengers.TotalCarsSpawn.Add(activeCar);
                slot.IsOccupied = true; 
                spawnedCount++;
            }
        }
    }

    public bool CheckBlockageForCar(Vector2Int currentGridIndex, Vector3 eulerAngles, out Vector2Int targetGridIndex)
    {
        Vector2Int forwardDirection = Vector2Int.zero;
        
        // Round to nearest 90 degrees to avoid rotation precision floating-point bugs
        float currentYRotation = Mathf.Repeat(Mathf.Round(eulerAngles.y / 90f) * 90f, 360f);

        if (Mathf.Approximately(currentYRotation, 0f))       forwardDirection = new Vector2Int(0, 1);   // North
        else if (Mathf.Approximately(currentYRotation, 90f))  forwardDirection = new Vector2Int(1, 0);   // East
        else if (Mathf.Approximately(currentYRotation, 180f)) forwardDirection = new Vector2Int(0, -1);  // South
        else if (Mathf.Approximately(currentYRotation, 270f)) forwardDirection = new Vector2Int(-1, 0);  // West

        targetGridIndex = currentGridIndex + forwardDirection;

        return IsSlotBlocked(targetGridIndex.x, targetGridIndex.y);
    }

    private void InitializeGridData()
    {
        gridSlots.Clear();
        int centerCol = maxColumns / 2;
        int centerRow = maxRows / 2;

        for (int r = 0; r < maxRows; r++)
        {
            for (int c = 0; c < maxColumns; c++)
            {
                float offsetX = (c - centerCol) * GridSlotSize;
                float offsetZ = (r - centerRow) * GridSlotSize;
                Vector3 spawnPosition = centerArea + new Vector3(offsetX, 0, offsetZ);
                
                gridSlots.Add(new GridSlot(new Vector2Int(c, r), spawnPosition));
            }
        }
    }

    private void ClearOldPositions()
    {
        foreach (GameObject pos in spawnedPositions)
        {
            if (pos != null) Destroy(pos);
        }
        spawnedPositions.Clear();
        gridSlots.Clear(); 
    }

    void OnDrawGizmos()
    {
        int cols = (currentLevelData != null) ? currentLevelData.gridColumns : maxColumns;
        int rows = (currentLevelData != null) ? currentLevelData.gridRows : maxRows;
        
        int centerCol = cols / 2;
        int centerRow = rows / 2;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float offsetX = (c - centerCol) * GridSlotSize;
                float offsetZ = (r - centerRow) * GridSlotSize;
                if (c == centerCol && r == centerRow) Gizmos.color = Color.red;
                else Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(centerArea + new Vector3(offsetX, 0, offsetZ), new Vector3(GridSlotSize - 0.2f, 0.2f, GridSlotSize - 0.2f));
            }
        }
    }
}
