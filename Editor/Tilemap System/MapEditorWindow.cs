//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Tilemaps;
//using static WarpData;

//// ========== SUPPORTING CLASSES (Place these in separate files) ==========
//[CreateAssetMenu(menuName = "RPGToolkit/Map/TilePalette")]
//public class TilePalette : ScriptableObject
//{


//    public string paletteName = "New Palette";
//    public List<TileBase> tiles = new List<TileBase>();
//    public List<GameObject> prefabTiles = new List<GameObject>();
//    public Texture2D previewTexture;

//    public void AddTile(TileBase tile)
//    {
//        if (!tiles.Contains(tile))
//            tiles.Add(tile);
//    }

//    public void RemoveTile(TileBase tile)
//    {
//        tiles.Remove(tile);
//    }
//}

//[System.Serializable]
//public class TilemapLayerData
//{
//    public string layerName = "New Layer";
//    public Tilemap tilemap;
//    public bool isVisible = true;
//    public bool isLocked = false;
//    public bool isCollision = false;
//    public int sortingOrder = 0;
//    public Color gizmoColor = Color.white;
//    public List<MapEvent> events = new List<MapEvent>();

//    public TileBase GetTile(Vector3Int position)
//    {
//        if (tilemap == null) return null;
//        return tilemap.GetTile(position);
//    }

//    public void SetTile(Vector3Int position, TileBase tile)
//    {
//        if (tilemap == null) return;
//        tilemap.SetTile(position, tile);
//    }
//}

//[System.Serializable]
//public class MapEvent
//{
//    public string id = Guid.NewGuid().ToString();
//    public string eventName = "New Event";
//    public Vector3Int position;
//    public EventType eventType;
//    public TriggerType triggerType = TriggerType.OnInteract;
//    public List<EventAction> actions = new List<EventAction>();
//    public List<EventCondition> conditions = new List<EventCondition>();
//    public bool isActive = true;

//    [Serializable]
//    public class EventCondition
//    {
//        public string flagName;
//        public bool requiredValue = true;
//        public FlagCheckType checkType = FlagCheckType.GameFlag;
//    }
//}

//[CreateAssetMenu(menuName = "RPGToolkit/Map/MapData")]
//public class MapData : ScriptableObject
//{
//    public string mapName = "New Map";
//    public Vector2Int mapSize = new Vector2Int(50, 50);
//    public Vector2Int cellSize = new Vector2Int(1, 1);
//    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
//    public List<TilemapLayerData> layers = new List<TilemapLayerData>();
//    public List<TilePalette> palettes = new List<TilePalette>();

//    [Header("Map Properties")]
//    public string musicTrack;
//    public bool isIndoor = false;
//    public WeatherType weather = WeatherType.None;
//    public List<string> requiredFlags = new List<string>();
//    public List<WarpPoint> warpPoints = new List<WarpPoint>();

//    public void ResizeMap(Vector2Int newSize)
//    {
//        mapSize = newSize;
//        // Resize all tilemaps
//        foreach (var layer in layers)
//        {
//            if (layer.tilemap != null)
//            {
//                // Clear tiles outside new bounds
//                BoundsInt newBounds = new BoundsInt(
//                    new Vector3Int(-newSize.x / 2, -newSize.y / 2, 0),
//                    new Vector3Int(newSize.x, newSize.y, 1));

//                layer.tilemap.ClearAllEditorPreviewTiles();
//                // Note: Actual resizing would require clearing and repositioning
//            }
//        }
//    }
//}

//[System.Serializable]
//public class WarpPoint
//{
//    public Vector3Int position;
//    public string targetMap;
//    public Vector3Int targetPosition;
//    public Direction facingDirection = Direction.Down;
//}

//public enum Direction { Up, Down, Left, Right }
//public enum EventType { NPC, Dialogue, Battle, Treasure, Warp, Cutscene, Switch, Trigger, Shop, Inn, SavePoint }
//public enum TriggerType { OnInteract, OnStep, OnApproach, OnEvent, Parallel, Autorun }
//public enum ActionType { ShowDialogue, StartBattle, ChangeMap, SetFlag, MoveNPC, PlayAnimation, Wait, ChangeWeather }
//public enum FlagCheckType { GameFlag, ItemOwned, PartyMember, Variable }
//public enum WeatherType { None, Rain, Snow, Fog, Storm, Sunny }

//// ========== MAP EDITOR WINDOW (Complete Implementation) ==========
//public class MapEditorWindow : EditorWindow
//{
//    #region Static Variables
//    private static MapEditorWindow instance;
//    private static readonly float[] zoomLevels = { 0.1f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f, 4f };
//    #endregion

//    #region State Variables
//    private MapData currentMapData;
//    private TilemapLayerData[] layers;
//    private int activeLayerIndex = 0;
//    private TileBase selectedTile;
//    private TileBase[] brushTiles = new TileBase[9]; // 3x3 brush
//    private int brushSize = 1;
//    private BrushType currentBrush = BrushType.Single;
//    private EventToolMode eventToolMode = EventToolMode.None;
//    private MapEvent selectedEvent;
//    private Vector2 cameraPosition = Vector2.zero;
//    private float zoomLevel = 1f;
//    private bool showGrid = true;
//    private bool showCollision = false;
//    private bool snapToGrid = true;
//    private bool hasUnsavedChanges = false;
//    private Vector2Int selectionStart;
//    private Vector2Int selectionEnd;
//    private bool isSelecting = false;
//    private Vector2 tilePaletteScroll;
//    private Vector2 layerListScroll;
//    private Vector2 eventListScroll;
//    private TilePalette currentPalette;
//    private PaletteDisplayMode paletteDisplayMode = PaletteDisplayMode.Grid;
//    private Dictionary<TileBase, Texture2D> tilePreviews = new Dictionary<TileBase, Texture2D>();
//    private SerializedObject serializedMapData;
//    private Rect mapViewRect;
//    private Vector2 lastMousePosition;
//    private bool isPanning = false;
//    private List<MapEvent> copiedEvents = new List<MapEvent>();
//    private TileBase copiedTile;
//    private Vector3Int pasteOffset = Vector3Int.zero;
//    #endregion

//    #region Enums
//    private enum BrushType { Single, Rectangle, Circle, Fill, Eraser, Line }
//    private enum EventToolMode { None, Place, Select, Link, Move }
//    private enum PaletteDisplayMode { Grid, List }
//    private enum MapTool { Brush, Eraser, EventPlacer, TilePicker, Select }
//    private MapTool currentTool = MapTool.Brush;
//    #endregion

//    #region Constants
//    private const float TOOLBAR_HEIGHT = 40f;
//    private const float LAYER_PANEL_WIDTH = 220f;
//    private const float TILE_PALETTE_HEIGHT = 180f;
//    private const float EVENT_PANEL_WIDTH = 280f;
//    private const float MIN_ZOOM = 0.1f;
//    private const float MAX_ZOOM = 10f;
//    #endregion

//    #region Window Management
//    [MenuItem("RPGToolkit/Map Editor")]
//    public static void ShowWindow()
//    {
//        instance = GetWindow<MapEditorWindow>("Map Editor", typeof(SceneView));
//        instance.minSize = new Vector2(1024, 768);
//        instance.titleContent = new GUIContent("Map Editor", EditorGUIUtility.IconContent("GridLayoutGroup Icon").image);
//        instance.InitializeNewMap();
//    }

//    private void OnEnable()
//    {
//        instance = this;

//        // Subscribe to events
//        SceneView.duringSceneGui += OnSceneGUI;
//        Undo.undoRedoPerformed += OnUndoRedo;
//        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

//        // Initialize editor
//        LoadLastMap();
//        GenerateTilePreviews();

//        // Focus the scene view when window opens
//        if (SceneView.lastActiveSceneView != null)
//            SceneView.lastActiveSceneView.Focus();

//        wantsMouseMove = true;
//    }

//    private void OnDisable()
//    {
//        // Unsubscribe from events
//        SceneView.duringSceneGui -= OnSceneGUI;
//        Undo.undoRedoPerformed -= OnUndoRedo;
//        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

//        // Save before closing
//        if (hasUnsavedChanges)
//        {
//            if (EditorUtility.DisplayDialog("Unsaved Changes",
//                "Save changes to map?", "Save", "Don't Save"))
//            {
//                SaveMap();
//            }
//        }

//        // Cleanup
//        ClearTilePreviews();
//    }

//    private void OnPlayModeStateChanged(PlayModeStateChange state)
//    {
//        if (state == PlayModeStateChange.ExitingEditMode)
//        {
//            SaveMap();
//        }
//    }

//    private void LoadLastMap()
//    {
//        string lastMapPath = EditorPrefs.GetString("RPGToolkit_LastMap", "");
//        if (!string.IsNullOrEmpty(lastMapPath) && System.IO.File.Exists(lastMapPath))
//        {
//            currentMapData = AssetDatabase.LoadAssetAtPath<MapData>(lastMapPath);
//            if (currentMapData != null)
//            {
//                LoadMapData(currentMapData);
//                return;
//            }
//        }
//        CreateNewMap();
//    }

//    private void LoadMapData(MapData mapData)
//    {
//        currentMapData = mapData;
//        layers = currentMapData.layers.ToArray();
//        serializedMapData = new SerializedObject(currentMapData);

//        // Ensure we have at least one layer
//        if (layers.Length == 0)
//        {
//            AddNewLayer("Base");
//        }

//        // Initialize tilemaps if they don't exist
//        foreach (var layer in layers)
//        {
//            if (layer.tilemap == null)
//            {
//                CreateTilemapForLayer(layer);
//            }
//        }

//        // Load first palette if available
//        if (currentMapData.palettes.Count > 0)
//        {
//            currentPalette = currentMapData.palettes[0];
//        }

//        EditorPrefs.SetString("RPGToolkit_LastMap", AssetDatabase.GetAssetPath(currentMapData));
//        hasUnsavedChanges = false;
//        Repaint();
//    }
//    #endregion

//    #region Main GUI
//    private void OnGUI()
//    {
//        // Handle input events
//        HandleInput();

//        // Draw the main editor
//        DrawMainLayout();

//        // Handle GUI changes
//        if (GUI.changed)
//            Repaint();
//    }

//    private void DrawMainLayout()
//    {
//        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
//        {
//            DrawTopToolbar();

//            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
//            {
//                DrawLeftPanel();
//                DrawMapViewArea();
//                DrawRightPanel();
//            }
//            EditorGUILayout.EndHorizontal();

//            DrawBottomPanel();
//        }
//        EditorGUILayout.EndVertical();
//    }
//    #endregion

//    #region Top Toolbar Functions
//    private void DrawTopToolbar()
//    {
//        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(TOOLBAR_HEIGHT));
//        {
//            DrawFileMenu();
//            DrawEditMenu();
//            DrawToolsMenu();
//            DrawBrushControls();
//            DrawViewControls();
//            DrawZoomControls();
//            DrawSaveIndicator();
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawFileMenu()
//    {
//        if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
//        {
//            GenericMenu menu = new GenericMenu();
//            menu.AddItem(new GUIContent("New Map"), false, CreateNewMap);
//            menu.AddItem(new GUIContent("Open Map..."), false, OpenMap);
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Save"), false, SaveMap);
//            menu.AddItem(new GUIContent("Save As..."), false, SaveMapAs);
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Import Tileset..."), false, ImportTileset);
//            menu.AddItem(new GUIContent("Export as PNG"), false, ExportMapAsPNG);
//            menu.AddItem(new GUIContent("Export Tilemap Data"), false, ExportTilemapData);
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Recent Maps/"), false, null);
//            AddRecentMapsToMenu(menu);
//            menu.ShowAsContext();
//        }
//    }

//    private void DrawEditMenu()
//    {
//        if (GUILayout.Button("Edit", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
//        {
//            GenericMenu menu = new GenericMenu();
//            menu.AddItem(new GUIContent("Undo (Ctrl+Z)"), false, () => Undo.PerformUndo());
//            menu.AddItem(new GUIContent("Redo (Ctrl+Y)"), false, () => Undo.PerformRedo());
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Cut (Ctrl+X)"), false, CutSelection);
//            menu.AddItem(new GUIContent("Copy (Ctrl+C)"), false, CopySelection);
//            menu.AddItem(new GUIContent("Paste (Ctrl+V)"), false, PasteSelection);
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Select All Tiles"), false, SelectAllTiles);
//            menu.AddItem(new GUIContent("Clear Layer"), false, ClearActiveLayer);
//            menu.AddItem(new GUIContent("Fill Layer with Tile"), false, FillLayerWithTile);
//            menu.AddItem(new GUIContent("Mirror Layer"), false, MirrorLayer);
//            menu.AddItem(new GUIContent("Rotate Layer"), false, RotateLayer);
//            menu.ShowAsContext();
//        }
//    }

//    private void DrawToolsMenu()
//    {
//        if (GUILayout.Button("Tools", EditorStyles.toolbarDropDown, GUILayout.Width(60)))
//        {
//            GenericMenu menu = new GenericMenu();
//            menu.AddItem(new GUIContent("Tile Picker (P)"), currentTool == MapTool.TilePicker, () => currentTool = MapTool.TilePicker);
//            menu.AddItem(new GUIContent("Event Placer (E)"), currentTool == MapTool.EventPlacer, () => currentTool = MapTool.EventPlacer);
//            menu.AddItem(new GUIContent("Selection Tool (S)"), currentTool == MapTool.Select, () => currentTool = MapTool.Select);
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Toggle Collision View (C)"), showCollision, () => showCollision = !showCollision);
//            menu.AddItem(new GUIContent("Toggle Grid (G)"), showGrid, () => showGrid = !showGrid);
//            menu.AddSeparator("");
//            menu.AddItem(new GUIContent("Generate Collision from Tiles"), false, GenerateCollisionFromTiles);
//            menu.AddItem(new GUIContent("Auto-Tile Borders"), false, RunAutoTiling);
//            menu.AddItem(new GUIContent("Clean Up Empty Tiles"), false, CleanUpEmptyTiles);
//            menu.AddItem(new GUIContent("Optimize Tilemap"), false, OptimizeTilemap);
//            menu.ShowAsContext();
//        }
//    }

//    private void DrawBrushControls()
//    {
//        EditorGUI.BeginChangeCheck();
//        {
//            GUILayout.Label("Brush:", EditorStyles.miniLabel, GUILayout.Width(40));
//            currentBrush = (BrushType)EditorGUILayout.EnumPopup(currentBrush, EditorStyles.toolbarPopup, GUILayout.Width(80));

//            if (currentBrush != BrushType.Single)
//            {
//                GUILayout.Label("Size:", EditorStyles.miniLabel, GUILayout.Width(30));
//                brushSize = EditorGUILayout.IntSlider(brushSize, 1, 10, GUILayout.Width(100));
//            }

//            if (currentBrush == BrushType.Line)
//            {
//                GUILayout.Label("Mode:", EditorStyles.miniLabel, GUILayout.Width(40));
//                GUILayout.Toggle(false, "Start", EditorStyles.toolbarButton, GUILayout.Width(45));
//                GUILayout.Toggle(false, "End", EditorStyles.toolbarButton, GUILayout.Width(45));
//            }
//        }
//        if (EditorGUI.EndChangeCheck())
//        {
//            UpdateBrushPreview();
//        }
//    }

//    private void DrawViewControls()
//    {
//        GUILayout.Space(10);
//        showGrid = GUILayout.Toggle(showGrid, new GUIContent("Grid", "Toggle grid visibility (G)"),
//            EditorStyles.toolbarButton, GUILayout.Width(45));
//        showCollision = GUILayout.Toggle(showCollision, new GUIContent("Collision", "Toggle collision overlay (C)"),
//            EditorStyles.toolbarButton, GUILayout.Width(65));
//        snapToGrid = GUILayout.Toggle(snapToGrid, new GUIContent("Snap", "Snap to grid"),
//            EditorStyles.toolbarButton, GUILayout.Width(45));

//        if (GUILayout.Button("Center", EditorStyles.toolbarButton, GUILayout.Width(55)))
//        {
//            CenterView();
//        }
//    }

//    private void DrawZoomControls()
//    {
//        GUILayout.Space(10);
//        if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(25)))
//            ZoomOut();

//        float newZoom = GUILayout.HorizontalSlider(zoomLevel, MIN_ZOOM, MAX_ZOOM, GUILayout.Width(100));
//        if (Mathf.Abs(newZoom - zoomLevel) > 0.01f)
//        {
//            zoomLevel = newZoom;
//            Repaint();
//        }

//        GUILayout.Label($"{zoomLevel:0.0}x", EditorStyles.miniLabel, GUILayout.Width(40));
//        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
//            ZoomIn();

//        if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(45)))
//            ResetView();
//    }

//    private void DrawSaveIndicator()
//    {
//        GUILayout.FlexibleSpace();
//        GUI.color = hasUnsavedChanges ? Color.yellow : Color.green;
//        if (GUILayout.Button(hasUnsavedChanges ? "●" : "✓", EditorStyles.toolbarButton, GUILayout.Width(25)))
//        {
//            if (hasUnsavedChanges)
//                QuickSave();
//            else
//                EditorUtility.DisplayDialog("Map Saved", "All changes have been saved.", "OK");
//        }
//        GUI.color = Color.white;
//    }
//    #endregion

//    #region Left Panel Functions
//    private void DrawLeftPanel()
//    {
//        EditorGUILayout.BeginVertical(EditorStyles.helpBox,
//            GUILayout.Width(LAYER_PANEL_WIDTH), GUILayout.ExpandHeight(true));
//        {
//            DrawLayerHeader();
//            DrawLayerList();
//            DrawLayerProperties();
//            DrawMapProperties();
//        }
//        EditorGUILayout.EndVertical();
//    }

//    private void DrawLayerHeader()
//    {
//        EditorGUILayout.BeginHorizontal();
//        {
//            GUILayout.Label("Layers", EditorStyles.boldLabel);
//            GUILayout.FlexibleSpace();
//            if (GUILayout.Button("+", GUILayout.Width(20)))
//                AddNewLayer();
//            if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(20)))
//                ShowLayerMenu();
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawLayerList()
//    {
//        layerListScroll = EditorGUILayout.BeginScrollView(layerListScroll, GUILayout.ExpandHeight(true));
//        {
//            if (layers == null || layers.Length == 0)
//            {
//                EditorGUILayout.HelpBox("No layers. Click '+' to add one.", MessageType.Info);
//            }
//            else
//            {
//                for (int i = 0; i < layers.Length; i++)
//                {
//                    DrawLayerListItem(i);
//                }
//            }
//        }
//        EditorGUILayout.EndScrollView();
//    }

//    private void DrawLayerListItem(int index)
//    {
//        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
//        {
//            bool isActive = (index == activeLayerIndex);
//            if (isActive)
//                GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);

//            // Eye icon for visibility
//            GUIContent visibilityIcon = layers[index].isVisible ?
//                new GUIContent(EditorGUIUtility.IconContent("d_VisibilityOn").image, "Visible") :
//                new GUIContent(EditorGUIUtility.IconContent("d_VisibilityOff").image, "Hidden");
//            layers[index].isVisible = GUILayout.Toggle(layers[index].isVisible, visibilityIcon,
//                EditorStyles.label, GUILayout.Width(20));

//            // Layer name button
//            if (GUILayout.Button(layers[index].layerName,
//                isActive ? EditorStyles.whiteLabel : EditorStyles.label,
//                GUILayout.ExpandWidth(true)))
//            {
//                SetActiveLayer(index);
//            }

//            // Lock icon
//            GUIContent lockIcon = layers[index].isLocked ?
//                new GUIContent(EditorGUIUtility.IconContent("LockIcon").image, "Locked") :
//                new GUIContent(EditorGUIUtility.IconContent("LockIcon-On").image, "Unlocked");
//            layers[index].isLocked = GUILayout.Toggle(layers[index].isLocked, lockIcon,
//                EditorStyles.label, GUILayout.Width(20));

//            // Reorder buttons
//            GUI.enabled = index > 0;
//            if (GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
//                MoveLayerUp(index);
//            GUI.enabled = index < layers.Length - 1;
//            if (GUILayout.Button("↓", EditorStyles.miniButtonRight, GUILayout.Width(20)))
//                MoveLayerDown(index);
//            GUI.enabled = true;

//            GUI.backgroundColor = Color.white;
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawLayerProperties()
//    {
//        if (layers == null || layers.Length == 0) return;

//        var layer = layers[activeLayerIndex];
//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Layer Properties", EditorStyles.boldLabel);

//        EditorGUI.BeginChangeCheck();
//        {
//            layer.layerName = EditorGUILayout.TextField("Name", layer.layerName);
//            layer.sortingOrder = EditorGUILayout.IntField("Order", layer.sortingOrder);
//            layer.gizmoColor = EditorGUILayout.ColorField("Gizmo Color", layer.gizmoColor);
//            layer.isCollision = EditorGUILayout.Toggle("Is Collision", layer.isCollision);

//            EditorGUILayout.BeginHorizontal();
//            if (GUILayout.Button("Rename"))
//            {
//                // Already handled by text field
//            }
//            if (GUILayout.Button("Duplicate"))
//            {
//                DuplicateLayer(activeLayerIndex);
//            }
//            if (GUILayout.Button("Delete"))
//            {
//                if (EditorUtility.DisplayDialog("Delete Layer",
//                    $"Delete layer '{layer.layerName}'?", "Delete", "Cancel"))
//                {
//                    DeleteLayer(activeLayerIndex);
//                }
//            }
//            EditorGUILayout.EndHorizontal();
//        }
//        if (EditorGUI.EndChangeCheck())
//        {
//            hasUnsavedChanges = true;
//        }
//    }

//    private void DrawMapProperties()
//    {
//        if (currentMapData == null) return;

//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Map Properties", EditorStyles.boldLabel);

//        EditorGUI.BeginChangeCheck();
//        {
//            currentMapData.mapName = EditorGUILayout.TextField("Map Name", currentMapData.mapName);

//            EditorGUILayout.BeginHorizontal();
//            currentMapData.mapSize.x = EditorGUILayout.IntField("Width", currentMapData.mapSize.x);
//            currentMapData.mapSize.y = EditorGUILayout.IntField("Height", currentMapData.mapSize.y);
//            EditorGUILayout.EndHorizontal();

//            currentMapData.backgroundColor = EditorGUILayout.ColorField("BG Color", currentMapData.backgroundColor);
//            currentMapData.musicTrack = EditorGUILayout.TextField("Music Track", currentMapData.musicTrack);
//            currentMapData.isIndoor = EditorGUILayout.Toggle("Is Indoor", currentMapData.isIndoor);
//            currentMapData.weather = (WeatherType)EditorGUILayout.EnumPopup("Weather", currentMapData.weather);

//            EditorGUILayout.Space(5);
//            if (GUILayout.Button("Resize Map"))
//            {
//                ResizeCurrentMap();
//            }
//        }
//        if (EditorGUI.EndChangeCheck())
//        {
//            hasUnsavedChanges = true;
//        }
//    }
//    #endregion

//    #region Map View Functions
//    private void DrawMapViewArea()
//    {
//        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
//        {
//            mapViewRect = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

//            // Draw background
//            EditorGUI.DrawRect(mapViewRect, currentMapData?.backgroundColor ?? new Color(0.1f, 0.1f, 0.1f, 1f));

//            // Calculate viewport
//            Rect viewportRect = CalculateViewportRect(mapViewRect);

//            // Draw map content
//            if (currentMapData != null)
//            {
//                DrawGrid(viewportRect);
//                DrawTilemapLayers(viewportRect);
//                DrawCollisionOverlay(viewportRect);
//                DrawEventsOverlay(viewportRect);
//                DrawBrushPreview(viewportRect);
//                DrawSelectionRect(viewportRect);
//                DrawMapInfoOverlay(viewportRect);
//            }

//            // Handle input
//            HandleMapInput(viewportRect);
//        }
//        EditorGUILayout.EndVertical();
//    }

//    private Rect CalculateViewportRect(Rect containerRect)
//    {
//        // Calculate the actual drawable area based on zoom and pan
//        float scaledWidth = containerRect.width / zoomLevel;
//        float scaledHeight = containerRect.height / zoomLevel;

//        Vector2 viewportCenter = new Vector2(
//            containerRect.center.x - cameraPosition.x * zoomLevel,
//            containerRect.center.y - cameraPosition.y * zoomLevel
//        );

//        return new Rect(
//            viewportCenter.x - scaledWidth / 2,
//            viewportCenter.y - scaledHeight / 2,
//            scaledWidth,
//            scaledHeight
//        );
//    }

//    private void DrawGrid(Rect viewportRect)
//    {
//        if (!showGrid || currentMapData == null) return;

//        Handles.BeginGUI();
//        Handles.color = new Color(1, 1, 1, 0.2f);

//        // Calculate grid bounds
//        int startX = Mathf.FloorToInt(viewportRect.x / currentMapData.cellSize.x);
//        int endX = Mathf.CeilToInt((viewportRect.x + viewportRect.width) / currentMapData.cellSize.x);
//        int startY = Mathf.FloorToInt(viewportRect.y / currentMapData.cellSize.y);
//        int endY = Mathf.CeilToInt((viewportRect.y + viewportRect.height) / currentMapData.cellSize.y);

//        // Draw vertical lines
//        for (int x = startX; x <= endX; x++)
//        {
//            float screenX = x * currentMapData.cellSize.x * zoomLevel + cameraPosition.x * zoomLevel + mapViewRect.center.x;
//            Handles.DrawLine(new Vector3(screenX, mapViewRect.y, 0),
//                            new Vector3(screenX, mapViewRect.y + mapViewRect.height, 0));
//        }

//        // Draw horizontal lines
//        for (int y = startY; y <= endY; y++)
//        {
//            float screenY = y * currentMapData.cellSize.y * zoomLevel + cameraPosition.y * zoomLevel + mapViewRect.center.y;
//            Handles.DrawLine(new Vector3(mapViewRect.x, screenY, 0),
//                            new Vector3(mapViewRect.x + mapViewRect.width, screenY, 0));
//        }

//        Handles.EndGUI();
//    }

//    private void DrawTilemapLayers(Rect viewportRect)
//    {
//        if (layers == null) return;

//        // Draw from bottom to top (background to foreground)
//        for (int i = 0; i < layers.Length; i++)
//        {
//            if (layers[i].isVisible)
//                DrawTilemapLayer(layers[i], viewportRect);
//        }
//    }

//    private void DrawTilemapLayer(TilemapLayerData layer, Rect viewportRect)
//    {
//        if (layer.tilemap == null) return;

//        // Get all tile positions
//        BoundsInt bounds = layer.tilemap.cellBounds;
//        for (int x = bounds.xMin; x < bounds.xMax; x++)
//        {
//            for (int y = bounds.yMin; y < bounds.yMax; y++)
//            {
//                Vector3Int pos = new Vector3Int(x, y, 0);
//                TileBase tile = layer.tilemap.GetTile(pos);
//                if (tile != null)
//                {
//                    DrawTile(tile, pos, viewportRect, layer.isCollision);
//                }
//            }
//        }
//    }

//    private void DrawTile(TileBase tile, Vector3Int position, Rect viewportRect, bool isCollision)
//    {
//        // Convert tile position to screen position
//        Vector2 screenPos = WorldToScreenPosition(new Vector2(position.x, position.y), viewportRect);
//        Rect tileRect = new Rect(screenPos.x, screenPos.y,
//                                currentMapData.cellSize.x * zoomLevel,
//                                currentMapData.cellSize.y * zoomLevel);

//        // Draw tile preview if available
//        if (tilePreviews.ContainsKey(tile) && tilePreviews[tile] != null)
//        {
//            GUI.DrawTexture(tileRect, tilePreviews[tile]);
//        }
//        else
//        {
//            // Fallback: draw colored rectangle
//            EditorGUI.DrawRect(tileRect, isCollision ?
//                new Color(1, 0, 0, 0.3f) : new Color(1, 1, 1, 0.5f));
//        }

//        // Draw collision overlay
//        if (isCollision && showCollision)
//        {
//            Handles.BeginGUI();
//            Handles.color = new Color(1, 0, 0, 0.5f);
//            Handles.DrawSolidRectangleWithOutline(tileRect,
//                new Color(1, 0, 0, 0.2f), new Color(1, 0, 0, 0.8f));
//            Handles.EndGUI();
//        }
//    }

//    private void DrawCollisionOverlay(Rect viewportRect)
//    {
//        if (!showCollision || layers == null) return;

//        Handles.BeginGUI();
//        Handles.color = new Color(1, 0, 0, 0.3f);

//        foreach (var layer in layers)
//        {
//            if (!layer.isVisible || !layer.isCollision) continue;

//            if (layer.tilemap != null)
//            {
//                BoundsInt bounds = layer.tilemap.cellBounds;
//                for (int x = bounds.xMin; x < bounds.xMax; x++)
//                {
//                    for (int y = bounds.yMin; y < bounds.yMax; y++)
//                    {
//                        Vector3Int pos = new Vector3Int(x, y, 0);
//                        TileBase tile = layer.tilemap.GetTile(pos);
//                        if (tile != null)
//                        {
//                            Vector2 screenPos = WorldToScreenPosition(new Vector2(pos.x, pos.y), viewportRect);
//                            Rect tileRect = new Rect(screenPos.x, screenPos.y,
//                                                    currentMapData.cellSize.x * zoomLevel,
//                                                    currentMapData.cellSize.y * zoomLevel);

//                            Handles.DrawSolidRectangleWithOutline(tileRect,
//                                new Color(1, 0, 0, 0.1f), new Color(1, 0, 0, 0.5f));
//                        }
//                    }
//                }
//            }
//        }

//        Handles.EndGUI();
//    }

//    private void DrawEventsOverlay(Rect viewportRect)
//    {
//        if (layers == null) return;

//        Handles.BeginGUI();

//        foreach (var layer in layers)
//        {
//            if (!layer.isVisible) continue;

//            foreach (var mapEvent in layer.events)
//            {
//                DrawEvent(mapEvent, viewportRect, layer == layers[activeLayerIndex]);
//            }
//        }

//        Handles.EndGUI();
//    }

//    private void DrawEvent(MapEvent mapEvent, Rect viewportRect, bool isActiveLayer)
//    {
//        Vector2 screenPos = WorldToScreenPosition(new Vector2(mapEvent.position.x, mapEvent.position.y), viewportRect);
//        Rect eventRect = new Rect(screenPos.x, screenPos.y,
//                                 currentMapData.cellSize.x * zoomLevel,
//                                 currentMapData.cellSize.y * zoomLevel);

//        // Determine color based on event type
//        Color eventColor = GetEventColor(mapEvent.eventType);
//        bool isSelected = (selectedEvent == mapEvent);

//        // Draw event icon/background
//        Handles.color = isSelected ? Color.yellow : eventColor;
//        Handles.DrawSolidRectangleWithOutline(eventRect,
//            new Color(eventColor.r, eventColor.g, eventColor.b, 0.3f),
//            isSelected ? Color.yellow : eventColor);

//        // Draw event icon
//        if (zoomLevel > 0.5f) // Only draw icon when zoomed in enough
//        {
//            Texture2D icon = GetEventIconTexture(mapEvent.eventType);
//            if (icon != null)
//            {
//                Rect iconRect = new Rect(eventRect.center.x - 8, eventRect.center.y - 8, 16, 16);
//                GUI.DrawTexture(iconRect, icon);
//            }
//        }

//        // Draw event name if zoomed in enough
//        if (zoomLevel > 1f && !string.IsNullOrEmpty(mapEvent.eventName))
//        {
//            GUIStyle style = new GUIStyle(GUI.skin.label);
//            style.normal.textColor = Color.white;
//            style.alignment = TextAnchor.MiddleCenter;
//            style.fontSize = Mathf.RoundToInt(10 * zoomLevel);

//            Rect labelRect = new Rect(eventRect.x, eventRect.y - 20 * zoomLevel, eventRect.width, 20 * zoomLevel);
//            GUI.Label(labelRect, mapEvent.eventName, style);
//        }
//    }

//    private void DrawBrushPreview(Rect viewportRect)
//    {
//        if (selectedTile == null || currentBrush == BrushType.Eraser) return;

//        Event e = Event.current;
//        if (!mapViewRect.Contains(e.mousePosition)) return;

//        Vector2 worldPos = ScreenToWorldPosition(e.mousePosition, viewportRect);
//        Vector3Int tilePos = WorldToTilePosition(worldPos);

//        Handles.BeginGUI();
//        Handles.color = new Color(0, 1, 1, 0.5f);

//        // Draw brush preview based on brush type and size
//        switch (currentBrush)
//        {
//            case BrushType.Single:
//                DrawBrushCell(tilePos, viewportRect);
//                break;

//            case BrushType.Rectangle:
//                for (int x = 0; x < brushSize; x++)
//                {
//                    for (int y = 0; y < brushSize; y++)
//                    {
//                        DrawBrushCell(new Vector3Int(tilePos.x + x, tilePos.y + y, 0), viewportRect);
//                    }
//                }
//                break;

//            case BrushType.Circle:
//                float radius = brushSize / 2f;
//                for (int x = -brushSize; x <= brushSize; x++)
//                {
//                    for (int y = -brushSize; y <= brushSize; y++)
//                    {
//                        if (Mathf.Sqrt(x * x + y * y) <= radius)
//                        {
//                            DrawBrushCell(new Vector3Int(tilePos.x + x, tilePos.y + y, 0), viewportRect);
//                        }
//                    }
//                }
//                break;
//        }

//        Handles.EndGUI();
//    }

//    private void DrawBrushCell(Vector3Int tilePos, Rect viewportRect)
//    {
//        Vector2 screenPos = WorldToScreenPosition(new Vector2(tilePos.x, tilePos.y), viewportRect);
//        Rect cellRect = new Rect(screenPos.x, screenPos.y,
//                                currentMapData.cellSize.x * zoomLevel,
//                                currentMapData.cellSize.y * zoomLevel);

//        Handles.DrawSolidRectangleWithOutline(cellRect,
//            new Color(0, 1, 1, 0.1f), new Color(0, 1, 1, 0.8f));
//    }

//    private void DrawSelectionRect(Rect viewportRect)
//    {
//        if (!isSelecting) return;

//        Vector2 startScreen = WorldToScreenPosition(new Vector2(selectionStart.x, selectionStart.y), viewportRect);
//        Vector2 endScreen = WorldToScreenPosition(new Vector2(selectionEnd.x, selectionEnd.y), viewportRect);

//        Rect selectionRect = new Rect(
//            Mathf.Min(startScreen.x, endScreen.x),
//            Mathf.Min(startScreen.y, endScreen.y),
//            Mathf.Abs(endScreen.x - startScreen.x) + currentMapData.cellSize.x * zoomLevel,
//            Mathf.Abs(endScreen.y - startScreen.y) + currentMapData.cellSize.y * zoomLevel
//        );

//        Handles.BeginGUI();
//        Handles.color = new Color(0, 0.5f, 1, 0.3f);
//        Handles.DrawSolidRectangleWithOutline(selectionRect,
//            new Color(0, 0.5f, 1, 0.1f), new Color(0, 0.5f, 1, 0.8f));
//        Handles.EndGUI();
//    }

//    private void DrawMapInfoOverlay(Rect viewportRect)
//    {
//        Event e = Event.current;
//        if (!mapViewRect.Contains(e.mousePosition) || currentMapData == null) return;

//        Vector2 worldPos = ScreenToWorldPosition(e.mousePosition, viewportRect);
//        Vector3Int tilePos = WorldToTilePosition(worldPos);

//        // Create info window
//        Rect infoRect = new Rect(e.mousePosition.x + 15, e.mousePosition.y + 15, 200, 100);

//        // Background
//        EditorGUI.DrawRect(infoRect, new Color(0, 0, 0, 0.85f));

//        // Content
//        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
//        titleStyle.normal.textColor = Color.white;
//        titleStyle.fontStyle = FontStyle.Bold;

//        GUIStyle contentStyle = new GUIStyle(GUI.skin.label);
//        contentStyle.normal.textColor = Color.white;

//        float y = infoRect.y + 5;

//        // Coordinates
//        GUI.Label(new Rect(infoRect.x + 5, y, infoRect.width - 10, 20),
//            $"Tile: ({tilePos.x}, {tilePos.y})", titleStyle);
//        y += 20;

//        // Tiles at this position
//        GUI.Label(new Rect(infoRect.x + 5, y, infoRect.width - 10, 20),
//            "Layers:", contentStyle);
//        y += 20;

//        for (int i = 0; i < layers.Length; i++)
//        {
//            if (!layers[i].isVisible) continue;

//            TileBase tile = layers[i].GetTile(tilePos);
//            if (tile != null)
//            {
//                GUI.Label(new Rect(infoRect.x + 10, y, infoRect.width - 15, 20),
//                    $"{layers[i].layerName}: {tile.name}", contentStyle);
//                y += 18;
//            }
//        }

//        // Events at this position
//        var eventsAtPos = GetEventsAtPosition(tilePos);
//        if (eventsAtPos.Count > 0)
//        {
//            y += 5;
//            GUI.Label(new Rect(infoRect.x + 5, y, infoRect.width - 10, 20),
//                $"Events: {eventsAtPos.Count}", titleStyle);
//        }
//    }
//    #endregion

//    #region Right Panel Functions
//    private void DrawRightPanel()
//    {
//        EditorGUILayout.BeginVertical(EditorStyles.helpBox,
//            GUILayout.Width(EVENT_PANEL_WIDTH), GUILayout.ExpandHeight(true));
//        {
//            DrawEventToolbar();
//            DrawEventList();
//            DrawSelectedEventProperties();
//            DrawQuickActions();
//        }
//        EditorGUILayout.EndVertical();
//    }

//    private void DrawEventToolbar()
//    {
//        EditorGUILayout.BeginHorizontal();
//        {
//            GUILayout.Label("Events", EditorStyles.boldLabel);
//            GUILayout.FlexibleSpace();

//            // Event tool mode
//            eventToolMode = (EventToolMode)GUILayout.Toolbar((int)eventToolMode,
//                new[] { "Select", "Place", "Link" }, GUILayout.Width(150));

//            if (GUILayout.Button("+", GUILayout.Width(20)))
//                ShowEventCreationMenu();
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawEventList()
//    {
//        eventListScroll = EditorGUILayout.BeginScrollView(eventListScroll, GUILayout.ExpandHeight(true));
//        {
//            if (layers == null || layers.Length == 0)
//            {
//                EditorGUILayout.HelpBox("No active layer", MessageType.Info);
//            }
//            else
//            {
//                var activeLayer = layers[activeLayerIndex];
//                if (activeLayer.events.Count == 0)
//                {
//                    EditorGUILayout.HelpBox("No events in this layer", MessageType.Info);
//                }
//                else
//                {
//                    foreach (var mapEvent in activeLayer.events)
//                    {
//                        DrawEventListItem(mapEvent);
//                    }
//                }
//            }
//        }
//        EditorGUILayout.EndScrollView();
//    }

//    private void DrawEventListItem(MapEvent mapEvent)
//    {
//        bool isSelected = (selectedEvent == mapEvent);

//        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
//        {
//            if (isSelected)
//                GUI.backgroundColor = new Color(0.8f, 0.5f, 0.3f, 0.3f);

//            // Icon
//            Texture2D icon = GetEventIconTexture(mapEvent.eventType);
//            if (icon != null)
//                GUILayout.Label(new GUIContent(icon), GUILayout.Width(20), GUILayout.Height(20));

//            // Event name button
//            if (GUILayout.Button(mapEvent.eventName,
//                isSelected ? EditorStyles.whiteBoldLabel : EditorStyles.label,
//                GUILayout.ExpandWidth(true)))
//            {
//                SelectEvent(mapEvent);
//                CenterViewOnEvent(mapEvent);
//            }

//            // Position
//            GUILayout.Label($"({mapEvent.position.x},{mapEvent.position.y})",
//                EditorStyles.miniLabel, GUILayout.Width(50));

//            // Active toggle
//            mapEvent.isActive = GUILayout.Toggle(mapEvent.isActive, GUIContent.none, GUILayout.Width(20));

//            GUI.backgroundColor = Color.white;
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawSelectedEventProperties()
//    {
//        if (selectedEvent == null) return;

//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Event Properties", EditorStyles.boldLabel);

//        EditorGUI.BeginChangeCheck();
//        {
//            // Basic properties
//            selectedEvent.eventName = EditorGUILayout.TextField("Name", selectedEvent.eventName);
//            selectedEvent.eventType = (EventType)EditorGUILayout.EnumPopup("Type", selectedEvent.eventType);
//            selectedEvent.triggerType = (TriggerType)EditorGUILayout.EnumPopup("Trigger", selectedEvent.triggerType);
//            selectedEvent.isActive = EditorGUILayout.Toggle("Active", selectedEvent.isActive);

//            // Position
//            EditorGUILayout.BeginHorizontal();
//            {
//                GUILayout.Label("Position", GUILayout.Width(60));
//                selectedEvent.position.x = EditorGUILayout.IntField(selectedEvent.position.x, GUILayout.Width(40));
//                GUILayout.Label(",", GUILayout.Width(10));
//                selectedEvent.position.y = EditorGUILayout.IntField(selectedEvent.position.y, GUILayout.Width(40));
//                if (GUILayout.Button("Go", GUILayout.Width(30)))
//                {
//                    CenterViewOnEvent(selectedEvent);
//                }
//            }
//            EditorGUILayout.EndHorizontal();

//            // Event-specific properties
//            DrawEventSpecificProperties();

//            // Conditions
//            DrawEventConditions();

//            // Actions
//            DrawEventActions();
//        }
//        if (EditorGUI.EndChangeCheck())
//        {
//            hasUnsavedChanges = true;
//        }
//    }

//    private void DrawEventSpecificProperties()
//    {
//        switch (selectedEvent.eventType)
//        {
//            case EventType.NPC:
//                DrawNPCEventProperties();
//                break;
//            case EventType.Dialogue:
//                DrawDialogueEventProperties();
//                break;
//            case EventType.Warp:
//                DrawWarpEventProperties();
//                break;
//            case EventType.Battle:
//                DrawBattleEventProperties();
//                break;
//            case EventType.Shop:
//                DrawShopEventProperties();
//                break;
//            case EventType.SavePoint:
//                DrawSavePointProperties();
//                break;
//        }
//    }

//    private void DrawEventConditions()
//    {
//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

//        for (int i = 0; i < selectedEvent.conditions.Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            {
//                selectedEvent.conditions[i].flagName = EditorGUILayout.TextField(
//                    selectedEvent.conditions[i].flagName);
//                selectedEvent.conditions[i].checkType = (FlagCheckType)EditorGUILayout.EnumPopup(
//                    selectedEvent.conditions[i].checkType, GUILayout.Width(80));
//                selectedEvent.conditions[i].requiredValue = EditorGUILayout.Toggle(
//                    selectedEvent.conditions[i].requiredValue, GUILayout.Width(20));

//                if (GUILayout.Button("X", GUILayout.Width(20)))
//                {
//                    selectedEvent.conditions.RemoveAt(i);
//                    i--;
//                }
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        if (GUILayout.Button("Add Condition"))
//        {
//            selectedEvent.conditions.Add(new MapEvent.EventCondition());
//        }
//    }

//    private void DrawEventActions()
//    {
//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

//        for (int i = 0; i < selectedEvent.actions.Count; i++)
//        {
//            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//            {
//                selectedEvent.actions[i].actionType = (ActionType)EditorGUILayout.EnumPopup(
//                    "Action", selectedEvent.actions[i].actionType);
//                selectedEvent.actions[i].actionData = (ScriptableObject)EditorGUILayout.ObjectField(
//                    "Data", selectedEvent.actions[i].actionData, typeof(ScriptableObject), false);
//                selectedEvent.actions[i].delay = EditorGUILayout.FloatField("Delay", selectedEvent.actions[i].delay);
//                selectedEvent.actions[i].waitForCompletion = EditorGUILayout.Toggle(
//                    "Wait", selectedEvent.actions[i].waitForCompletion);

//                EditorGUILayout.BeginHorizontal();
//                {
//                    if (GUILayout.Button("▲") && i > 0)
//                    {
//                        var temp = selectedEvent.actions[i];
//                        selectedEvent.actions[i] = selectedEvent.actions[i - 1];
//                        selectedEvent.actions[i - 1] = temp;
//                    }
//                    if (GUILayout.Button("▼") && i < selectedEvent.actions.Count - 1)
//                    {
//                        var temp = selectedEvent.actions[i];
//                        selectedEvent.actions[i] = selectedEvent.actions[i + 1];
//                        selectedEvent.actions[i + 1] = temp;
//                    }
//                    GUILayout.FlexibleSpace();
//                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
//                    {
//                        selectedEvent.actions.RemoveAt(i);
//                        i--;
//                    }
//                }
//                EditorGUILayout.EndHorizontal();
//            }
//            EditorGUILayout.EndVertical();
//        }

//        if (GUILayout.Button("Add Action"))
//        {
//            selectedEvent.actions.Add(new EventAction());
//        }
//    }

//    private void DrawQuickActions()
//    {
//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

//        EditorGUILayout.BeginHorizontal();
//        {
//            if (GUILayout.Button("Duplicate"))
//            {
//                DuplicateSelectedEvent();
//            }
//            if (GUILayout.Button("Delete"))
//            {
//                DeleteSelectedEvent();
//            }
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();
//        {
//            if (GUILayout.Button("Export JSON"))
//            {
//                ExportEventToJson();
//            }
//            if (GUILayout.Button("Import JSON"))
//            {
//                ImportEventFromJson();
//            }
//        }
//        EditorGUILayout.EndHorizontal();
//    }
//    #endregion

//    #region Bottom Panel Functions
//    private void DrawBottomPanel()
//    {
//        EditorGUILayout.BeginVertical(EditorStyles.helpBox,
//            GUILayout.Height(TILE_PALETTE_HEIGHT));
//        {
//            DrawPaletteHeader();
//            DrawTilePalette();
//            DrawBrushPreviewPanel();
//        }
//        EditorGUILayout.EndVertical();
//    }

//    private void DrawPaletteHeader()
//    {
//        EditorGUILayout.BeginHorizontal();
//        {
//            GUILayout.Label("Tile Palette", EditorStyles.boldLabel);

//            // Palette selection
//            currentPalette = EditorGUILayout.ObjectField(currentPalette, typeof(TilePalette), false)
//                as TilePalette;

//            GUILayout.FlexibleSpace();

//            // Display mode
//            paletteDisplayMode = (PaletteDisplayMode)GUILayout.Toolbar((int)paletteDisplayMode,
//                new[] { "Grid", "List" }, GUILayout.Width(80));

//            if (GUILayout.Button("Edit", GUILayout.Width(40)))
//            {
//                if (currentPalette != null)
//                    EditorGUIUtility.PingObject(currentPalette);
//            }
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawTilePalette()
//    {
//        tilePaletteScroll = EditorGUILayout.BeginScrollView(tilePaletteScroll, false, false,
//            GUILayout.ExpandHeight(true));
//        {
//            if (currentPalette == null)
//            {
//                EditorGUILayout.HelpBox("No palette selected", MessageType.Info);
//                if (GUILayout.Button("Create New Palette"))
//                {
//                    CreateNewPalette();
//                }
//            }
//            else if (currentPalette.tiles.Count == 0)
//            {
//                EditorGUILayout.HelpBox("Palette is empty", MessageType.Info);
//                if (GUILayout.Button("Add Tiles"))
//                {
//                    AddTilesToPalette();
//                }
//            }
//            else
//            {
//                if (paletteDisplayMode == PaletteDisplayMode.Grid)
//                    DrawTileGrid();
//                else
//                    DrawTileList();
//            }
//        }
//        EditorGUILayout.EndScrollView();
//    }

//    private void DrawTileGrid()
//    {
//        int tilesPerRow = Mathf.FloorToInt((position.width - LAYER_PANEL_WIDTH - EVENT_PANEL_WIDTH) / 70);
//        float tileSize = 64f;

//        int rowCount = Mathf.CeilToInt((float)currentPalette.tiles.Count / tilesPerRow);

//        for (int row = 0; row < rowCount; row++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            {
//                for (int col = 0; col < tilesPerRow; col++)
//                {
//                    int index = row * tilesPerRow + col;
//                    if (index >= currentPalette.tiles.Count) break;

//                    DrawTileButton(currentPalette.tiles[index], tileSize);
//                }
//            }
//            EditorGUILayout.EndHorizontal();
//        }
//    }

//    private void DrawTileButton(TileBase tile, float size)
//    {
//        bool isSelected = (selectedTile == tile);

//        Rect tileRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));

//        // Background
//        EditorGUI.DrawRect(tileRect, new Color(0.2f, 0.2f, 0.2f));

//        // Preview texture
//        if (tilePreviews.ContainsKey(tile) && tilePreviews[tile] != null)
//        {
//            GUI.DrawTexture(new Rect(tileRect.x + 2, tileRect.y + 2, size - 4, size - 4), tilePreviews[tile]);
//        }

//        // Selection outline
//        if (isSelected)
//        {
//            EditorGUI.DrawRect(new Rect(tileRect.x, tileRect.y, 2, size), Color.cyan);
//            EditorGUI.DrawRect(new Rect(tileRect.x, tileRect.y, size, 2), Color.cyan);
//            EditorGUI.DrawRect(new Rect(tileRect.x + size - 2, tileRect.y, 2, size), Color.cyan);
//            EditorGUI.DrawRect(new Rect(tileRect.x, tileRect.y + size - 2, size, 2), Color.cyan);
//        }

//        // Handle click
//        if (Event.current.type == EventType.MouseDown && tileRect.Contains(Event.current.mousePosition))
//        {
//            selectedTile = tile;
//            currentTool = MapTool.Brush;
//            Event.current.Use();
//            GUI.changed = true;
//        }

//        // Tooltip
//        if (tileRect.Contains(Event.current.mousePosition))
//        {
//            GUI.Label(new Rect(tileRect.x, tileRect.y - 20, tileRect.width, 20),
//                tile.name, EditorStyles.miniLabel);
//        }
//    }

//    private void DrawBrushPreviewPanel()
//    {
//        EditorGUILayout.BeginHorizontal();
//        {
//            GUILayout.Label("Current:", GUILayout.Width(50));

//            if (selectedTile != null && tilePreviews.ContainsKey(selectedTile))
//            {
//                GUILayout.Label(tilePreviews[selectedTile], GUILayout.Width(32), GUILayout.Height(32));
//                GUILayout.Label(selectedTile.name, EditorStyles.miniLabel);
//            }
//            else
//            {
//                GUILayout.Label("None", EditorStyles.miniLabel);
//            }

//            GUILayout.FlexibleSpace();

//            // Brush pattern preview
//            if (currentBrush != BrushType.Single && currentBrush != BrushType.Eraser)
//            {
//                GUILayout.Label("Pattern:", GUILayout.Width(45));
//                DrawBrushPatternPreview();
//            }
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawBrushPatternPreview()
//    {
//        int previewSize = 16;
//        for (int y = 0; y < brushSize; y++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            for (int x = 0; x < brushSize; x++)
//            {
//                bool isInBrush = IsInBrushPattern(x, y);
//                Color color = isInBrush ? Color.white : new Color(0.3f, 0.3f, 0.3f);

//                Rect rect = GUILayoutUtility.GetRect(previewSize, previewSize,
//                    GUILayout.Width(previewSize), GUILayout.Height(previewSize));
//                EditorGUI.DrawRect(rect, color);
//            }
//            EditorGUILayout.EndHorizontal();
//        }
//    }
//    #endregion

//    #region Event Property Drawers
//    private void DrawNPCEventProperties()
//    {
//        // Create a temporary NPCData object or use existing
//        NPCData npcData = selectedEvent.actions.Find(a => a.actionData is NPCData)?.actionData as NPCData;
//        if (npcData == null)
//        {
//            EditorGUILayout.HelpBox("No NPC data assigned", MessageType.Info);
//            if (GUILayout.Button("Create NPC Data"))
//            {
//                npcData = ScriptableObject.CreateInstance<NPCData>();
//                selectedEvent.actions.Add(new EventAction
//                {
//                    actionType = ActionType.MoveNPC,
//                    actionData = npcData
//                });
//            }
//        }
//        else
//        {
//            Editor editor = Editor.CreateEditor(npcData);
//            editor.OnInspectorGUI();
//        }
//    }

//    private void DrawDialogueEventProperties()
//    {
//        DialogueGraphAsset dialogue = selectedEvent.actions.Find(a => a.actionData is DialogueGraphAsset)?.actionData as DialogueGraphAsset;
//        if (dialogue == null)
//        {
//            EditorGUILayout.HelpBox("No dialogue assigned", MessageType.Info);
//            if (GUILayout.Button("Create Dialogue"))
//            {
//                dialogue = ScriptableObject.CreateInstance<DialogueObject>();
//                selectedEvent.actions.Add(new EventAction
//                {
//                    actionType = ActionType.ShowDialogue,
//                    actionData = dialogue
//                });
//            }
//        }
//        else
//        {
//            dialogue = EditorGUILayout.ObjectField("Dialogue", dialogue, typeof(DialogueGraphAsset), false) as DialogueGraphAsset;
//        }
//    }

//    private void DrawWarpEventProperties()
//    {
//        WarpData warpData = selectedEvent.actions.Find(a => a.actionData is WarpData)?.actionData as WarpData;
//        if (warpData == null)
//        {
//            warpData = ScriptableObject.CreateInstance<WarpData>();
//            selectedEvent.actions.Add(new EventAction
//            {
//                actionType = ActionType.ChangeMap,
//                actionData = warpData
//            });
//        }

//        warpData.targetMap = EditorGUILayout.TextField("Target Map", warpData.targetMap);
//        warpData.targetPosition = EditorGUILayout.Vector3IntField("Target Position", warpData.targetPosition);
//        warpData.facingDirection = (Direction)EditorGUILayout.EnumPopup("Facing", warpData.facingDirection);
//        warpData.fadeType = (FadeType)EditorGUILayout.EnumPopup("Fade", warpData.fadeType);
//    }

//    private void DrawBattleEventProperties()
//    {
//        BattleEncounter encounter = selectedEvent.actions.Find(a => a.actionData is BattleEncounter)?.actionData as BattleEncounter;
//        if (encounter == null)
//        {
//            EditorGUILayout.HelpBox("No battle encounter assigned", MessageType.Info);
//            if (GUILayout.Button("Create Encounter"))
//            {
//                encounter = ScriptableObject.CreateInstance<BattleEncounter>();
//                selectedEvent.actions.Add(new EventAction
//                {
//                    actionType = ActionType.StartBattle,
//                    actionData = encounter
//                });
//            }
//        }
//        else
//        {
//            encounter = EditorGUILayout.ObjectField("Encounter", encounter, typeof(BattleEncounter), false) as BattleEncounter;
//        }
//    }

//    private void DrawShopEventProperties()
//    {
//        ShopData shopData = selectedEvent.actions.Find(a => a.actionData is ShopData)?.actionData as ShopData;
//        if (shopData == null)
//        {
//            shopData = ScriptableObject.CreateInstance<ShopData>();
//            selectedEvent.actions.Add(new EventAction
//            {
//                actionType = ActionType.ShowDialogue, // Custom action for shop
//                actionData = shopData
//            });
//        }

//        shopData.shopName = EditorGUILayout.TextField("Shop Name", shopData.shopName);
//        shopData.welcomeMessage = EditorGUILayout.TextField("Welcome Message", shopData.welcomeMessage);
//        EditorGUILayout.LabelField("Items for Sale:");
//        // Draw item list editor
//    }

//    private void DrawSavePointProperties()
//    {
//        EditorGUILayout.HelpBox("Save Point: Allows players to save their game.", MessageType.Info);
//        selectedEvent.triggerType = TriggerType.OnInteract;

//        // Save point specific settings
//        EditorGUILayout.LabelField("Save Message:", EditorStyles.boldLabel);
//        if (selectedEvent.actions.Count == 0)
//        {
//            selectedEvent.actions.Add(new EventAction
//            {
//                actionType = ActionType.ShowDialogue,
//                delay = 0.5f,
//                waitForCompletion = true
//            });
//        }
//    }
//    #endregion

//    #region Input Handling
//    private void HandleInput()
//    {
//        Event e = Event.current;

//        // Global keyboard shortcuts
//        if (e.type == EventType.KeyDown)
//        {
//            // Tool shortcuts
//            if (e.keyCode == KeyCode.B && e.control) // Ctrl+B for brush
//            {
//                currentTool = MapTool.Brush;
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.E && e.control) // Ctrl+E for eraser
//            {
//                currentTool = MapTool.Eraser;
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.P && e.control) // Ctrl+P for tile picker
//            {
//                currentTool = MapTool.TilePicker;
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.S && e.control) // Ctrl+S for select
//            {
//                currentTool = MapTool.Select;
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.V && e.control) // Ctrl+V for event placer
//            {
//                currentTool = MapTool.EventPlacer;
//                e.Use();
//            }

//            // View shortcuts
//            if (e.keyCode == KeyCode.G)
//            {
//                showGrid = !showGrid;
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.C)
//            {
//                showCollision = !showCollision;
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.Equals && e.shift) // + key
//            {
//                ZoomIn();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.Minus)
//            {
//                ZoomOut();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.Space)
//            {
//                CenterView();
//                e.Use();
//            }

//            // Layer navigation
//            if (e.keyCode == KeyCode.PageUp)
//            {
//                SetActiveLayer(Mathf.Max(activeLayerIndex - 1, 0));
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.PageDown)
//            {
//                SetActiveLayer(Mathf.Min(activeLayerIndex + 1, layers.Length - 1));
//                e.Use();
//            }

//            // Save/Load
//            if (e.keyCode == KeyCode.S && e.control)
//            {
//                if (e.shift)
//                    SaveMapAs();
//                else
//                    SaveMap();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.O && e.control)
//            {
//                OpenMap();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.N && e.control)
//            {
//                CreateNewMap();
//                e.Use();
//            }

//            // Edit operations
//            if (e.keyCode == KeyCode.Z && e.control && !e.shift)
//            {
//                Undo.PerformUndo();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.Z && e.control && e.shift)
//            {
//                Undo.PerformRedo();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.X && e.control)
//            {
//                CutSelection();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.C && e.control)
//            {
//                CopySelection();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.V && e.control)
//            {
//                PasteSelection();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.A && e.control)
//            {
//                SelectAllTiles();
//                e.Use();
//            }
//            if (e.keyCode == KeyCode.Delete)
//            {
//                DeleteSelection();
//                e.Use();
//            }
//        }
//    }

//    private void HandleMapInput(Rect viewportRect)
//    {
//        Event e = Event.current;

//        if (!mapViewRect.Contains(e.mousePosition))
//            return;

//        Vector2 worldPos = ScreenToWorldPosition(e.mousePosition, viewportRect);
//        Vector3Int tilePos = WorldToTilePosition(worldPos);

//        // Use proper Event.current.type checks
//        if (e.type == EventType.MouseDown && e.button == 0) // LEFT CLICK
//        {
//            if (!IsMouseOverUI() && !layers[activeLayerIndex].isLocked)
//            {
//                switch (currentTool)
//                {
//                    case MapTool.Brush:
//                        PaintTiles(tilePos);
//                        break;
//                    case MapTool.Eraser:
//                        EraseTiles(tilePos);
//                        break;
//                    case MapTool.EventPlacer:
//                        PlaceEvent(tilePos);
//                        break;
//                    case MapTool.TilePicker:
//                        PickTile(tilePos);
//                        break;
//                    case MapTool.Select:
//                        StartSelection(tilePos);
//                        break;
//                }
//                e.Use();
//            }
//        }

//        if (e.type == EventType.MouseUp && e.button == 0) // LEFT MOUSE RELEASE
//        {
//            if (isSelecting)
//            {
//                FinalizeSelection();
//                isSelecting = false;
//                e.Use();
//            }
//        }

//        if (e.type == EventType.MouseDrag && e.button == 0) // LEFT MOUSE DRAG
//        {
//            if (currentTool == MapTool.Brush && !layers[activeLayerIndex].isLocked)
//            {
//                PaintTiles(tilePos);
//                e.Use();
//            }
//            else if (isSelecting)
//            {
//                selectionEnd = new Vector2Int(tilePos.x, tilePos.y);
//                e.Use();
//                Repaint();
//            }
//        }

//        if (e.type == EventType.MouseDown && e.button == 1) // RIGHT CLICK
//        {
//            ShowTileContextMenu(tilePos);
//            e.Use();
//        }

//        if (e.type == EventType.MouseDown && e.button == 2) // MIDDLE CLICK - Pan start
//        {
//            isPanning = true;
//            lastMousePosition = e.mousePosition;
//            e.Use();
//        }

//        if (e.type == EventType.MouseDrag && e.button == 2) // MIDDLE DRAG - Pan
//        {
//            Vector2 delta = (e.mousePosition - lastMousePosition) / zoomLevel;
//            cameraPosition -= delta;
//            lastMousePosition = e.mousePosition;
//            e.Use();
//            Repaint();
//        }

//        if (e.type == EventType.MouseUp && e.button == 2) // MIDDLE RELEASE
//        {
//            isPanning = false;
//            e.Use();
//        }

//        if (e.type == EventType.ContextClick) // CONTEXT MENU
//        {
//            ShowTileContextMenu(tilePos);
//            e.Use();
//        }

//        if (e.type == EventType.ScrollWheel && e.control) // ZOOM WITH CTRL
//        {
//            float zoomDelta = -e.delta.y * 0.01f;
//            zoomLevel = Mathf.Clamp(zoomLevel + zoomDelta, MIN_ZOOM, MAX_ZOOM);
//            e.Use();
//            Repaint();
//        }

//        if (e.type == EventType.KeyDown) // KEYBOARD INPUT
//        {
//            HandleKeyDown(e);
//        }

//        if (e.type == EventType.ValidateCommand) // HANDLE UNDO/REDO COMMANDS
//        {
//            if (e.commandName == "UndoRedoPerformed")
//            {
//                RefreshTilemapDisplay();
//                e.Use();
//            }
//        }
//    }

//    private void HandleKeyDown(Event e)
//    {
//        switch (e.keyCode)
//        {
//            case KeyCode.B:
//                if (e.control) currentTool = MapTool.Brush;
//                break;
//            case KeyCode.E:
//                if (e.control) currentTool = MapTool.Eraser;
//                break;
//            case KeyCode.P:
//                if (e.control) currentTool = MapTool.TilePicker;
//                break;
//            case KeyCode.S:
//                if (e.control) currentTool = MapTool.Select;
//                break;
//            case KeyCode.V:
//                if (e.control) currentTool = MapTool.EventPlacer;
//                break;
//            case KeyCode.G:
//                showGrid = !showGrid;
//                Repaint();
//                break;
//            case KeyCode.C:
//                showCollision = !showCollision;
//                Repaint();
//                break;
//            case KeyCode.Delete:
//                DeleteSelection();
//                e.Use();
//                break;
//            case KeyCode.A:
//                if (e.control)
//                {
//                    SelectAllTiles();
//                    e.Use();
//                }
//                break;
//            case KeyCode.Z:
//                if (e.control && !e.shift)
//                {
//                    Undo.PerformUndo();
//                    e.Use();
//                }
//                else if (e.control && e.shift)
//                {
//                    Undo.PerformRedo();
//                    e.Use();
//                }
//                break;
//        }
//    }
//    private bool IsMouseOverUI()
//    {
//        // Check if mouse is over any of the panels
//        return false; // Simplified - implement actual check
//    }
//    #endregion

//    #region Tile & Event Operations
//    private void PaintTiles(Vector3Int tilePos)
//    {
//        if (selectedTile == null || layers[activeLayerIndex].isLocked) return;

//        // Register tilemap for undo
//        Tilemap tilemap = layers[activeLayerIndex].tilemap;
//        Undo.RegisterCompleteObjectUndo(tilemap, "Paint Tiles");

//        switch (currentBrush)
//        {
//            case BrushType.Single:
//                // Record individual tile change
//                TileBase previousTile = tilemap.GetTile(tilePos);
//                if (previousTile != selectedTile)
//                {
//                    tilemap.SetTile(tilePos, selectedTile);
//                    // Also record for MapData if needed
//                    EditorUtility.SetDirty(currentMapData);
//                }
//                break;

//            case BrushType.Rectangle:
//                // Record multiple tiles
//                for (int x = 0; x < brushSize; x++)
//                {
//                    for (int y = 0; y < brushSize; y++)
//                    {
//                        Vector3Int brushPos = new Vector3Int(tilePos.x + x, tilePos.y + y, 0);
//                        tilemap.SetTile(brushPos, selectedTile);
//                    }
//                }
//                EditorUtility.SetDirty(currentMapData);
//                break;

//            case BrushType.Fill:
//                // For flood fill, we need to track all changed tiles
//                List<Vector3Int> changedPositions = new List<Vector3Int>();
//                TileBase targetTile = tilemap.GetTile(tilePos);

//                if (targetTile != selectedTile)
//                {
//                    Undo.RecordObject(currentMapData, "Flood Fill");
//                    FloodFill(tilePos, targetTile, selectedTile, changedPositions);
//                }
//                break;
//        }

//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void PlaceEvent(Vector3Int tilePos)
//    {
//        var activeLayer = layers[activeLayerIndex];

//        // Check if there's already an event here
//        var existingEvent = activeLayer.events.Find(e => e.position == tilePos);
//        if (existingEvent != null)
//        {
//            SelectEvent(existingEvent);
//            return;
//        }

//        // Register MapData for undo since events are stored there
//        Undo.RecordObject(currentMapData, "Place Event");

//        MapEvent newEvent = new MapEvent
//        {
//            id = Guid.NewGuid().ToString(),
//            eventName = $"Event_{tilePos.x}_{tilePos.y}",
//            position = tilePos,
//            eventType = EventType.Dialogue,
//            triggerType = TriggerType.OnInteract
//        };

//        activeLayer.events.Add(newEvent);
//        SelectEvent(newEvent);
//        hasUnsavedChanges = true;

//        // Mark MapData as dirty
//        EditorUtility.SetDirty(currentMapData);
//        Repaint();
//    }

//    private void DeleteSelectedEvent()
//    {
//        if (selectedEvent == null) return;

//        var activeLayer = layers[activeLayerIndex];
//        if (activeLayer.events.Contains(selectedEvent))
//        {
//            // Register MapData for undo
//            Undo.RecordObject(currentMapData, "Delete Event");

//            activeLayer.events.Remove(selectedEvent);
//            selectedEvent = null;
//            hasUnsavedChanges = true;

//            EditorUtility.SetDirty(currentMapData);
//            Repaint();
//        }
//    }

//    private void EraseTiles(Vector3Int tilePos)
//    {
//        if (layers[activeLayerIndex].isLocked) return;

//        Undo.RegisterCompleteObjectUndo(layers[activeLayerIndex].tilemap, "Erase Tiles");

//        switch (currentBrush)
//        {
//            case BrushType.Single:
//                layers[activeLayerIndex].SetTile(tilePos, null);
//                break;

//            case BrushType.Rectangle:
//                for (int x = 0; x < brushSize; x++)
//                {
//                    for (int y = 0; y < brushSize; y++)
//                    {
//                        layers[activeLayerIndex].SetTile(
//                            new Vector3Int(tilePos.x + x, tilePos.y + y, 0),
//                            null);
//                    }
//                }
//                break;

//            case BrushType.Eraser: // Full eraser mode
//                layers[activeLayerIndex].tilemap.ClearAllTiles();
//                break;
//        }

//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void PlaceEvent(Vector3Int tilePos)
//    {
//        var activeLayer = layers[activeLayerIndex];

//        // Check if there's already an event here
//        var existingEvent = activeLayer.events.Find(e => e.position == tilePos);
//        if (existingEvent != null)
//        {
//            SelectEvent(existingEvent);
//            return;
//        }

//        Undo.RegisterCompleteObjectUndo(activeLayer, "Place Event");

//        MapEvent newEvent = new MapEvent
//        {
//            eventName = $"Event_{tilePos.x}_{tilePos.y}",
//            position = tilePos,
//            eventType = EventType.Dialogue, // Default
//            triggerType = TriggerType.OnInteract
//        };

//        activeLayer.events.Add(newEvent);
//        SelectEvent(newEvent);
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void PickTile(Vector3Int tilePos)
//    {
//        var activeLayer = layers[activeLayerIndex];
//        selectedTile = activeLayer.GetTile(tilePos);

//        if (selectedTile != null)
//        {
//            currentTool = MapTool.Brush;
//            EditorGUIUtility.systemCopyBuffer = selectedTile.name;
//            Debug.Log($"Picked tile: {selectedTile.name}");
//        }

//        Repaint();
//    }

//    private void StartSelection(Vector3Int tilePos)
//    {
//        selectionStart = new Vector2Int(tilePos.x, tilePos.y);
//        selectionEnd = selectionStart;
//        isSelecting = true;
//        Repaint();
//    }

//    private void ShowTileContextMenu(Vector3Int tilePos)
//    {
//        GenericMenu menu = new GenericMenu();

//        // Layer operations
//        menu.AddItem(new GUIContent("Paint Here"), false, () => PaintTiles(tilePos));
//        menu.AddItem(new GUIContent("Erase Here"), false, () => EraseTiles(tilePos));
//        menu.AddSeparator("");

//        // Event operations
//        menu.AddItem(new GUIContent("Add Event/New Event"), false, () => PlaceEvent(tilePos));
//        menu.AddItem(new GUIContent("Add Event/NPC"), false, () => CreateNPCEvent(tilePos));
//        menu.AddItem(new GUIContent("Add Event/Treasure"), false, () => CreateTreasureEvent(tilePos));
//        menu.AddItem(new GUIContent("Add Event/Warp"), false, () => CreateWarpEvent(tilePos));
//        menu.AddSeparator("");

//        // Copy/Paste
//        menu.AddItem(new GUIContent("Copy Tile"), false, () => CopyTile(tilePos));
//        menu.AddItem(new GUIContent("Paste Tile"), false, () => PasteTile(tilePos));
//        menu.AddItem(new GUIContent("Copy Event"), false, () => CopyEventAt(tilePos));
//        menu.AddItem(new GUIContent("Paste Event"), false, () => PasteEventAt(tilePos));
//        menu.AddSeparator("");

//        // Quick actions
//        var events = GetEventsAtPosition(tilePos);
//        if (events.Count > 0)
//        {
//            menu.AddItem(new GUIContent($"Edit Event ({events[0].eventName})"), false,
//                () => SelectEvent(events[0]));
//            menu.AddItem(new GUIContent("Delete Events Here"), false,
//                () => DeleteEventsAt(tilePos));
//        }

//        menu.AddItem(new GUIContent("Fill Area"), false, () => StartFillOperation(tilePos));
//        menu.AddItem(new GUIContent("Select All on Layer"), false, SelectAllOnLayer);

//        menu.ShowAsContext();
//    }
//    #endregion

//    #region Map Management
//    private void CreateNewMap()
//    {
//        if (hasUnsavedChanges)
//        {
//            if (!EditorUtility.DisplayDialog("Unsaved Changes",
//                "Save current map?", "Save", "Don't Save"))
//            {
//                return;
//            }
//            SaveMap();
//        }

//        // Create new map asset
//        string path = EditorUtility.SaveFilePanelInProject(
//            "New Map",
//            "NewMap",
//            "asset",
//            "Save new map");

//        if (string.IsNullOrEmpty(path)) return;

//        MapData newMap = ScriptableObject.CreateInstance<MapData>();
//        newMap.mapName = System.IO.Path.GetFileNameWithoutExtension(path);
//        newMap.mapSize = new Vector2Int(50, 50);

//        AssetDatabase.CreateAsset(newMap, path);
//        AssetDatabase.SaveAssets();

//        LoadMapData(newMap);

//        // Add default layers
//        AddNewLayer("Ground");
//        AddNewLayer("Objects");
//        AddNewLayer("Collision", true);

//        Debug.Log($"Created new map: {path}");
//    }

//    private void OpenMap()
//    {
//        string path = EditorUtility.OpenFilePanel("Open Map",
//            "Assets", "asset");

//        if (string.IsNullOrEmpty(path)) return;

//        path = "Assets" + path.Substring(Application.dataPath.Length);
//        MapData mapData = AssetDatabase.LoadAssetAtPath<MapData>(path);

//        if (mapData != null)
//        {
//            LoadMapData(mapData);
//        }
//        else
//        {
//            EditorUtility.DisplayDialog("Error", "Could not load map asset.", "OK");
//        }
//    }

//    private void SaveMap()
//    {
//        if (currentMapData == null) return;

//        EditorUtility.SetDirty(currentMapData);
//        AssetDatabase.SaveAssets();
//        hasUnsavedChanges = false;

//        Debug.Log($"Map saved: {currentMapData.mapName}");
//    }

//    private void SaveMapAs()
//    {
//        if (currentMapData == null) return;

//        string path = EditorUtility.SaveFilePanelInProject(
//            "Save Map As",
//            currentMapData.mapName + "_Copy",
//            "asset",
//            "Save map copy");

//        if (string.IsNullOrEmpty(path)) return;

//        MapData copy = Instantiate(currentMapData);
//        AssetDatabase.CreateAsset(copy, path);
//        AssetDatabase.SaveAssets();

//        LoadMapData(copy);
//    }

//    private void QuickSave()
//    {
//        SaveMap();
//        Repaint();
//    }

//    private void ExportMapAsPNG()
//    {
//        if (currentMapData == null || layers == null) return;

//        string path = EditorUtility.SaveFilePanel(
//            "Export Map as PNG",
//            "",
//            currentMapData.mapName + ".png",
//            "png");

//        if (string.IsNullOrEmpty(path)) return;

//        // Create a render texture and capture the map
//        // Implementation depends on how you want to render the map
//        Debug.Log($"Exporting map to: {path}");
//        // Note: You'll need to implement actual rendering logic
//    }
//    #endregion

//    #region Layer Management
//    private void AddNewLayer(string name = "New Layer", bool isCollision = false)
//    {
//        Undo.RegisterCompleteObjectUndo(currentMapData, "Add Layer");

//        TilemapLayerData newLayer = new TilemapLayerData
//        {
//            layerName = name,
//            isCollision = isCollision,
//            gizmoColor = Color.HSVToRGB(UnityEngine.Random.value, 0.7f, 1f)
//        };

//        CreateTilemapForLayer(newLayer);

//        List<TilemapLayerData> layerList = new List<TilemapLayerData>(layers);
//        layerList.Add(newLayer);
//        layers = layerList.ToArray();

//        SetActiveLayer(layers.Length - 1);
//        hasUnsavedChanges = true;

//        // Update serialized object
//        if (serializedMapData != null)
//        {
//            serializedMapData.Update();
//        }
//    }

//    private void CreateTilemapForLayer(TilemapLayerData layer)
//    {
//        // Find or create the map GameObject
//        GameObject mapGO = GameObject.Find("Map_" + currentMapData.mapName);
//        if (mapGO == null)
//        {
//            mapGO = new GameObject("Map_" + currentMapData.mapName);
//            mapGO.AddComponent<Grid>();
//        }

//        // Create tilemap GameObject for this layer
//        GameObject tilemapGO = new GameObject(layer.layerName);
//        tilemapGO.transform.SetParent(mapGO.transform);

//        Tilemap tilemap = tilemapGO.AddComponent<Tilemap>();
//        TilemapRenderer renderer = tilemapGO.AddComponent<TilemapRenderer>();
//        renderer.sortingOrder = layer.sortingOrder;

//        layer.tilemap = tilemap;
//    }

//    private void SetActiveLayer(int index)
//    {
//        if (index < 0 || index >= layers.Length) return;

//        activeLayerIndex = index;

//        // Highlight the layer in the scene view
//        if (layers[activeLayerIndex].tilemap != null)
//        {
//            Selection.activeGameObject = layers[activeLayerIndex].tilemap.gameObject;
//        }

//        Repaint();
//    }

//    private void MoveLayerUp(int index)
//    {
//        if (index <= 0) return;

//        Undo.RegisterCompleteObjectUndo(currentMapData, "Move Layer Up");

//        var temp = layers[index];
//        layers[index] = layers[index - 1];
//        layers[index - 1] = temp;

//        // Update sorting orders
//        for (int i = 0; i < layers.Length; i++)
//        {
//            if (layers[i].tilemap != null)
//            {
//                var renderer = layers[i].tilemap.GetComponent<TilemapRenderer>();
//                if (renderer != null)
//                    renderer.sortingOrder = i;
//            }
//        }

//        activeLayerIndex = index - 1;
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void MoveLayerDown(int index)
//    {
//        if (index >= layers.Length - 1) return;

//        Undo.RegisterCompleteObjectUndo(currentMapData, "Move Layer Down");

//        var temp = layers[index];
//        layers[index] = layers[index + 1];
//        layers[index + 1] = temp;

//        // Update sorting orders
//        for (int i = 0; i < layers.Length; i++)
//        {
//            if (layers[i].tilemap != null)
//            {
//                var renderer = layers[i].tilemap.GetComponent<TilemapRenderer>();
//                if (renderer != null)
//                    renderer.sortingOrder = i;
//            }
//        }

//        activeLayerIndex = index + 1;
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void DeleteLayer(int index)
//    {
//        if (layers.Length <= 1) return;

//        if (!EditorUtility.DisplayDialog("Delete Layer",
//            $"Delete layer '{layers[index].layerName}'?", "Delete", "Cancel"))
//            return;

//        Undo.RegisterCompleteObjectUndo(currentMapData, "Delete Layer");

//        // Destroy the tilemap GameObject
//        if (layers[index].tilemap != null)
//            DestroyImmediate(layers[index].tilemap.gameObject);

//        // Remove from array
//        List<TilemapLayerData> layerList = new List<TilemapLayerData>(layers);
//        layerList.RemoveAt(index);
//        layers = layerList.ToArray();

//        // Adjust active layer index
//        activeLayerIndex = Mathf.Clamp(activeLayerIndex, 0, layers.Length - 1);
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void DuplicateLayer(int index)
//    {
//        Undo.RegisterCompleteObjectUndo(currentMapData, "Duplicate Layer");

//        TilemapLayerData original = layers[index];
//        TilemapLayerData duplicate = new TilemapLayerData
//        {
//            layerName = original.layerName + " Copy",
//            isVisible = original.isVisible,
//            isLocked = original.isLocked,
//            isCollision = original.isCollision,
//            sortingOrder = original.sortingOrder,
//            gizmoColor = original.gizmoColor
//        };

//        // Duplicate tilemap
//        if (original.tilemap != null)
//        {
//            GameObject originalGO = original.tilemap.gameObject;
//            GameObject duplicateGO = Instantiate(originalGO, originalGO.transform.parent);
//            duplicateGO.name = duplicate.layerName;
//            duplicate.tilemap = duplicateGO.GetComponent<Tilemap>();

//            // Copy tiles
//            duplicate.tilemap.ClearAllTiles();
//            BoundsInt bounds = original.tilemap.cellBounds;
//            for (int x = bounds.xMin; x < bounds.xMax; x++)
//            {
//                for (int y = bounds.yMin; y < bounds.yMax; y++)
//                {
//                    Vector3Int pos = new Vector3Int(x, y, 0);
//                    TileBase tile = original.tilemap.GetTile(pos);
//                    if (tile != null)
//                        duplicate.tilemap.SetTile(pos, tile);
//                }
//            }
//        }

//        // Add to layers
//        List<TilemapLayerData> layerList = new List<TilemapLayerData>(layers);
//        layerList.Insert(index + 1, duplicate);
//        layers = layerList.ToArray();

//        SetActiveLayer(index + 1);
//        hasUnsavedChanges = true;
//    }
//    #endregion

//    #region Event Management
//    private void SelectEvent(MapEvent mapEvent)
//    {
//        selectedEvent = mapEvent;
//        eventToolMode = EventToolMode.Select;
//        Repaint();
//    }

//    private void CenterViewOnEvent(MapEvent mapEvent)
//    {
//        cameraPosition = new Vector2(-mapEvent.position.x, -mapEvent.position.y);
//        zoomLevel = 2f; // Zoom in a bit
//        Repaint();
//    }

//    private void DeleteSelectedEvent()
//    {
//        if (selectedEvent == null) return;

//        var activeLayer = layers[activeLayerIndex];
//        if (activeLayer.events.Contains(selectedEvent))
//        {
//            Undo.RegisterCompleteObjectUndo(activeLayer, "Delete Event");
//            activeLayer.events.Remove(selectedEvent);
//            selectedEvent = null;
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void DuplicateSelectedEvent()
//    {
//        if (selectedEvent == null) return;

//        var activeLayer = layers[activeLayerIndex];
//        Undo.RegisterCompleteObjectUndo(activeLayer, "Duplicate Event");

//        MapEvent duplicate = new MapEvent
//        {
//            id = Guid.NewGuid().ToString(),
//            eventName = selectedEvent.eventName + " Copy",
//            position = selectedEvent.position + new Vector3Int(1, 0, 0),
//            eventType = selectedEvent.eventType,
//            triggerType = selectedEvent.triggerType,
//            isActive = selectedEvent.isActive
//        };

//        // Deep copy conditions
//        foreach (var condition in selectedEvent.conditions)
//        {
//            duplicate.conditions.Add(new MapEvent.EventCondition
//            {
//                flagName = condition.flagName,
//                requiredValue = condition.requiredValue,
//                checkType = condition.checkType
//            });
//        }

//        // Deep copy actions
//        foreach (var action in selectedEvent.actions)
//        {
//            duplicate.actions.Add(ScriptableObject.CreateInstance<EventAction>());
//        }

//        activeLayer.events.Add(duplicate);
//        SelectEvent(duplicate);
//        hasUnsavedChanges = true;
//    }

//    private List<MapEvent> GetEventsAtPosition(Vector3Int position)
//    {
//        List<MapEvent> events = new List<MapEvent>();

//        foreach (var layer in layers)
//        {
//            events.AddRange(layer.events.Where(e => e.position == position));
//        }

//        return events;
//    }

//    private void DeleteEventsAt(Vector3Int position)
//    {
//        var activeLayer = layers[activeLayerIndex];
//        var eventsToRemove = activeLayer.events.Where(e => e.position == position).ToList();

//        if (eventsToRemove.Count == 0) return;

//        Undo.RegisterCompleteObjectUndo(activeLayer, "Delete Events");

//        foreach (var eventToRemove in eventsToRemove)
//        {
//            activeLayer.events.Remove(eventToRemove);
//            if (selectedEvent == eventToRemove)
//                selectedEvent = null;
//        }

//        hasUnsavedChanges = true;
//        Repaint();
//    }
//    #endregion

//    #region Utility Functions
//    private Vector2 ScreenToWorldPosition(Vector2 screenPos, Rect viewportRect)
//    {
//        // Convert screen position to world position
//        float worldX = (screenPos.x - mapViewRect.center.x) / zoomLevel - cameraPosition.x;
//        float worldY = (screenPos.y - mapViewRect.center.y) / zoomLevel - cameraPosition.y;

//        return new Vector2(worldX, worldY);
//    }

//    private Vector2 WorldToScreenPosition(Vector2 worldPos, Rect viewportRect)
//    {
//        // Convert world position to screen position
//        float screenX = (worldPos.x + cameraPosition.x) * zoomLevel + mapViewRect.center.x;
//        float screenY = (worldPos.y + cameraPosition.y) * zoomLevel + mapViewRect.center.y;

//        return new Vector2(screenX, screenY);
//    }

//    private Vector3Int WorldToTilePosition(Vector2 worldPos)
//    {
//        if (currentMapData == null) return Vector3Int.zero;

//        int tileX = Mathf.FloorToInt(worldPos.x / currentMapData.cellSize.x);
//        int tileY = Mathf.FloorToInt(worldPos.y / currentMapData.cellSize.y);

//        return new Vector3Int(tileX, tileY, 0);
//    }

//    private void ZoomIn()
//    {
//        // Find next zoom level
//        for (int i = 0; i < zoomLevels.Length - 1; i++)
//        {
//            if (zoomLevel >= zoomLevels[i] && zoomLevel < zoomLevels[i + 1])
//            {
//                zoomLevel = zoomLevels[i + 1];
//                break;
//            }
//        }
//        Repaint();
//    }

//    private void ZoomOut()
//    {
//        // Find previous zoom level
//        for (int i = zoomLevels.Length - 1; i > 0; i--)
//        {
//            if (zoomLevel <= zoomLevels[i] && zoomLevel > zoomLevels[i - 1])
//            {
//                zoomLevel = zoomLevels[i - 1];
//                break;
//            }
//        }
//        Repaint();
//    }

//    private void ResetView()
//    {
//        cameraPosition = Vector2.zero;
//        zoomLevel = 1f;
//        Repaint();
//    }

//    private void CenterView()
//    {
//        if (currentMapData == null) return;

//        cameraPosition = new Vector2(
//            currentMapData.mapSize.x * currentMapData.cellSize.x / 2f,
//            currentMapData.mapSize.y * currentMapData.cellSize.y / 2f
//        );
//        Repaint();
//    }

//    private Color GetEventColor(EventType eventType)
//    {
//        switch (eventType)
//        {
//            case EventType.NPC: return new Color(0, 1, 0, 1); // Green
//            case EventType.Dialogue: return new Color(0, 0.5f, 1, 1); // Blue
//            case EventType.Battle: return new Color(1, 0, 0, 1); // Red
//            case EventType.Treasure: return new Color(1, 1, 0, 1); // Yellow
//            case EventType.Warp: return new Color(1, 0, 1, 1); // Magenta
//            case EventType.Cutscene: return new Color(1, 0.5f, 0, 1); // Orange
//            case EventType.Switch: return new Color(0, 1, 1, 1); // Cyan
//            case EventType.Trigger: return new Color(0.5f, 0, 0.5f, 1); // Purple
//            case EventType.Shop: return new Color(1, 0.8f, 0, 1); // Gold
//            case EventType.Inn: return new Color(0.8f, 0.6f, 0.4f, 1); // Brown
//            case EventType.SavePoint: return new Color(1, 1, 1, 1); // White
//            default: return Color.gray;
//        }
//    }

//    private Texture2D GetEventIconTexture(EventType eventType)
//    {
//        // Use Unity's built-in icons
//        string iconName = "";
//        switch (eventType)
//        {
//            case EventType.NPC: iconName = "AvatarSelector"; break;
//            case EventType.Dialogue: iconName = "d_UnityEditor.ConsoleWindow"; break;
//            case EventType.Battle: iconName = "d_PreMatCube"; break;
//            case EventType.Treasure: iconName = "d_PreMatSphere"; break;
//            case EventType.Warp: iconName = "d_SceneLoadOut"; break;
//            case EventType.Cutscene: iconName = "d_AnimationClip"; break;
//            default: iconName = "d_GameObject"; break;
//        }

//        return EditorGUIUtility.IconContent(iconName).image as Texture2D;
//    }

//    private void GenerateTilePreviews()
//    {
//        ClearTilePreviews();

//        if (currentPalette == null) return;

//        foreach (var tile in currentPalette.tiles)
//        {
//            if (tile == null) continue;

//            // Create preview texture
//            Texture2D preview = AssetPreview.GetAssetPreview(tile);
//            if (preview != null)
//            {
//                tilePreviews[tile] = preview;
//            }
//        }
//    }

//    private void ClearTilePreviews()
//    {
//        tilePreviews.Clear();
//    }

//    private void UpdateBrushPreview()
//    {
//        // Update brush pattern based on brush type and size
//        Repaint();
//    }

//    private bool IsInBrushPattern(int x, int y)
//    {
//        switch (currentBrush)
//        {
//            case BrushType.Rectangle:
//                return x < brushSize && y < brushSize;
//            case BrushType.Circle:
//                float radius = brushSize / 2f;
//                float distance = Mathf.Sqrt((x - radius + 0.5f) * (x - radius + 0.5f) +
//                                           (y - radius + 0.5f) * (y - radius + 0.5f));
//                return distance <= radius;
//            default:
//                return x == 0 && y == 0;
//        }
//    }
//    #endregion

//    #region Scene View Integration
//    private void OnSceneGUI(SceneView sceneView)
//    {
//        if (currentMapData == null || layers == null) return;

//        DrawSceneViewGrid();
//        DrawSceneViewEvents();
//        DrawSceneViewGizmos();
//    }

//    private void DrawSceneViewGrid()
//    {
//        if (!showGrid) return;

//        Handles.color = new Color(1, 1, 1, 0.1f);

//        // Draw grid in scene view
//        for (int x = -currentMapData.mapSize.x / 2; x <= currentMapData.mapSize.x / 2; x++)
//        {
//            Vector3 start = new Vector3(x, 0, -currentMapData.mapSize.y / 2);
//            Vector3 end = new Vector3(x, 0, currentMapData.mapSize.y / 2);
//            Handles.DrawLine(start, end);
//        }

//        for (int y = -currentMapData.mapSize.y / 2; y <= currentMapData.mapSize.y / 2; y++)
//        {
//            Vector3 start = new Vector3(-currentMapData.mapSize.x / 2, 0, y);
//            Vector3 end = new Vector3(currentMapData.mapSize.x / 2, 0, y);
//            Handles.DrawLine(start, end);
//        }
//    }

//    private void DrawSceneViewEvents()
//    {
//        foreach (var layer in layers)
//        {
//            if (!layer.isVisible) continue;

//            Handles.color = layer.gizmoColor;

//            foreach (var mapEvent in layer.events)
//            {
//                Vector3 position = new Vector3(
//                    mapEvent.position.x + 0.5f,
//                    0,
//                    mapEvent.position.y + 0.5f);

//                // Draw event handle
//                float handleSize = HandleUtility.GetHandleSize(position) * 0.5f;

//                if (Handles.Button(position, Quaternion.identity,
//                    handleSize, handleSize, Handles.SphereHandleCap))
//                {
//                    SelectEvent(mapEvent);
//                    SceneView.lastActiveSceneView.pivot = position;
//                    SceneView.lastActiveSceneView.Repaint();
//                }

//                // Draw event label
//                GUIStyle style = new GUIStyle(GUI.skin.label);
//                style.normal.textColor = Color.white;
//                style.alignment = TextAnchor.MiddleCenter;
//                Handles.Label(position + Vector3.up * handleSize, mapEvent.eventName, style);
//            }
//        }
//    }

//    private void DrawSceneViewGizmos()
//    {
//        // Draw additional scene view gizmos here
//    }

//    private void OnUndoRedo()
//    {
//        RefreshTilemapDisplay();
//        Repaint();
//    }

//    private void RefreshTilemapDisplay()
//    {
//        // Refresh any cached data or displays
//        GenerateTilePreviews();
//        Repaint();
//    }
//    #endregion

//    #region Missing Function Implementations
//    private void InitializeNewMap()
//    {
//        // Already handled in LoadLastMap and CreateNewMap
//    }

//    private void ShowLayerMenu()
//    {
//        GenericMenu menu = new GenericMenu();
//        menu.AddItem(new GUIContent("Add Layer"), false, () => AddNewLayer());
//        menu.AddItem(new GUIContent("Add Collision Layer"), false, () => AddNewLayer("Collision", true));
//        menu.AddSeparator("");
//        menu.AddItem(new GUIContent("Merge Down"), false, MergeLayerDown);
//        menu.AddItem(new GUIContent("Flatten Layers"), false, FlattenLayers);
//        menu.AddSeparator("");
//        menu.AddItem(new GUIContent("Show All"), false, ShowAllLayers);
//        menu.AddItem(new GUIContent("Hide All"), false, HideAllLayers);
//        menu.AddItem(new GUIContent("Lock All"), false, LockAllLayers);
//        menu.AddItem(new GUIContent("Unlock All"), false, UnlockAllLayers);
//        menu.ShowAsContext();
//    }

//    private void ResizeCurrentMap()
//    {
//        if (currentMapData == null) return;

//        Vector2Int newSize = currentMapData.mapSize;
//        if (EditorUtility.DisplayDialog("Resize Map",
//            "This will clear tiles outside the new bounds. Continue?", "Resize", "Cancel"))
//        {
//            currentMapData.ResizeMap(newSize);
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void ShowEventCreationMenu()
//    {
//        GenericMenu menu = new GenericMenu();
//        foreach (EventType type in Enum.GetValues(typeof(EventType)))
//        {
//            menu.AddItem(new GUIContent(type.ToString()), false,
//                () => CreateEventOfType(type));
//        }
//        menu.ShowAsContext();
//    }

//    private void CreateEventOfType(EventType type)
//    {
//        Vector3Int position = new Vector3Int(0, 0, 0); // Default position
//        switch (type)
//        {
//            case EventType.NPC:
//                CreateNPCEvent(position);
//                break;
//            case EventType.Treasure:
//                CreateTreasureEvent(position);
//                break;
//            case EventType.Warp:
//                CreateWarpEvent(position);
//                break;
//            default:
//                PlaceEvent(position);
//                break;
//        }
//    }

//    private void CreateNPCEvent(Vector3Int position)
//    {
//        MapEvent npcEvent = new MapEvent
//        {
//            eventName = "NPC",
//            position = position,
//            eventType = EventType.NPC,
//            triggerType = TriggerType.OnInteract
//        };

//        layers[activeLayerIndex].events.Add(npcEvent);
//        SelectEvent(npcEvent);
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void CreateTreasureEvent(Vector3Int position)
//    {
//        MapEvent treasureEvent = new MapEvent
//        {
//            eventName = "Treasure Chest",
//            position = position,
//            eventType = EventType.Treasure,
//            triggerType = TriggerType.OnInteract
//        };

//        layers[activeLayerIndex].events.Add(treasureEvent);
//        SelectEvent(treasureEvent);
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void CreateWarpEvent(Vector3Int position)
//    {
//        MapEvent warpEvent = new MapEvent
//        {
//            eventName = "Warp Point",
//            position = position,
//            eventType = EventType.Warp,
//            triggerType = TriggerType.OnStep
//        };

//        layers[activeLayerIndex].events.Add(warpEvent);
//        SelectEvent(warpEvent);
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void CreateNewPalette()
//    {
//        string path = EditorUtility.SaveFilePanelInProject(
//            "New Tile Palette",
//            "NewPalette",
//            "asset",
//            "Save new palette");

//        if (string.IsNullOrEmpty(path)) return;

//        TilePalette newPalette = ScriptableObject.CreateInstance<TilePalette>();
//        AssetDatabase.CreateAsset(newPalette, path);
//        AssetDatabase.SaveAssets();

//        currentPalette = newPalette;
//        if (currentMapData != null)
//        {
//            currentMapData.palettes.Add(newPalette);
//            hasUnsavedChanges = true;
//        }

//        Repaint();
//    }

//    private void AddTilesToPalette()
//    {
//        if (currentPalette == null) return;

//        // Use OpenFilePanelMultiSelect for multiple file selection
//        string[] paths = EditorUtility.OpenFilePanelMultiSelect(
//            "Select Tiles",
//            "Assets",
//            "", // Extension filter - empty for all assets
//            "Prefab,Tile,Sprite" // Filter types
//        );

//        if (paths == null || paths.Length == 0) return;

//        foreach (string path in paths)
//        {
//            string assetPath = "Assets" + path.Substring(Application.dataPath.Length);

//            // Load as TileBase
//            TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
//            if (tile != null)
//            {
//                currentPalette.AddTile(tile);
//                continue;
//            }

//            // Try loading as GameObject (for prefab tiles)
//            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
//            if (prefab != null && prefab.GetComponent<TileBase>() == null)
//            {
//                // Convert GameObject to RuleTile or CustomTile
//                RuleTile ruleTile = CreateRuleTileFromPrefab(prefab);
//                if (ruleTile != null)
//                {
//                    currentPalette.AddTile(ruleTile);
//                }
//            }

//            // Try loading as Sprite to create a BasicTile
//            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
//            if (sprite != null)
//            {
//                BasicTile basicTile = CreateBasicTileFromSprite(sprite);
//                if (basicTile != null)
//                {
//                    currentPalette.AddTile(basicTile);
//                }
//            }
//        }

//        GenerateTilePreviews();
//        Repaint();

//        // Save the palette
//        EditorUtility.SetDirty(currentPalette);
//        AssetDatabase.SaveAssets();
//    }

//    private RuleTile CreateRuleTileFromPrefab(GameObject prefab)
//    {
//        string tilePath = AssetDatabase.GetAssetPath(prefab).Replace(".prefab", "_Tile.asset");
//        RuleTile ruleTile = ScriptableObject.CreateInstance<RuleTile>();

//        // Configure rule tile based on prefab
//        ruleTile.m_DefaultSprite = prefab.GetComponent<SpriteRenderer>()?.sprite;
//        ruleTile.name = prefab.name + "_Tile";

//        AssetDatabase.CreateAsset(ruleTile, tilePath);
//        AssetDatabase.SaveAssets();

//        return ruleTile;
//    }

//    private BasicTile CreateBasicTileFromSprite(Sprite sprite)
//    {
//        string tilePath = AssetDatabase.GetAssetPath(sprite).Replace(".png", "_Tile.asset");
//        BasicTile basicTile = ScriptableObject.CreateInstance<BasicTile>();

//        basicTile.sprite = sprite;
//        basicTile.name = sprite.name + "_Tile";
//        basicTile.colliderType = Tile.ColliderType.Sprite;

//        AssetDatabase.CreateAsset(basicTile, tilePath);
//        AssetDatabase.SaveAssets();

//        return basicTile;
//    }

//    private void DrawTileList()
//    {
//        foreach (var tile in currentPalette.tiles)
//        {
//            if (tile == null) continue;

//            EditorGUILayout.BeginHorizontal();
//            {
//                bool isSelected = (selectedTile == tile);
//                if (GUILayout.Toggle(isSelected, tile.name, EditorStyles.radioButton))
//                {
//                    selectedTile = tile;
//                }

//                if (tilePreviews.ContainsKey(tile) && tilePreviews[tile] != null)
//                {
//                    GUILayout.Label(tilePreviews[tile], GUILayout.Width(32), GUILayout.Height(32));
//                }
//            }
//            EditorGUILayout.EndHorizontal();
//        }
//    }

//    private void CutSelection()
//    {
//        CopySelection();
//        DeleteSelection();
//    }

//    private void CopySelection()
//    {
//        if (isSelecting)
//        {
//            // Copy selected area
//            Debug.Log("Copy selection area");
//        }
//        else if (selectedEvent != null)
//        {
//            copiedEvents.Clear();
//            copiedEvents.Add(selectedEvent);
//            Debug.Log($"Copied event: {selectedEvent.eventName}");
//        }
//        else if (selectedTile != null)
//        {
//            copiedTile = selectedTile;
//            Debug.Log($"Copied tile: {selectedTile.name}");
//        }
//    }

//    private void PasteSelection()
//    {
//        if (copiedEvents.Count > 0)
//        {
//            // Paste events with offset
//            foreach (var eventToPaste in copiedEvents)
//            {
//                MapEvent duplicate = new MapEvent
//                {
//                    id = Guid.NewGuid().ToString(),
//                    eventName = eventToPaste.eventName + " Copy",
//                    position = eventToPaste.position + pasteOffset,
//                    eventType = eventToPaste.eventType,
//                    triggerType = eventToPaste.triggerType
//                };

//                layers[activeLayerIndex].events.Add(duplicate);
//            }
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//        else if (copiedTile != null)
//        {
//            selectedTile = copiedTile;
//            currentTool = MapTool.Brush;
//            Repaint();
//        }
//    }

//    private void DeleteSelection()
//    {
//        if (isSelecting)
//        {
//            // Delete selected area
//            DeleteSelectedArea();
//        }
//        else if (selectedEvent != null)
//        {
//            DeleteSelectedEvent();
//        }
//    }

//    private void SelectAllTiles()
//    {
//        // Select all tiles in active layer
//        isSelecting = true;
//        selectionStart = new Vector2Int(-currentMapData.mapSize.x / 2, -currentMapData.mapSize.y / 2);
//        selectionEnd = new Vector2Int(currentMapData.mapSize.x / 2, currentMapData.mapSize.y / 2);
//        Repaint();
//    }

//    private void ClearActiveLayer()
//    {
//        if (layers[activeLayerIndex].isLocked) return;

//        if (EditorUtility.DisplayDialog("Clear Layer",
//            $"Clear all tiles in layer '{layers[activeLayerIndex].layerName}'?", "Clear", "Cancel"))
//        {
//            Undo.RegisterCompleteObjectUndo(layers[activeLayerIndex].tilemap, "Clear Layer");
//            layers[activeLayerIndex].tilemap.ClearAllTiles();
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void FillLayerWithTile()
//    {
//        if (selectedTile == null || layers[activeLayerIndex].isLocked) return;

//        if (EditorUtility.DisplayDialog("Fill Layer",
//            $"Fill layer '{layers[activeLayerIndex].layerName}' with selected tile?", "Fill", "Cancel"))
//        {
//            Undo.RegisterCompleteObjectUndo(layers[activeLayerIndex].tilemap, "Fill Layer");

//            for (int x = -currentMapData.mapSize.x / 2; x < currentMapData.mapSize.x / 2; x++)
//            {
//                for (int y = -currentMapData.mapSize.y / 2; y < currentMapData.mapSize.y / 2; y++)
//                {
//                    layers[activeLayerIndex].SetTile(new Vector3Int(x, y, 0), selectedTile);
//                }
//            }

//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void MirrorLayer()
//    {
//        if (layers[activeLayerIndex].isLocked) return;

//        Undo.RegisterCompleteObjectUndo(layers[activeLayerIndex].tilemap, "Mirror Layer");

//        // Implement mirror logic
//        Debug.Log("Mirror layer");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void RotateLayer()
//    {
//        if (layers[activeLayerIndex].isLocked) return;

//        Undo.RegisterCompleteObjectUndo(layers[activeLayerIndex].tilemap, "Rotate Layer");

//        // Implement rotate logic
//        Debug.Log("Rotate layer");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void GenerateCollisionFromTiles()
//    {
//        // Generate collision layer based on tile properties
//        Debug.Log("Generate collision from tiles");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void RunAutoTiling()
//    {
//        // Apply auto-tiling rules
//        Debug.Log("Run auto-tiling");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void CleanUpEmptyTiles()
//    {
//        // Remove empty tiles from tilemap
//        Debug.Log("Clean up empty tiles");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void OptimizeTilemap()
//    {
//        // Optimize tilemap for performance
//        Debug.Log("Optimize tilemap");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void MergeLayerDown()
//    {
//        if (activeLayerIndex <= 0) return;

//        // Merge current layer with layer below
//        Debug.Log("Merge layer down");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void FlattenLayers()
//    {
//        // Flatten all visible layers into one
//        Debug.Log("Flatten layers");
//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void ShowAllLayers()
//    {
//        foreach (var layer in layers)
//        {
//            layer.isVisible = true;
//        }
//        Repaint();
//    }

//    private void HideAllLayers()
//    {
//        foreach (var layer in layers)
//        {
//            layer.isVisible = false;
//        }
//        Repaint();
//    }

//    private void LockAllLayers()
//    {
//        foreach (var layer in layers)
//        {
//            layer.isLocked = true;
//        }
//        Repaint();
//    }

//    private void UnlockAllLayers()
//    {
//        foreach (var layer in layers)
//        {
//            layer.isLocked = false;
//        }
//        Repaint();
//    }

//    private void CopyTile(Vector3Int tilePos)
//    {
//        copiedTile = layers[activeLayerIndex].GetTile(tilePos);
//        if (copiedTile != null)
//        {
//            Debug.Log($"Copied tile: {copiedTile.name}");
//        }
//    }

//    private void PasteTile(Vector3Int tilePos)
//    {
//        if (copiedTile != null)
//        {
//            layers[activeLayerIndex].SetTile(tilePos, copiedTile);
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void CopyEventAt(Vector3Int tilePos)
//    {
//        var eventToCopy = GetEventsAtPosition(tilePos).FirstOrDefault();
//        if (eventToCopy != null)
//        {
//            copiedEvents.Clear();
//            copiedEvents.Add(eventToCopy);
//            Debug.Log($"Copied event: {eventToCopy.eventName}");
//        }
//    }

//    private void PasteEventAt(Vector3Int tilePos)
//    {
//        if (copiedEvents.Count > 0)
//        {
//            var eventToPaste = copiedEvents[0];
//            MapEvent duplicate = new MapEvent
//            {
//                id = Guid.NewGuid().ToString(),
//                eventName = eventToPaste.eventName + " Copy",
//                position = tilePos,
//                eventType = eventToPaste.eventType,
//                triggerType = eventToPaste.triggerType
//            };

//            layers[activeLayerIndex].events.Add(duplicate);
//            SelectEvent(duplicate);
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void StartFillOperation(Vector3Int tilePos)
//    {
//        if (selectedTile == null) return;

//        FloodFill(tilePos, selectedTile);
//    }

//    private void SelectAllOnLayer()
//    {
//        // Select all non-empty tiles on active layer
//        isSelecting = true;

//        BoundsInt bounds = layers[activeLayerIndex].tilemap.cellBounds;
//        selectionStart = new Vector2Int(bounds.xMin, bounds.yMin);
//        selectionEnd = new Vector2Int(bounds.xMax, bounds.yMax);

//        Repaint();
//    }

//    private void FinalizeSelection()
//    {
//        // Process the selected area
//        Debug.Log($"Selected area: {selectionStart} to {selectionEnd}");

//        // Here you could:
//        // - Copy the selected area
//        // - Apply operations to the selection
//        // - etc.
//    }

//    private void DeleteSelectedArea()
//    {
//        if (layers[activeLayerIndex].isLocked) return;

//        Undo.RegisterCompleteObjectUndo(layers[activeLayerIndex].tilemap, "Delete Selection");

//        for (int x = Mathf.Min(selectionStart.x, selectionEnd.x);
//             x <= Mathf.Max(selectionStart.x, selectionEnd.x); x++)
//        {
//            for (int y = Mathf.Min(selectionStart.y, selectionEnd.y);
//                 y <= Mathf.Max(selectionStart.y, selectionEnd.y); y++)
//            {
//                layers[activeLayerIndex].SetTile(new Vector3Int(x, y, 0), null);
//            }
//        }

//        hasUnsavedChanges = true;
//        isSelecting = false;
//        Repaint();
//    }

//    private void FloodFill(Vector3Int startPos, TileBase newTile)
//    {
//        var activeLayer = layers[activeLayerIndex];
//        TileBase targetTile = activeLayer.GetTile(startPos);

//        if (targetTile == newTile) return; // Already the right tile

//        Undo.RegisterCompleteObjectUndo(activeLayer.tilemap, "Flood Fill");

//        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
//        Stack<Vector3Int> stack = new Stack<Vector3Int>();
//        stack.Push(startPos);

//        while (stack.Count > 0)
//        {
//            Vector3Int pos = stack.Pop();

//            if (visited.Contains(pos)) continue;
//            if (activeLayer.GetTile(pos) != targetTile) continue;

//            activeLayer.SetTile(pos, newTile);
//            visited.Add(pos);

//            // Check neighbors
//            Vector3Int[] neighbors = {
//                new Vector3Int(pos.x + 1, pos.y, 0),
//                new Vector3Int(pos.x - 1, pos.y, 0),
//                new Vector3Int(pos.x, pos.y + 1, 0),
//                new Vector3Int(pos.x, pos.y - 1, 0)
//            };

//            foreach (var neighbor in neighbors)
//            {
//                if (!visited.Contains(neighbor) && activeLayer.GetTile(neighbor) == targetTile)
//                {
//                    stack.Push(neighbor);
//                }
//            }
//        }

//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void DrawLine(Vector2Int start, Vector2Int end)
//    {
//        var activeLayer = layers[activeLayerIndex];
//        Undo.RegisterCompleteObjectUndo(activeLayer.tilemap, "Draw Line");

//        // Bresenham's line algorithm
//        int x = start.x;
//        int y = start.y;
//        int dx = Mathf.Abs(end.x - start.x);
//        int dy = Mathf.Abs(end.y - start.y);
//        int sx = start.x < end.x ? 1 : -1;
//        int sy = start.y < end.y ? 1 : -1;
//        int err = dx - dy;

//        while (true)
//        {
//            activeLayer.SetTile(new Vector3Int(x, y, 0), selectedTile);

//            if (x == end.x && y == end.y) break;

//            int e2 = 2 * err;
//            if (e2 > -dy)
//            {
//                err -= dy;
//                x += sx;
//            }
//            if (e2 < dx)
//            {
//                err += dx;
//                y += sy;
//            }
//        }

//        hasUnsavedChanges = true;
//        Repaint();
//    }

//    private void ExportEventToJson()
//    {
//        if (selectedEvent == null) return;

//        string json = JsonUtility.ToJson(selectedEvent, true);
//        string path = EditorUtility.SaveFilePanel(
//            "Export Event as JSON",
//            "",
//            selectedEvent.eventName + ".json",
//            "json");

//        if (!string.IsNullOrEmpty(path))
//        {
//            System.IO.File.WriteAllText(path, json);
//            Debug.Log($"Event exported to: {path}");
//        }
//    }

//    private void ImportEventFromJson()
//    {
//        string path = EditorUtility.OpenFilePanel(
//            "Import Event from JSON",
//            "",
//            "json");

//        if (string.IsNullOrEmpty(path)) return;

//        string json = System.IO.File.ReadAllText(path);
//        MapEvent importedEvent = JsonUtility.FromJson<MapEvent>(json);

//        if (importedEvent != null)
//        {
//            importedEvent.id = Guid.NewGuid().ToString(); // Generate new ID
//            layers[activeLayerIndex].events.Add(importedEvent);
//            SelectEvent(importedEvent);
//            hasUnsavedChanges = true;
//            Repaint();
//        }
//    }

//    private void ExportTilemapData()
//    {
//        if (currentMapData == null) return;

//        string path = EditorUtility.SaveFilePanel(
//            "Export Tilemap Data",
//            "",
//            currentMapData.mapName + "_data.json",
//            "json");

//        if (string.IsNullOrEmpty(path)) return;

//        // Create export data structure
//        var exportData = new
//        {
//            mapName = currentMapData.mapName,
//            mapSize = currentMapData.mapSize,
//            layers = layers.Select(l => new
//            {
//                name = l.layerName,
//                tiles = GetTileDataFromLayer(l)
//            }).ToArray()
//        };

//        string json = JsonUtility.ToJson(exportData, true);
//        System.IO.File.WriteAllText(path, json);
//        Debug.Log($"Tilemap data exported to: {path}");
//    }

//    private Dictionary<Vector3Int, string> GetTileDataFromLayer(TilemapLayerData layer)
//    {
//        var tileData = new Dictionary<Vector3Int, string>();

//        if (layer.tilemap != null)
//        {
//            BoundsInt bounds = layer.tilemap.cellBounds;
//            for (int x = bounds.xMin; x < bounds.xMax; x++)
//            {
//                for (int y = bounds.yMin; y < bounds.yMax; y++)
//                {
//                    Vector3Int pos = new Vector3Int(x, y, 0);
//                    TileBase tile = layer.tilemap.GetTile(pos);
//                    if (tile != null)
//                    {
//                        tileData[pos] = tile.name;
//                    }
//                }
//            }
//        }

//        return tileData;
//    }

//    private void ImportTileset()
//    {
//        string path = EditorUtility.OpenFilePanel(
//            "Import Tileset",
//            "",
//            "png,jpg,tif");

//        if (string.IsNullOrEmpty(path)) return;

//        // Create tiles from texture
//        Debug.Log($"Import tileset from: {path}");
//        // Note: You'll need to implement texture slicing and tile creation
//    }

//    private void AddRecentMapsToMenu(GenericMenu menu)
//    {
//        // Get recent maps from EditorPrefs
//        string recentMapsJson = EditorPrefs.GetString("RPGToolkit_RecentMaps", "[]");
//        string[] recentMaps = JsonUtility.FromJson<string[]>(recentMapsJson) ?? new string[0];

//        foreach (string mapPath in recentMaps.Take(5))
//        {
//            if (System.IO.File.Exists(mapPath))
//            {
//                string mapName = System.IO.Path.GetFileNameWithoutExtension(mapPath);
//                menu.AddItem(new GUIContent("Recent Maps/" + mapName), false,
//                    () => LoadMapFromPath(mapPath));
//            }
//        }

//        if (recentMaps.Length == 0)
//        {
//            menu.AddDisabledItem(new GUIContent("Recent Maps/No recent maps"));
//        }
//    }

//    private void LoadMapFromPath(string path)
//    {
//        MapData mapData = AssetDatabase.LoadAssetAtPath<MapData>(path);
//        if (mapData != null)
//        {
//            LoadMapData(mapData);
//        }
//    }

//    private void DrawTilemapBoundsGizmo()
//    {
//        // Draw bounds in scene view
//        if (currentMapData == null) return;

//        Handles.color = Color.green;
//        Vector3 center = new Vector3(0, 0, 0);
//        Vector3 size = new Vector3(currentMapData.mapSize.x, 0, currentMapData.mapSize.y);
//        Handles.DrawWireCube(center, size);
//    }

//    private void HandleSceneViewInput()
//    {
//        // Handle scene view specific input
//        Event e = Event.current;

//        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F)
//        {
//            if (selectedEvent != null)
//            {
//                SceneView.lastActiveSceneView.Frame(
//                    new Bounds(
//                        new Vector3(selectedEvent.position.x + 0.5f, 0, selectedEvent.position.y + 0.5f),
//                        Vector3.one * 5
//                    ),
//                    false
//                );
//                e.Use();
//            }
//        }
//    }
//    #endregion
//}

//// Supporting classes for event actions
//[CreateAssetMenu(menuName = "RPGToolkit/Events/NPCData")]
//public class NPCData : ScriptableObject
//{
//    public string npcName = "NPC";
//    public Sprite sprite;
//    public MovementPattern movement = MovementPattern.Stationary;
//    public List<Vector3Int> patrolPoints = new List<Vector3Int>();
//    public float moveSpeed = 2f;

//    public enum MovementPattern { Stationary, Patrol, Wander, Follow }
//}

//[CreateAssetMenu(menuName = "RPGToolkit/Events/WarpData")]
//public class WarpData : ScriptableObject
//{
//    public string targetMap = "";
//    public Vector3Int targetPosition = Vector3Int.zero;
//    public Direction facingDirection = Direction.Down;
//    public FadeType fadeType = FadeType.FadeOutIn;

//    public enum FadeType { None, FadeOutIn, CrossFade, Instant }
//}

//[CreateAssetMenu(menuName = "RPGToolkit/Events/ShopData")]
//public class ShopData : ScriptableObject
//{
//    public string shopName = "Shop";
//    public string welcomeMessage = "Welcome!";
//    public List<Item> itemsForSale = new List<Item>();
//    public float buyMultiplier = 1.0f;
//    public float sellMultiplier = 0.5f;
//}

//// Placeholder classes
//[CreateAssetMenu(menuName = "RPGToolkit/Battle/Encounter")]
//public class BattleEncounter : ScriptableObject { }

//[CreateAssetMenu(menuName = "RPGToolkit/Items/Item")]
//public class Item : ScriptableObject { }