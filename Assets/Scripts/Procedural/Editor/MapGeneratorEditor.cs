using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    private MapGenerator mapGenerator;
    private bool showPreview = false;

    // Style variables for GUI
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private bool stylesInitialized = false;

    private void OnEnable()
    {
        mapGenerator = (MapGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();

        // Header
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Map Generator", headerStyle);
        EditorGUILayout.Space(10);

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        // Generation Controls
        EditorGUILayout.LabelField("Generation Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // Generate Map Button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate New Map", buttonStyle, GUILayout.Height(40)))
        {
            if (Application.isPlaying)
            {
                mapGenerator.GenerateMap();
            }
            else
            {
                // Record undo for editor changes
                Undo.RecordObject(mapGenerator.transform, "Generate Map");
                mapGenerator.GenerateMap();
                EditorUtility.SetDirty(mapGenerator);
            }
        }

        // Clear Map Button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear Map", buttonStyle, GUILayout.Height(40)))
        {
            if (
                EditorUtility.DisplayDialog(
                    "Clear Map",
                    "Are you sure you want to clear the current map?",
                    "Yes",
                    "Cancel"
                )
            )
            {
                ClearMap();
            }
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Quick Settings
        EditorGUILayout.LabelField("Quick Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Small Map (15x15)"))
        {
            SetMapSize(15, 2);
        }
        if (GUILayout.Button("Medium Map (27x27)"))
        {
            SetMapSize(27, 3);
        }
        if (GUILayout.Button("Large Map (39x39)"))
        {
            SetMapSize(39, 4);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Map Information
        EditorGUILayout.LabelField("Map Information", EditorStyles.boldLabel);

        // Calculate estimated tiles
        var gridSizeProp = serializedObject.FindProperty("gridSize");
        var numEndPointsProp = serializedObject.FindProperty("numEndPoints");
        int totalTiles = gridSizeProp.intValue * gridSizeProp.intValue;

        EditorGUILayout.LabelField($"Total Tiles: {totalTiles:N0}");
        EditorGUILayout.LabelField($"End Points: {numEndPointsProp.intValue}");
        EditorGUILayout.LabelField($"Map Dimensions: {gridSizeProp.intValue}x{gridSizeProp.intValue}");

        if (mapGenerator.transform.childCount > 0)
        {
            EditorGUILayout.LabelField($"Generated Objects: {mapGenerator.transform.childCount:N0}");
        }

        EditorGUILayout.Space(10);

        // Validation
        ValidateSettings();

        EditorGUILayout.Space(10);

        // Preview Toggle
        showPreview = EditorGUILayout.Toggle("Show Grid Preview", showPreview);

        if (showPreview)
        {
            EditorGUILayout.HelpBox(
                "Grid preview will be shown in Scene view when this object is selected.",
                MessageType.Info
            );
        }

        // Apply changes
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void InitializeStyles()
    {
        if (stylesInitialized)
            return;

        headerStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
        };

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 12, fontStyle = FontStyle.Bold };

        stylesInitialized = true;
    }

    private void SetMapSize(int size, int endPoints)
    {
        var gridSizeProp = serializedObject.FindProperty("gridSize");
        var numEndPointsProp = serializedObject.FindProperty("numEndPoints");

        Undo.RecordObject(target, "Change Map Size");

        gridSizeProp.intValue = size;
        numEndPointsProp.intValue = endPoints;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void ClearMap()
    {
        Undo.RecordObject(mapGenerator.transform, "Clear Map");

        // Clear all children
        for (int i = mapGenerator.transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                DestroyImmediate(mapGenerator.transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(mapGenerator.transform.GetChild(i).gameObject);
            }
        }

        EditorUtility.SetDirty(mapGenerator);
    }

    private void ValidateSettings()
    {
        var gridSizeProp = serializedObject.FindProperty("gridSize");
        var numEndPointsProp = serializedObject.FindProperty("numEndPoints");
        var tileSizeProp = serializedObject.FindProperty("tileSize");

        bool hasWarnings = false;

        if (gridSizeProp.intValue < 5)
        {
            EditorGUILayout.HelpBox("Grid size is very small. Minimum recommended size is 5x5.", MessageType.Warning);
            hasWarnings = true;
        }

        if (gridSizeProp.intValue > 100)
        {
            EditorGUILayout.HelpBox(
                "Large grid sizes may cause performance issues and long generation times.",
                MessageType.Warning
            );
            hasWarnings = true;
        }

        if (numEndPointsProp.intValue < 1)
        {
            EditorGUILayout.HelpBox("At least 1 end point is required.", MessageType.Error);
            hasWarnings = true;
        }

        if (numEndPointsProp.intValue > gridSizeProp.intValue)
        {
            EditorGUILayout.HelpBox(
                "Too many end points for the grid size. Maximum is one per edge tile.",
                MessageType.Error
            );
            hasWarnings = true;
        }

        if (tileSizeProp.floatValue <= 0)
        {
            EditorGUILayout.HelpBox("Tile size must be greater than 0.", MessageType.Error);
            hasWarnings = true;
        }

        // Check for missing prefabs
        var groundTileProp = serializedObject.FindProperty("groundTilePrefab");
        var pathStraightProp = serializedObject.FindProperty("pathStraightPrefab");
        var pathTurnProp = serializedObject.FindProperty("pathTurnPrefab");
        var enemyTowerProp = serializedObject.FindProperty("enemyTowerPrefab");
        var playerBaseProp = serializedObject.FindProperty("playerBasePrefab");

        if (
            groundTileProp.objectReferenceValue == null
            || pathStraightProp.objectReferenceValue == null
            || pathTurnProp.objectReferenceValue == null
            || enemyTowerProp.objectReferenceValue == null
            || playerBaseProp.objectReferenceValue == null
        )
        {
            EditorGUILayout.HelpBox("Some required prefabs are missing. Map generation may fail.", MessageType.Warning);
            hasWarnings = true;
        }

        if (!hasWarnings)
        {
            EditorGUILayout.HelpBox("All settings are valid. Ready to generate!", MessageType.Info);
        }
    }

    // Scene view grid preview
    private void OnSceneGUI()
    {
        if (!showPreview)
            return;

        var gridSizeProp = serializedObject.FindProperty("gridSize");
        var tileSizeProp = serializedObject.FindProperty("tileSize");

        if (gridSizeProp == null || tileSizeProp == null)
            return;

        int gridSize = gridSizeProp.intValue;
        float tileSize = tileSizeProp.floatValue;

        Vector3 startPos = mapGenerator.transform.position;

        // Draw grid lines
        Handles.color = Color.yellow;

        // Vertical lines
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = startPos + new Vector3(x * tileSize, 0, 0);
            Vector3 end = startPos + new Vector3(x * tileSize, 0, gridSize * tileSize);
            Handles.DrawLine(start, end);
        }

        // Horizontal lines
        for (int z = 0; z <= gridSize; z++)
        {
            Vector3 start = startPos + new Vector3(0, 0, z * tileSize);
            Vector3 end = startPos + new Vector3(gridSize * tileSize, 0, z * tileSize);
            Handles.DrawLine(start, end);
        }

        // Draw center point
        Vector3 centerPos = startPos + new Vector3((gridSize / 2) * tileSize, 0, (gridSize / 2) * tileSize);
        Handles.color = Color.green;
        Handles.DrawWireCube(centerPos, Vector3.one * tileSize * 0.8f);
        Handles.Label(centerPos + Vector3.up * 2, "Start Point");

        // Draw corner markers for potential end points
        Handles.color = Color.red;
        Vector3[] corners =
        {
            startPos + new Vector3(0, 0, (gridSize - 1) * tileSize), // Top-left
            startPos + new Vector3((gridSize - 1) * tileSize, 0, (gridSize - 1) * tileSize), // Top-right
            startPos + new Vector3((gridSize - 1) * tileSize, 0, 0), // Bottom-right
            startPos + new Vector3(0, 0, 0) // Bottom-left
        };

        foreach (var corner in corners)
        {
            Handles.DrawWireCube(corner, Vector3.one * tileSize * 0.6f);
        }
    }

    [MenuItem("Tools/Map Generator/Create Map Generator")]
    private static void CreateMapGenerator()
    {
        GameObject go = new GameObject("Map Generator");
        go.AddComponent<MapGenerator>();
        Selection.activeGameObject = go;

        // Center it in scene view
        if (SceneView.lastActiveSceneView != null)
        {
            go.transform.position = SceneView.lastActiveSceneView.pivot;
        }
    }
}
