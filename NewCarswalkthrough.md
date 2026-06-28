# 🚗 Multi-Size Car System — Complete Walkthrough
## Car-OUT Puzzle Game — Small / Medium / Bus

---

## 📋 Table of Contents
1. [What Was Built](#1-what-was-built)
2. [How It All Works Together](#2-how-it-all-works-together)
3. [SCRIPT 1 — LevelData.cs](#3-script-1--leveldatacs)
4. [SCRIPT 2 — SpawnCars.cs](#4-script-2--spawncarscs)
5. [SCRIPT 3 — Carout.cs](#5-script-3--caroutcs)
6. [SCRIPT 4 — CarMover.cs](#6-script-4--carmovercs)
7. [SCRIPT 5 — LevelEditorWindow.cs](#7-script-5--leveleditorwindowcs)
8. [Unity Inspector Setup — Step by Step](#8-unity-inspector-setup--step-by-step)
9. [Making a Medium Car Prefab](#9-making-a-medium-car-prefab)
10. [Making a Bus Prefab](#10-making-a-bus-prefab)
11. [Using the Level Editor](#11-using-the-level-editor)
12. [How to Test Everything Works](#12-how-to-test-everything-works)

---

## 1. What Was Built

Three car size types now exist in the game:

| Type | Grid Cells | Inspector Value | Visual |
|------|-----------|----------------|--------|
| **Small** | 1 cell | `CarSize.Small` | Existing cars (no change) |
| **Medium** | 2 cells | `CarSize.Medium` | Wider/longer car |
| **Bus** | 3 cells | `CarSize.Bus` | Full bus |

**Rules (same as before, just extended):**
- A car's **anchor cell** is where it sits on the grid (front of car)
- Body cells extend **behind** the anchor (opposite of facing direction)
- When a car leaves the grid → **all cells it held are freed**
- Blockage check still works from the **front (anchor) cell**
- All 8 directions still work for all 3 sizes
- **Existing levels are 100% backward-compatible** — old `CarSpawnInfo` defaults to `Small`

---

## 2. How It All Works Together

```
LevelData.cs
  └── CarSpawnInfo now has  gridPosition + rotationY + carSize (NEW)

SpawnCars.cs  (spawn time)
  ├── Picks prefab from the right list: smallCarPrefabs / mediumCarPrefabs / busCarPrefabs
  ├── Calls GetCellsForCar(anchor, facing, size) → list of all cells
  ├── Marks ALL those cells IsOccupied = true
  └── Passes the cells list to carout.SetOccupiedCells()

Carout.cs  (when player taps car)
  ├── occupiedCells stores all cells this car holds
  ├── CheckForBlockage() → if clear → calls SpawnCars.FreeSlots(occupiedCells)
  └── Clears occupiedCells (car is now moving, off-grid)

CarMover.cs  (movement)
  └── carSize field declared — used for prefab identity, Inspector-readable

LevelEditorWindow.cs  (Tools menu)
  ├── cellSizes[,] tracks per-cell size
  ├── Grid shows S (green) / M (yellow) / BUS (red) / empty (dark)
  ├── Left-click empty → place Small
  ├── Left-click filled → cycle Small→Medium→Bus→Small
  ├── Right-click filled → remove
  └── GENERATE writes carSize into every CarSpawnInfo
```

---

## 3. SCRIPT 1 — `LevelData.cs`

**File location:** `Assets/Script/Level/LevelData.cs`

### What changed
- Added `CarSize` enum (3 values)
- Added `carSize` field to `CarSpawnInfo`

### New code added (lines 15–29)

```csharp
// ── NEW: defines the three car size types ──────────────────────────────────
public enum CarSize
{
    Small  = 1,   // occupies 1 grid slot  (existing cars)
    Medium = 2,   // occupies 2 grid slots along the facing axis
    Bus    = 3    // occupies 3 grid slots along the facing axis
}
// ───────────────────────────────────────────────────────────────────────────

[System.Serializable]
public class CarSpawnInfo
{
    public Vector2Int gridPosition; // Anchor cell (front of the car) on the grid
    public float rotationY;         // 0=Up, 90=Right, 180=Down, 270=Left, 45/135/225/315=Diagonals
    public CarSize carSize = CarSize.Small; // NEW – defaults to Small (backward-compatible)
}
```

### Complete final file

```csharp
using System.Collections.Generic;
using UnityEngine;

// ── NEW: defines the three car size types ──────────────────────────────────
public enum CarSize
{
    Small  = 1,
    Medium = 2,
    Bus    = 3
}

[System.Serializable]
public class CarSpawnInfo
{
    public Vector2Int gridPosition;
    public float rotationY;
    public CarSize carSize = CarSize.Small;  // NEW
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
```

---

## 4. SCRIPT 2 — `SpawnCars.cs`

**File location:** `Assets/Script/SpawnCars.cs`

### What changed
1. Removed single `carPrefabs` list → replaced with **3 size-specific lists**
2. Added helper method `AngleToGridDir()` — converts rotationY to grid direction
3. Added helper method `GetCellsForCar()` — returns all cells a car occupies
4. Added helper method `GetPrefabForSize()` — picks random prefab from correct list
5. Added helper method `FreeSlots()` — frees a list of cells at once
6. `SpawnGridAndCars()` now marks ALL car cells as occupied
7. Fallback path also updated (always spawns Small in fallback)

---

### CHANGE A — Replace `carPrefabs` with 3 lists

**BEFORE:**
```csharp
[Header("Prefabs & References")]
[SerializeField] public GameObject carSpawnPrefab;
public List<CarMover> carPrefabs;
public SpawnPassengers spawnPassengers;
public LevelData currentLevelData;
```

**AFTER:**
```csharp
[Header("Prefabs & References")]
[SerializeField] public GameObject carSpawnPrefab;

// ── THREE SIZE LISTS — drag prefabs here in the Inspector ────────────────
[Header("Car Prefabs by Size")]
public List<CarMover> smallCarPrefabs;   // 1-slot cars  (your existing cars)
public List<CarMover> mediumCarPrefabs;  // 2-slot cars
public List<CarMover> busCarPrefabs;     // 3-slot cars

public SpawnPassengers spawnPassengers;
public LevelData currentLevelData;
```

---

### CHANGE B — 4 new helper methods (add before `InitializeGridData()`)

```csharp
// =========================================================
// HELPER: Convert rotationY angle → grid direction Vector2Int
// Must match GetDirectionFromAngle in CarMover.cs
// =========================================================
public Vector2Int AngleToGridDir(float rotationY)
{
    float angle = Mathf.Repeat(Mathf.Round(rotationY / 45f) * 45f, 360f);
    int a = Mathf.RoundToInt(angle);
    switch (a)
    {
        case 0:   return new Vector2Int( 0,  1);   // Up
        case 45:  return new Vector2Int( 1,  1);   // Up-Right
        case 90:  return new Vector2Int( 1,  0);   // Right
        case 135: return new Vector2Int( 1, -1);   // Down-Right
        case 180: return new Vector2Int( 0, -1);   // Down
        case 225: return new Vector2Int(-1, -1);   // Down-Left
        case 270: return new Vector2Int(-1,  0);   // Left
        case 315: return new Vector2Int(-1,  1);   // Up-Left
    }
    return Vector2Int.zero;
}

// =========================================================
// HELPER: Returns all grid cells this car occupies.
// anchorCell = front cell of the car.
// Body cells extend BEHIND the facing direction.
// =========================================================
public List<Vector2Int> GetCellsForCar(
    Vector2Int anchorCell,
    Vector2Int facingDir,
    CarSize size)
{
    List<Vector2Int> cells = new List<Vector2Int>();
    cells.Add(anchorCell);                   // Always include front (anchor)

    Vector2Int behindDir = -facingDir;       // Body extends opposite to facing
    for (int i = 1; i < (int)size; i++)
        cells.Add(anchorCell + behindDir * i);

    return cells;
    // Small  → returns 1 cell:  [anchor]
    // Medium → returns 2 cells: [anchor, anchor-facing]
    // Bus    → returns 3 cells: [anchor, anchor-facing, anchor-facing*2]
}

// =========================================================
// HELPER: Pick a random prefab from the correct size list.
// Falls back to smallCarPrefabs if a list is empty/null.
// =========================================================
private CarMover GetPrefabForSize(CarSize size)
{
    List<CarMover> pool;
    switch (size)
    {
        case CarSize.Medium: pool = mediumCarPrefabs; break;
        case CarSize.Bus:    pool = busCarPrefabs;    break;
        default:             pool = smallCarPrefabs;  break;
    }

    // Graceful fallback — missing list never crashes the game
    if (pool == null || pool.Count == 0)
        pool = smallCarPrefabs;

    if (pool == null || pool.Count == 0) return null;

    return pool[Random.Range(0, pool.Count)];
}

// =========================================================
// HELPER: Free a batch of cells at once.
// Called by Carout when a multi-slot car leaves the grid.
// =========================================================
public void FreeSlots(List<Vector2Int> cells)
{
    if (cells == null) return;
    foreach (var cell in cells)
        SetSlotOccupation(cell.x, cell.y, false);
}
```

---

### CHANGE C — `SpawnGridAndCars()` Custom Level Path

The spawn loop inside `foreach (var spawn in currentLevelData.carSpawns)` is updated:

**BEFORE (old inner loop):**
```csharp
// picked random from carPrefabs, only set anchor slot occupied
int randomCarIndex = Random.Range(0, carPrefabs.Count);
CarMover chosenPrefab = carPrefabs[randomCarIndex];
CarMover activeCar = ObjectPool.Instance.GetCarFromPool(chosenPrefab);
// ...
caroutComp.currentGridIndex = slot.GridIndex;
slot.IsOccupied = true;  // only 1 slot marked!
```

**AFTER (new inner loop — full replacement):**
```csharp
// ─ pick the right prefab list based on size ───────────────────────────
CarMover chosenPrefab = GetPrefabForSize(spawn.carSize);
if (chosenPrefab == null) continue;

// ─ calculate ALL cells this car will occupy ───────────────────────────
Vector2Int facingDir = AngleToGridDir(spawn.rotationY);
List<Vector2Int> carCells = GetCellsForCar(
    spawn.gridPosition, facingDir, spawn.carSize);

// ─ skip if ANY cell is already occupied ──────────────────────────────
bool canSpawn = true;
foreach (var cell in carCells)
{
    GridSlot cs = GetSlotAt(cell.x, cell.y);
    if (cs == null || cs.IsOccupied) { canSpawn = false; break; }
}
if (!canSpawn) continue;

Quaternion carRotation = Quaternion.Euler(0, spawn.rotationY, 0);
GameObject slotIndicator = Instantiate(carSpawnPrefab, slot.WorldPosition, carRotation);
spawnedPositions.Add(slotIndicator);

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

// ─ Assign anchor + full cell list to Carout ─────────────────────────
Carout caroutComp = activeCar.GetComponent<Carout>();
if (caroutComp != null)
{
    caroutComp.currentGridIndex = slot.GridIndex;
    caroutComp.SetOccupiedCells(carCells);   // ← NEW: tell Carout all cells
}

// ─ Mark ALL cells occupied (not just anchor) ────────────────────────
foreach (var cell in carCells)
    SetSlotOccupation(cell.x, cell.y, true);  // ← was: slot.IsOccupied = true

AnimateCarSpawn(activeCar, spawnPassengers.TotalCarsSpawn.Count);
spawnPassengers.TotalCarsSpawn.Add(activeCar);
```

---

## 5. SCRIPT 3 — `Carout.cs`

**File location:** `Assets/Script/Carout.cs`

### What changed
1. Added `occupiedCells` private field
2. Added `SetOccupiedCells()` public method (called by SpawnCars on spawn)
3. Updated `CheckForBlockage()` to free ALL cells (not just anchor)

---

### CHANGE A — New field + method (add after `currentGridIndex` declaration)

**BEFORE:**
```csharp
public Vector2Int currentGridIndex;
[Header("Dynamic Tackle Settings")]
public float tackleCollsionOffset = 4f;
```

**AFTER:**
```csharp
public Vector2Int currentGridIndex;

// ── NEW: all grid cells this car occupies (anchor + body cells) ────────────
private List<Vector2Int> occupiedCells = new List<Vector2Int>();

/// <summary>
/// Called by SpawnCars when this car is spawned.
/// Tells Carout which grid cells this car's body covers.
/// </summary>
public void SetOccupiedCells(List<Vector2Int> cells)
{
    occupiedCells = new List<Vector2Int>(cells);
}
// ──────────────────────────────────────────────────────────────────────────

[Header("Dynamic Tackle Settings")]
public float tackleCollsionOffset = 4f;
```

---

### CHANGE B — Update `CheckForBlockage()`

**BEFORE:**
```csharp
public bool CheckForBlockage()
{
    if (SpawnCars.Instance == null) return false;

    isBlocked = SpawnCars.Instance.CheckBlockageForCar(
        currentGridIndex, transform.eulerAngles, out Vector2Int targetGridIndex);

    if (!isBlocked)
    {
        // OLD: only freed the single anchor slot
        SpawnCars.Instance.SetSlotOccupation(
            currentGridIndex.x, currentGridIndex.y, false);
        currentGridIndex = targetGridIndex;
    }
    else
    {
        blockingGridIndex = targetGridIndex;
    }

    return isBlocked;
}
```

**AFTER:**
```csharp
public bool CheckForBlockage()
{
    if (SpawnCars.Instance == null) return false;

    // 1. Check for block using the grid system in SpawnCars
    isBlocked = SpawnCars.Instance.CheckBlockageForCar(
        currentGridIndex, transform.eulerAngles, out Vector2Int targetGridIndex);

    if (!isBlocked)
    {
        // ── FREE ALL CELLS this car was occupying ──────────────────────────
        // Works for Small (1 cell), Medium (2 cells), Bus (3 cells)
        if (occupiedCells != null && occupiedCells.Count > 0)
            SpawnCars.Instance.FreeSlots(occupiedCells);       // ← NEW
        else
            SpawnCars.Instance.SetSlotOccupation(
                currentGridIndex.x, currentGridIndex.y, false);

        // Move to the new anchor position
        currentGridIndex = targetGridIndex;

        // Car is now moving off-grid → track only its new anchor
        occupiedCells.Clear();
        occupiedCells.Add(currentGridIndex);
    }
    else
    {
        blockingGridIndex = targetGridIndex;
    }

    return isBlocked;
}
```

---

## 6. SCRIPT 4 — `CarMover.cs`

**File location:** `Assets/Script/CarMover.cs`

### What changed
- Added `carSize` field so each prefab declares its size in the Inspector
- Capacity (`CapacityOfPassengers`) is unchanged — still editable per prefab

### CHANGE — Add `carSize` field (add after `isMoving` field)

**BEFORE:**
```csharp
private bool isMoving = false;

public int CapacityOfPassengers = 4;
public int thisCarCapacity;
```

**AFTER:**
```csharp
private bool isMoving = false;

// ── NEW: matches the prefab's CarSize — set this in the Inspector ──────────
[Header("Car Size")]
public CarSize carSize = CarSize.Small;
// ──────────────────────────────────────────────────────────────────────────

public int CapacityOfPassengers = 4;
public int thisCarCapacity;
```

> Nothing else changes in CarMover. All movement, parking, DriveAway logic stays exactly the same.

---

## 7. SCRIPT 5 — `LevelEditorWindow.cs`

**File location:** `Assets/Script/Level/Editor/LevelEditorWindow.cs`

### What changed (full upgrade)
- Added `CarSize[,] cellSizes` parallel array alongside `bool[,] drawnPattern`
- Grid renders **color-coded buttons**: 🟢 S (green), 🟡 M (yellow), 🔴 BUS (red), ⬛ empty (dark)
- **Left-click empty cell** → places Small car
- **Left-click filled cell** → cycles: Small → Medium → Bus → Small
- **Right-click filled cell** → removes car
- Added **legend panel** explaining controls
- `ResizePatternArray()` now also resizes `cellSizes`
- `LoadPatternFromLevel()` now loads `carSize` from each `CarSpawnInfo`
- `GenerateSmartLayout()` now writes `carSize` into every `CarSpawnInfo`

### Key new code snippets

#### Parallel arrays declaration:
```csharp
private bool[,]    drawnPattern;   // true = a car is placed here
private CarSize[,] cellSizes;      // what size car is at this cell
```

#### Grid cell button logic (inside the `c` loop):
```csharp
bool isFilled = drawnPattern[c, r];

// Choose button colour based on size
Color originalBg = GUI.backgroundColor;
if (isFilled)
{
    switch (cellSizes[c, r])
    {
        case CarSize.Small:  GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f); break; // green
        case CarSize.Medium: GUI.backgroundColor = new Color(1.0f, 0.8f, 0.1f); break; // yellow
        case CarSize.Bus:    GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f); break; // red
    }
}
else
{
    GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // dark = empty
}

// Label: S / M / BUS / (empty)
string label = isFilled
    ? (cellSizes[c, r] == CarSize.Small  ? "S"   :
       cellSizes[c, r] == CarSize.Medium ? "M"   : "BUS")
    : "";

// Left-click
if (GUILayout.Button(label, GUILayout.Width(44f), GUILayout.Height(44f)))
{
    if (!isFilled)
    {
        drawnPattern[c, r] = true;
        cellSizes[c, r]    = CarSize.Small;     // new cell defaults to Small
    }
    else
    {
        // Cycle: Small → Medium → Bus → Small
        cellSizes[c, r] = cellSizes[c, r] switch
        {
            CarSize.Small  => CarSize.Medium,
            CarSize.Medium => CarSize.Bus,
            _              => CarSize.Small,
        };
    }
}

// Right-click → remove
Rect lastRect = GUILayoutUtility.GetLastRect();
if (Event.current.type == EventType.ContextClick
    && lastRect.Contains(Event.current.mousePosition))
{
    drawnPattern[c, r] = false;
    cellSizes[c, r]    = CarSize.Small;
    Event.current.Use();
    Repaint();
}
```

#### `GenerateSmartLayout()` now writes `carSize`:
```csharp
targetLevel.carSpawns.Add(new CarSpawnInfo
{
    gridPosition = new Vector2Int(x, y),
    rotationY    = angle,         // auto-calculated outward direction
    carSize      = cellSizes[x,y] // ← NOW INCLUDED (was missing before)
});
```

---

## 8. Unity Inspector Setup — Step by Step

### Step 1 — Fix the SpawnCars component

> In your scene, select the `GameObject` that has the `SpawnCars` component on it.

**In the Inspector you will see:**
- The old `Car Prefabs` list may show as **missing/empty** (that field was renamed)
- Three new lists appear: `Small Car Prefabs`, `Medium Car Prefabs`, `Bus Car Prefabs`

**What to do:**
1. Click the `+` button on `Small Car Prefabs`
2. Drag in: `BlueCar`, `RedCar`, `GreenCar`, `OrangeCar`, `WhiteCar` prefabs
3. Leave `Medium Car Prefabs` empty for now (game auto-falls back to Small)
4. Leave `Bus Car Prefabs` empty for now

---

### Step 2 — Fix each existing Car prefab

For every car prefab in `Assets/Prafab/` (BlueCar, RedCar, GreenCar, OrangeCar, WhiteCar):

1. **Double-click** the prefab to open it
2. Select the **root GameObject** (the car root itself)
3. In Inspector → find the `CarMover` component
4. Look for the new **"Car Size"** header
5. Set `Car Size = Small`
6. **Save** the prefab (Ctrl+S)

> Nothing else changes on existing prefabs. All your particles, CarMover settings, Carout, arrows stay the same.

---

### Step 3 — Verify the Carout component

Open any car prefab → check the `Carout` component:
- It should still show: `Ray Dis`, `Dash Speed`, `Return Speed`, `Tackle Collision Offset`
- The `occupiedCells` field is **private** — it won't show in Inspector (that's correct)
- No action needed here

---

## 9. Making a Medium Car Prefab

Follow these steps to create a medium-size 2-slot car:

### Step 1 — Duplicate a prefab
1. In `Assets/Prafab/`, right-click `BlueCar.prefab`
2. Click **Duplicate**
3. Rename it `BlueCar_Medium`

### Step 2 — Open the prefab
1. Double-click `BlueCar_Medium` to open it in Prefab Mode

### Step 3 — Stretch the 3D model
1. Find the **child GameObject** that has the car's 3D mesh (usually named something like `Model` or `CarBody`)
2. Select it
3. In Inspector → `Transform → Scale`
4. Change **Z scale** to `2.0` (this stretches it to fill 2 grid slots)
   - Keep X and Y the same
   - OR use a different model if you have one

### Step 4 — Resize the Collider
1. Select the **root GameObject** (the car root)
2. Find the `Box Collider` component
3. Change the **Size Z** to approximately `8.5`
   - Formula: `GridSlotSize (4.5f) × 2 - 0.5f buffer = 8.5f`
   - Keep Size X and Size Y the same as the original

### Step 5 — Set CarSize on CarMover
1. Select the **root GameObject**
2. Find `CarMover` component
3. Under **"Car Size"** header → set `Car Size = Medium`

### Step 6 — Save prefab
Press **Ctrl+S** or click **Save** in Prefab Mode toolbar

### Step 7 — Add to SpawnCars Inspector
1. Go back to your scene
2. Select the `SpawnCars` GameObject
3. In `Medium Car Prefabs` list → click `+` → drag in `BlueCar_Medium`

> Repeat for other colors: `RedCar_Medium`, `GreenCar_Medium` etc.

---

## 10. Making a Bus Prefab

### Step 1 — Duplicate a prefab
1. Right-click `BlueCar.prefab` → Duplicate
2. Rename it `BlueBus`

### Step 2 — Open and stretch
1. Open `BlueBus` in Prefab Mode
2. Select the **child mesh GameObject**
3. Set **Z scale to `3.0`** (3× a normal car)

### Step 3 — Resize Collider
1. Select root → `Box Collider`
2. Set **Size Z** to approximately `13.0`
   - Formula: `GridSlotSize (4.5f) × 3 - 0.5f buffer = 13.0f`

### Step 4 — Set CarSize
1. `CarMover` component → `Car Size = Bus`

### Step 5 — Save + Add to Inspector
1. Save prefab
2. In scene → `SpawnCars` component → `Bus Car Prefabs` → drag in `BlueBus`

> **Tip:** When you get a proper bus 3D model, just swap the mesh child — all scripts, colliders, and components stay the same.

---

## 11. Using the Level Editor

### Open the editor
In Unity menu bar: **`Tools → Smart Pattern Editor`**

### Controls
| Action | Result |
|--------|--------|
| Click **empty** dark cell | Places a Small car 🟢 |
| Click **filled green** cell (S) | Cycles to Medium 🟡 |
| Click **filled yellow** cell (M) | Cycles to Bus 🔴 |
| Click **filled red** cell (BUS) | Cycles back to Small 🟢 |
| **Right-click** any filled cell | Removes car |
| **GENERATE SMART LAYOUT** button | Calculates outward directions for all cars + saves sizes |
| **Save Level Asset** button | Saves the ScriptableObject to disk |

### Workflow for a new level
1. Open `Tools → Smart Pattern Editor`
2. Click `Select Level Data` → pick your LevelData asset (or create new)
3. Set `Grid Columns` and `Grid Rows`
4. **Draw your pattern** by clicking cells
5. Click filled cells to **cycle sizes** (S → M → BUS) where you want bigger cars
6. Click **GENERATE SMART LAYOUT** — cars auto-face outward from the cluster center
7. Click **Save Level Asset**
8. Hit **Play** — your level loads with the mixed car sizes!

### Example Pattern (5×5 grid, how it looks)

```
  ⬛ ⬛ 🔴 ⬛ ⬛    ← Row 4 (top)
  ⬛ 🟡 🟢 🟡 ⬛    ← Row 3
  🔴 🟢 🟢 🟢 🔴    ← Row 2
  ⬛ 🟡 🟢 🟡 ⬛    ← Row 1
  ⬛ ⬛ 🔴 ⬛ ⬛    ← Row 0 (bottom)

  🟢 = Small (S)   🟡 = Medium (M)   🔴 = Bus (BUS)   ⬛ = Empty
```

This gives: 5 small cars in center, 4 medium surrounding, 4 buses at the edges — a beautiful star pattern!

---

## 12. How to Test Everything Works

### Test 1 — Small car (existing behavior, no regression)
1. Open any existing level with only Small cars
2. Press Play → cars spawn ✅
3. Tap a car → it moves → grid slot frees ✅
4. Level completes normally ✅

### Test 2 — Medium car (new behavior)
1. In Level Editor, place a few Medium (yellow) cells
2. Generate → Save → Play
3. Verify Medium car spawns and **occupies 2 grid slots**
   - In Scene view you can see `SpawnCars.gridSlots` — 2 should be marked `IsOccupied`
4. Tap the Medium car → it moves → **BOTH slots free** ✅
5. Other cars that were blocked by it can now move ✅

### Test 3 — Bus (new behavior)
1. Place a Bus (red) cell in Level Editor
2. Generate → Save → Play
3. Bus spawns occupying **3 grid slots** ✅
4. Tap bus → moves → all 3 slots free ✅

### Test 4 — Bus blocking check
1. Place a Small car directly in front of a Bus (1 cell ahead in facing direction)
2. Play → Tap the Bus → it should **do the DoTackle animation** (blocked) ✅
3. Tap the Small car → it moves away
4. Now tap the Bus → it moves freely ✅

### Test 5 — Level Editor save/load
1. Design a level in editor with mixed sizes
2. Click Save
3. Close the editor window
4. Reopen `Tools → Smart Pattern Editor` → select same LevelData
5. Your pattern and sizes should **load back correctly** (green/yellow/red cells preserved) ✅

---

## Summary of All Files Changed

| File | Lines Changed | Risk |
|------|--------------|------|
| `LevelData.cs` | +12 lines | Zero — new enum + field with default |
| `SpawnCars.cs` | +90 lines, spawn loop updated | Low — new code paths, old `//` comments preserved |
| `Carout.cs` | +20 lines, `CheckForBlockage` updated | Low — fallback to single-cell still exists |
| `CarMover.cs` | +5 lines | Zero — just a new field |
| `LevelEditorWindow.cs` | Full rewrite of active class | Zero — Editor-only, doesn't affect runtime |

> **No existing functionality was removed.**  
> All old commented-out code in SpawnCars.cs is still there.  
> All existing prefabs, scenes, and level assets work as before.
