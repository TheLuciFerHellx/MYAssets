# Walkthrough - Grid Level Editor Tool

We have successfully implemented the **visual Grid Level Editor tool**! Here is a summary of the scripts we updated and created, followed by a guide on how you can use this brand new tool in Unity.

---

## 1. Summary of Changes

### 🛠️ Modified Scripts

#### 1. [LevelData.cs](file:///c:/Users/ABHAYprajapati/Downloads/Car-OUT-jam-puzzle-Game-Grid/Car-OUT-jam-puzzle-Game-Grid/Assets/Script/Level/LevelData.cs)
* Added a serialized helper structure `CarSpawnInfo` storing `gridPosition` (as a `Vector2Int`) and `rotationY`.
* Added custom grid dimension properties `gridColumns` and `gridRows` (defaulting to 5x5).
* Added a serialized list `carSpawns` to record custom level draw points.
* Retained the legacy `levelNumber` and `carsToSpawn` fields to guarantee 100% backward compatibility for Levels 1–8.

#### 2. [SpawnCars.cs](file:///c:/Users/ABHAYprajapati/Downloads/Car-OUT-jam-puzzle-Game-Grid/Car-OUT-jam-puzzle-Game-Grid/Assets/Script/SpawnCars.cs)
* Modified the main grid generation and car spawning routine `SpawnGridAndCars()`.
* **If a level has a custom drawn grid**: The script dynamically resizes the gameplay arena grid dimensions to match the level, loops through the specified spawn points, picks a **random car prefab** from your list at runtime, and spawns it in the exact grid slot facing your specified direction.
* **If the level has no visual grid spawns** (legacy level assets or procedurally generated infinite levels): The script automatically falls back to your old logic, ensuring that Levels 1 to 8 and infinite gameplay still run perfectly as before.
* Updated `OnDrawGizmos()` to read from the active `LevelData` and dynamically scale the Scene View Gizmos, giving you a beautiful real-time preview of your custom grid sizes.

---

### ✨ New Editor Script

#### 3. [LevelEditorWindow.cs](file:///c:/Users/ABHAYprajapati/Downloads/Car-OUT-jam-puzzle-Game-Grid/Car-OUT-jam-puzzle-Game-Grid/Assets/Script/Level/Editor/LevelEditorWindow.cs)
* Created a highly intuitive editor window class derived from `EditorWindow`.
* Accessible from Unity's top menu bar via **`Tools > Grid Level Editor`**.
* Implemented dynamic grid sizing, an interactive cell drawing grid, golden/green cell selection highlights, arrow previews for rotations, and an integrated inspector to configure cell properties or delete/add spawns.
* Includes a built-in **Save Level Asset** button that marks the asset as dirty and writes modifications to disk immediately.
* Wrapped inside `#if UNITY_EDITOR` to ensure it is completely ignored during your final game build process.

---

## 2. How to Use the Grid Level Editor Tool

### Step 1: Open the Window
1. Open your project in Unity.
2. In the top toolbar menu, click on **`Tools`** > **`Grid Level Editor`**.
3. A new tab named **`Grid Level Editor`** will open. You can dock it anywhere in your Unity layout (e.g., next to the Inspector or Scene View).

### Step 2: Load or Create a Level Asset
* **To Edit an Existing Level**: Drag and drop any `LevelData` asset (like `Level_1.asset`) into the **Select Level Data** slot.
* **To Create a New Level**: Click the **`Create New Level Data Asset`** button. A save dialog will open in your project directory. Enter a name (e.g. `Level_9.asset`) and save it.

### Step 3: Draw Your Grid Layout
1. Enter your desired grid dimensions under **Grid Columns** and **Grid Rows** (e.g. `4` and `4`).
2. Click any cell coordinate button in the visual grid:
   * **Adding a Spawn**: Clicking an empty coordinate immediately adds a car spawn point there (colored soft blue, defaulted to face **Up ↑**).
   * **Selecting a Spawn**: Clicking an active spawn highlights it green and reveals the **Properties Panel** below.
3. Configure the car's orientation using the simple arrow buttons in the panel: `[Up ↑]`, `[Right →]`, `[Down ↓]`, or `[Left ←]`. You can also fine-tune the rotation using the custom slider.
4. **Removing a Spawn**: To clear a car spawn, click its cell, and untoggle **Spawn Car Here** in the properties panel below.

### Step 4: Save & Play!
1. When you are happy with your layout, click the green **`Save Level Asset`** button at the bottom of the tool.
2. Add your newly created level to the **`allLevels`** list on the `LevelManager` component in your scene.
3. Hit Play and have fun testing your custom puzzle designs!
