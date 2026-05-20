#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEditorWindow : EditorWindow
{
    private LevelData targetLevel;
    private Vector2Int selectedCell = new Vector2Int(-1, -1);
    private Vector2 scrollPosition;

    [MenuItem("Tools/Grid Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Grid Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. Select the LevelData ScriptableObject
        EditorGUI.BeginChangeCheck();
        targetLevel = (LevelData)EditorGUILayout.ObjectField("Select Level Data", targetLevel, typeof(LevelData), false);
        if (EditorGUI.EndChangeCheck())
        {
            selectedCell = new Vector2Int(-1, -1); // Reset selection when changing assets
        }

        if (targetLevel == null)
        {
            EditorGUILayout.HelpBox("Please select a LevelData ScriptableObject to edit, or create a new one below.", MessageType.Info);
            if (GUILayout.Button("Create New Level Data Asset", GUILayout.Height(30)))
            {
                CreateNewLevelAsset();
            }
            return;
        }

        EditorGUILayout.BeginVertical("box");
        
        // 2. Base Configuration
        targetLevel.levelNumber = EditorGUILayout.IntField("Level Number", targetLevel.levelNumber);
        
        EditorGUI.BeginChangeCheck();
        int cols = EditorGUILayout.IntField("Grid Columns", targetLevel.gridColumns);
        int rows = EditorGUILayout.IntField("Grid Rows", targetLevel.gridRows);
        if (EditorGUI.EndChangeCheck())
        {
            targetLevel.gridColumns = Mathf.Clamp(cols, 1, 15);
            targetLevel.gridRows = Mathf.Clamp(rows, 1, 15);
            EditorUtility.SetDirty(targetLevel);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // 3. Visual Grid Section
        GUILayout.Label("Visual Grid Layout (Click to toggle spawn / edit properties)", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Render grid row-by-row (top-to-bottom)
        // We draw rows from rows-1 down to 0 so coordinate (0,0) is at the bottom-left visually
        float buttonSize = 55f;
        
        EditorGUILayout.BeginVertical();
        for (int r = targetLevel.gridRows - 1; r >= 0; r--)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int c = 0; c < targetLevel.gridColumns; c++)
            {
                Vector2Int coord = new Vector2Int(c, r);
                CarSpawnInfo spawn = targetLevel.carSpawns.Find(s => s.gridPosition == coord);
                
                string btnText = ".";
                Color originalBg = GUI.backgroundColor;

                // Colorize cells
                if (spawn != null)
                {
                    // Cell contains a car spawn
                    string arrow = GetArrowString(spawn.rotationY);
                    btnText = "CAR\n" + arrow;
                    
                    if (selectedCell == coord)
                        GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f); // Selected Car is Bright Green
                    else
                        GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f); // Inactive Car is Soft Blue
                }
                else
                {
                    // Empty Cell
                    btnText = $"({c},{r})";
                    if (selectedCell == coord)
                        GUI.backgroundColor = Color.yellow; // Selected Empty Cell is Yellow
                }

                if (GUILayout.Button(btnText, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
                {
                    selectedCell = coord;
                    
                    // If clicked and doesn't exist, automatically add a spawn there with facing UP
                    if (spawn == null)
                    {
                        CarSpawnInfo newSpawn = new CarSpawnInfo
                        {
                            gridPosition = coord,
                            rotationY = 0f
                        };
                        targetLevel.carSpawns.Add(newSpawn);
                        EditorUtility.SetDirty(targetLevel);
                    }
                }
                GUI.backgroundColor = originalBg;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();

        // 4. Selected Cell Properties Panel
        if (selectedCell.x >= 0 && selectedCell.x < targetLevel.gridColumns &&
            selectedCell.y >= 0 && selectedCell.y < targetLevel.gridRows)
        {
            DrawCellPropertiesPanel();
        }
        else
        {
            EditorGUILayout.HelpBox("Click any cell in the grid above to view or configure its properties.", MessageType.Info);
        }

        EditorGUILayout.Space();

        // 5. Save Button
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Save Level Asset", GUILayout.Height(35)))
        {
            EditorUtility.SetDirty(targetLevel);
            AssetDatabase.SaveAssets();
            Debug.Log($"Level_{targetLevel.levelNumber} data saved successfully!");
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawCellPropertiesPanel()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label($"Selected Coordinate: ({selectedCell.x}, {selectedCell.y})", EditorStyles.boldLabel);

        CarSpawnInfo spawn = targetLevel.carSpawns.Find(s => s.gridPosition == selectedCell);
        bool hasCar = spawn != null;

        EditorGUI.BeginChangeCheck();
        bool toggleCar = EditorGUILayout.Toggle("Spawn Car Here", hasCar);
        if (EditorGUI.EndChangeCheck())
        {
            if (toggleCar && !hasCar)
            {
                // Add new spawn
                spawn = new CarSpawnInfo { gridPosition = selectedCell, rotationY = 0f };
                targetLevel.carSpawns.Add(spawn);
            }
            else if (!toggleCar && hasCar)
            {
                // Remove existing spawn
                targetLevel.carSpawns.Remove(spawn);
                spawn = null;
            }
            EditorUtility.SetDirty(targetLevel);
        }

        if (spawn != null)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Car Facing Direction (Rotation):");

            EditorGUILayout.BeginHorizontal();

            // Up Button
            Color origColor = GUI.backgroundColor;
            if (Mathf.Approximately(spawn.rotationY, 0f)) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Up ↑", GUILayout.Height(30)))
            {
                spawn.rotationY = 0f;
                EditorUtility.SetDirty(targetLevel);
            }
            GUI.backgroundColor = origColor;

            // Right Button
            if (Mathf.Approximately(spawn.rotationY, 90f)) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Right →", GUILayout.Height(30)))
            {
                spawn.rotationY = 90f;
                EditorUtility.SetDirty(targetLevel);
            }
            GUI.backgroundColor = origColor;

            // Down Button
            if (Mathf.Approximately(spawn.rotationY, 180f)) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Down ↓", GUILayout.Height(30)))
            {
                spawn.rotationY = 180f;
                EditorUtility.SetDirty(targetLevel);
            }
            GUI.backgroundColor = origColor;

            // Left Button
            if (Mathf.Approximately(spawn.rotationY, 270f)) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Left ←", GUILayout.Height(30)))
            {
                spawn.rotationY = 270f;
                EditorUtility.SetDirty(targetLevel);
            }
            GUI.backgroundColor = origColor;

            EditorGUILayout.EndHorizontal();

            // Precise Rotation Slider as a nice fallback
            EditorGUI.BeginChangeCheck();
            float rot = EditorGUILayout.Slider("Custom Rotation (Y)", spawn.rotationY, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                // Snap to nearest 90 degrees if close
                if (Mathf.Abs(rot - 0f) < 15f || Mathf.Abs(rot - 360f) < 15f) rot = 0f;
                else if (Mathf.Abs(rot - 90f) < 15f) rot = 90f;
                else if (Mathf.Abs(rot - 180f) < 15f) rot = 180f;
                else if (Mathf.Abs(rot - 270f) < 15f) rot = 270f;
                
                spawn.rotationY = rot;
                EditorUtility.SetDirty(targetLevel);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private string GetArrowString(float rotationY)
    {
        float normalizedAngle = Mathf.Repeat(Mathf.Round(rotationY / 90f) * 90f, 360f);
        if (Mathf.Approximately(normalizedAngle, 0f)) return "↑";
        if (Mathf.Approximately(normalizedAngle, 90f)) return "→";
        if (Mathf.Approximately(normalizedAngle, 180f)) return "↓";
        if (Mathf.Approximately(normalizedAngle, 270f)) return "←";
        return normalizedAngle.ToString() + "°";
    }

    private void CreateNewLevelAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Level Data",
            "NewLevel",
            "asset",
            "Save new level data asset"
        );

        if (!string.IsNullOrEmpty(path))
        {
            LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(newLevel, path);
            AssetDatabase.SaveAssets();
            targetLevel = newLevel;
            selectedCell = new Vector2Int(-1, -1);
            Debug.Log($"Created new LevelData asset at {path}");
        }
    }
}
#endif
