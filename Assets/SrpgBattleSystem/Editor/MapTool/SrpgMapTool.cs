using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class SrpgMapTool : EditorWindow
{
    public static SrpgMtConfig config = null;

    public enum ToolMode
    {
        Generate = 0,
        Edit
    }
    public static ToolMode toolMode = ToolMode.Generate;

    public enum EditMenu
    {
        Brush = 0,
        Paint,
        Erase,
        Select,
        None
    }
    public static EditMenu editMenu = EditMenu.None;

    public static int layerZIndexGap = 5;

    private SrpgCanvas _canvas = null;
    private SrpgBrush _brush = null;
    private SrpgMap _currMap = null;

    private List<SrpgLayer> _sortedLayer = null;
    private SrpgLayer _currLayer = null;

    private Vector2 _scrollPos = Vector2.zero;

    //private SrpgMap.ViewType _genViewType = SrpgMap.ViewType.ORTHOGONAL;
    private int _genMapWidth = 20;
    private int _genMapHeight = 20;
    private int _genMapAltitude = 0;
    private int _genTileWidth = 128;
    private int _genTileHeight = 128;
    private string _genMapName = "";
        
    private bool _layerAdding = false;
    private string _genLayerName;
    private bool _genLayerTouchable = false;

    private struct Tileset
    {
        public Texture2D texture;
        public float spriteWidth;
        public float spriteHeight;
    }
    private Tileset _tileset;
    private Sprite[,] _sprites = null;
    private Color _tilesetBackColor = Color.gray;
        
    private struct SpriteSelection
    {
        public int dragSttY, dragSttX;
        public int dragEndY, dragEndX;
        public int yMin, xMin;
        public int yMax, xMax;
        public bool changed;

        public void Init()
        {
            dragSttX = dragSttY = dragEndX = dragEndY = -1;
            xMin = yMin = xMax = yMax = -1;
            changed = false;
        }
    }
    private SpriteSelection _spriteSelection;

    [MenuItem("Window/Srpg Battle System/Map Tool")]
    static void ShowMapEditor()
    {
		EditorWindow.GetWindow<SrpgMapTool>(false, "Map Tool");
        //editorWindow.titleContent.text = "Map Tool";
        //editorWindow.minSize = new Vector2 (400, 300);
    }

    void OnEnable()
    {
        //AssetDatabase.Refresh();
        //AssetPreview.SetPreviewTextureCacheSize(1000); // what?

        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;

        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
		EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;

        SrpgMtUtils.ShowUnityGrid(false);
        SrpgMtUtils.SetupGizmos(true);
        //SrpgMtUtils.AddLayer("SrpgMap");
        //SrpgMapUtility.AddTag();

        string[] assetGuids = AssetDatabase.FindAssets("SrpgMtConfigImpl");
        //config = ScriptableObject.CreateInstance<SrpgMtConfig>();
        config = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGuids[0]), typeof(SrpgMtConfig)) as SrpgMtConfig;

        SetupCanvas();
        SetupBrush();
        SetupMap();

        _layerAdding = false;
        _genLayerName = "";

        _spriteSelection.Init();

        //EditorSceneManager.MarkAllScenesDirty(); // what?
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        SrpgMtUtils.ShowUnityGrid(true);
        SrpgMtUtils.SetupGizmos(false);

        if (_canvas != null)
        {
            DestroyImmediate(_canvas.gameObject);
            _canvas = null;
        }

        if (_brush != null)
        {
            DestroyImmediate(_brush.gameObject);
            _brush = null;
        }

        if (_currMap != null)
        {
            _currMap.lastEditedTime = DateTime.Now.Ticks;
            _currMap = null;
        }

        if(_currLayer != null)
        {
            _currLayer.lastEditedTime = DateTime.Now.Ticks;
            _currLayer = null;
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

        SrpgMtScene.Instance.OnSceneGUI(sceneView, _canvas, _brush, _currMap, _currLayer);
    }

    void OnHierarchyChanged()
    {
        SetupCanvas();
        SetupBrush();
        SetupMap();

        Repaint();
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    private void SetupCanvas()
    {
        string objectName = "SrpgCanvas";

        GameObject canvasObj = GameObject.Find(objectName);
        if (canvasObj == null)
        {
            canvasObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            canvasObj.name = objectName;
            //canvasObj.hideFlags = HideFlags.HideInHierarchy;
            canvasObj.GetComponent<MeshRenderer>().enabled = false;
            _canvas = canvasObj.AddComponent<SrpgCanvas>();
        }
        else
        {
            _canvas = canvasObj.GetComponent<SrpgCanvas>();
        }

        if (_currMap != null)
            _canvas.SetCurrentMap(_currMap);
    }

    private void SetupBrush()
    {
        string objectName = "SrpgBrush";

        GameObject brushObj = GameObject.Find(objectName);
        if (brushObj == null)
        {
            brushObj = new GameObject(objectName);
            //brushObj.hideFlags = HideFlags.HideInHierarchy;
            _brush = brushObj.AddComponent<SrpgBrush>();
        }
        else
        {
            _brush = brushObj.GetComponent<SrpgBrush>();
        }

        if (_currMap != null)
            _brush.SetCurrentMap(_currMap);
    }

    private void SetupMap()
    {
        if (_currMap == null)
        {
            SrpgMap[] foundMaps = GameObject.FindObjectsOfType<SrpgMap>();
            if (foundMaps.Length > 0)
            {
                Array.Sort(foundMaps, (map1, map2) =>
                {
                    if (map1.lastEditedTime < map2.lastEditedTime)
                        return 1;
                    else if (map1.lastEditedTime > map2.lastEditedTime)
                        return -1;
                    else
                        return 0;
                });

                SetCurrentMap(foundMaps[0]);
                toolMode = ToolMode.Edit;
                editMenu = EditMenu.Brush;
            }
        }
        else
        {
            if (_currMap.gameObject.activeSelf)
                SetupLayer();
            else
                _currMap = null;
        }

        if(_currMap != null)
        {
            _currMap.transform.position = Vector3.zero;

            Camera mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            if (mainCamera != null)
            {
                float cameraX = (_currMap.mapWidth * _currMap.tileWidth) / 2;
                //float cameraY = (_currMap.mapHeight * _currMap.tileHeight + _currMap.altitude) / 2;
                float cameraY = (_currMap.mapHeight * _currMap.tileHeight) / 2;
                mainCamera.transform.position = new Vector3(cameraX, cameraY, -10);
                mainCamera.orthographicSize = Mathf.Max(cameraX, cameraY);
            }
        }
    }

    private void SetupLayer()
    {
        if (_currMap.transform.childCount == 0)
        {
            _currLayer = _currMap.CreateLayer("Ground", true);
            Undo.RegisterCreatedObjectUndo(_currLayer.gameObject, _currLayer.name);

            _sortedLayer = new List<SrpgLayer>();
            _sortedLayer.Add(_currLayer);
        }
        else
        {
            _sortedLayer = _currMap.GetSortedLayers();
            if (_currLayer == null)
                _currLayer = _currMap.GetLastestEditedLayer();
        }
    }

    private void SetCurrentMap(SrpgMap newMap)
    {
        _currMap = newMap;
        if(_currMap != null)
            SetupLayer();

        if (_canvas != null)
            _canvas.SetCurrentMap(newMap);

        if(_brush != null)
            _brush.SetCurrentMap(newMap);

        _spriteSelection.Init();
    }

    void OnGUI()
    {
        if (_currMap == null)
        {
            SrpgMap[] maps = GameObject.FindObjectsOfType<SrpgMap>();
            if (maps.Length == 0)
            {
                toolMode = ToolMode.Generate;
                editMenu = EditMenu.None;
            }
        }

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        {
            switch (toolMode)
            {
                case ToolMode.Generate:
                    GUI_GenerateMode();
                    break;
                case ToolMode.Edit:
                    GUI_EditMode();
                    break;
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void GUI_GenerateMode()
    {
        if (_currMap != null)
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Edit " + _currMap.gameObject.name))
                {
                    toolMode = ToolMode.Edit;
                    editMenu = EditMenu.Brush;
                }
            }
            EditorGUILayout.EndVertical();
        }

        GUI_Title("Generate map");

        //EditorGUILayout.BeginVertical("box");
        //{
        //	//EditorGUILayout.EnumPopup("View Type: ", _genViewType);
        //	//EditorGUILayout.EnumPopup("Render Start", _genRenderStart);
        //}
        //EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.LabelField("Map Size");
			_genMapWidth = EditorGUILayout.IntField("Width", _genMapWidth);
			if (_genMapWidth < 4)
				_genMapWidth = 4;
			_genMapHeight = EditorGUILayout.IntField("Height", _genMapHeight);
			if (_genMapHeight < 4)
				_genMapHeight = 4;
            _genMapAltitude = EditorGUILayout.IntField("Altitude", _genMapAltitude);
            if (_genMapAltitude < 0)
                _genMapAltitude = 0;
        }
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.LabelField("Tile Size");
			_genTileWidth = EditorGUILayout.IntField("Width", _genTileWidth);
			if (_genTileWidth < 16)
				_genTileWidth = 16;
			else if (_genTileWidth > 256)
				_genTileWidth = 256;
			_genTileHeight = EditorGUILayout.IntField("Height", _genTileHeight);
			if (_genTileHeight < 16)
				_genTileHeight = 16;
			else if (_genTileHeight > 256)
				_genTileHeight = 256;
		}
		EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        {
            _genMapName = EditorGUILayout.TextField("Name", _genMapName);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate"))
        {
            string mapName = _genMapName;
			if (mapName.Length == 0)
				mapName = "SrpgMap_" + UnityEngine.Random.Range(0, 1000);

            GameObject mapObj = new GameObject(mapName);
            mapObj.transform.position = Vector3.zero;
            //mapObj.layer = LayerMask.NameToLayer("SrpgMap");
            SetCurrentMap(mapObj.AddComponent<SrpgMap>());
            //_currMap.viewType = _genViewType;
            _currMap.mapWidth = _genMapWidth;
            _currMap.mapHeight = _genMapHeight;
            //_currMap.altitude = _genMapAltitude;
            _currMap.tileWidth = _genTileWidth;
            _currMap.tileHeight = _genTileHeight;

            toolMode = ToolMode.Edit;
            editMenu = EditMenu.Brush;

            _layerAdding = true;
            _genLayerName = "Ground";
        }
    }

    private void GUI_EditMode()
    {
        if (_currMap == null)
            return;

        EditorGUILayout.BeginHorizontal("box");
        {
            if (GUILayout.Button("Generate map"))
            {
                toolMode = ToolMode.Generate;
                editMenu = EditMenu.None;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUI_Title("Edit map: " + _currMap.gameObject.name);

        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.LabelField("Layers (z-index rank. name)");

            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            buttonStyle.padding = new RectOffset(2, 2, 1, 1);

            String[] toolbars = { "↑", "↓", "x" };

            for(int i = 0; i < _sortedLayer.Count; i++)
            {
                SrpgLayer layer = _sortedLayer[i];
                if(layer == null)
                {
                    Debug.LogWarning("layer is null");
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    Texture2D layerTex = layer.gameObject.activeSelf ?
                        config.layerVisibleIcon :
                        config.layerHiddenIcon;

                    if (GUILayout.Button(layerTex, buttonStyle, GUILayout.Width(20), GUILayout.Height(15)))
                        layer.gameObject.SetActive(!layer.gameObject.activeSelf);

                    string layerName = (i+1) + ". " + layer.gameObject.name;
                    if(EditorGUILayout.ToggleLeft(layerName, layer == _currLayer))
                        _currLayer = layer;

                    int clickedButton = GUILayout.Toolbar(-1, toolbars, GUILayout.Width(60));
					if (clickedButton == 0) // up
					{
                        _currMap.ZIndexUp(i);
                        SetupLayer();
					}
					else if (clickedButton == 1) // down
					{
                        _currMap.ZIndexDown(i);
                        SetupLayer();
                    }
                    if (clickedButton == 2) // remove
					{
						if (_sortedLayer.Count > 1)
						{
							_sortedLayer.RemoveAt(i);
							Undo.DestroyObjectImmediate(layer.gameObject);
						}
					}
                }
                EditorGUILayout.EndHorizontal();
            }

            if(!_layerAdding)
            {
                if (GUILayout.Button("Add Layer"))
                    _layerAdding = true;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Name", GUILayout.Width(40));
                    _genLayerName = EditorGUILayout.TextField(_genLayerName);

                    EditorGUILayout.LabelField("", GUILayout.Width(10)); // space

                    //_genLayerTouchable = GUILayout.Toggle(_genLayerTouchable, "Touchable", GUILayout.Width(80));

                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        string layerName = _genLayerName;
                        if (layerName.Length == 0)
                            layerName = "Layer";

                        _currLayer = _currMap.CreateLayer(layerName, _genLayerTouchable);
                        Undo.RegisterCreatedObjectUndo(_currLayer.gameObject, _currLayer.name);

                        _layerAdding = false;
                        _genLayerName = "";
                    }

                    if (GUILayout.Button("Cancel", GUILayout.Width(50)))
                    {
                        _layerAdding = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();

        Texture2D prevTex = _tileset.texture;
        Rect boxRect = EditorGUILayout.BeginVertical("box");
        {
            string tilesetName = (_tileset.texture == null ? "not selected" : _tileset.texture.name);
            _tileset.texture = (Texture2D)EditorGUILayout.ObjectField("Tileset: " + tilesetName, _tileset.texture, typeof(Texture2D), true, GUILayout.Height(40));

            if (_tileset.texture == null && _currMap.lastTileset != null)
                _tileset.texture = _currMap.lastTileset;

            Sprite firstSprite = null;
            if(_tileset.texture != null)
            {
                string tilesetPath = AssetDatabase.GetAssetPath(_tileset.texture.GetInstanceID());
                firstSprite = AssetDatabase.LoadAssetAtPath<Sprite>(tilesetPath);
                if(firstSprite != null)
                {
                    _tileset.spriteWidth = firstSprite.rect.width;
                    _tileset.spriteHeight = firstSprite.rect.height;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Texture Size: " + _tileset.texture.width + " X " + _tileset.texture.height, MessageType.Info);
                    EditorGUILayout.HelpBox("Sprite Size: " + _tileset.spriteWidth + " X " + _tileset.spriteHeight, MessageType.Info);
                    EditorGUILayout.EndHorizontal();

                    _currMap.lastTileset = _tileset.texture;

                    _tilesetBackColor = EditorGUILayout.ColorField("Background Color", _tilesetBackColor);
                }
            }
        }
        EditorGUILayout.EndVertical();

        if (_tileset.texture == null)
            return;

        // draw background and tileset
        float sideMargin = 5;
        float rightMargin = 15;
        float tilesetOriginX = sideMargin;
        float tilesetOriginY = boxRect.y + boxRect.height + sideMargin;

#if UNITY_EDITOR_OSX
        float scaledTilesetWidth = (Screen.width / 2) - (sideMargin + rightMargin);
#else
        float scaledTilesetWidth = Screen.width - (sideMargin + rightMargin);
#endif

        float tilesetScale = _tileset.texture.width / scaledTilesetWidth;
        float scaledTilesetHeight = _tileset.texture.height / tilesetScale;
        Rect tilesetRect = new Rect(tilesetOriginX, tilesetOriginY, scaledTilesetWidth, scaledTilesetHeight);

        EditorGUILayout.LabelField("", GUILayout.Height(scaledTilesetHeight), GUILayout.Width(scaledTilesetWidth-5)); // tileset place holder

        Texture2D backTex = SrpgMtUtils.MakeAlphaTexture(1, 1, _tilesetBackColor);
        GUI.DrawTexture(tilesetRect, backTex, ScaleMode.StretchToFill);
        GUI.DrawTexture(tilesetRect, _tileset.texture);

        // draw lines
        int tilesetWidth = (int)(_tileset.texture.width / _tileset.spriteWidth);
        int tilesetHeight = (int)(_tileset.texture.height / _tileset.spriteHeight);

        float scaledSpriteWidth = _tileset.spriteWidth / tilesetScale;
        float scaledSpriteHeight = _tileset.spriteHeight / tilesetScale;
        //float scaledSpriteWidth = scaledTilesetWidth / tilesetWidth;
        //float scaledSpriteHeight = scaledTilesetHeight / tilesetHeight;

        Handles.BeginGUI();
        {
            Handles.color = Color.white;

            for (int i = 1; i < tilesetWidth; i++)
            {
                float x = tilesetOriginX + scaledSpriteWidth * i;
                Handles.DrawLine(new Vector3(x, tilesetOriginY), new Vector3(x, tilesetOriginY + scaledTilesetHeight));
            }

            for (int i = 1; i < tilesetHeight; i++)
            {
                float y = tilesetOriginY + scaledSpriteHeight * i;
                Handles.DrawLine(new Vector3(tilesetOriginX, y), new Vector3(tilesetOriginX + scaledTilesetWidth, y));
            }
        }
        Handles.EndGUI();

        // fill _sprites
        if (_tileset.texture != prevTex)
        {
            _sprites = new Sprite[tilesetWidth, tilesetHeight];

            string tilesetPath = AssetDatabase.GetAssetPath(_tileset.texture.GetInstanceID());
            Sprite[] rawSprites = AssetDatabase.LoadAllAssetsAtPath(tilesetPath).OfType<Sprite>().ToArray();

            foreach (Sprite sprt in rawSprites)
            {
                float spriteCenterX = tilesetOriginX + sprt.rect.center.x / tilesetScale;
                float spriteCenterY = tilesetOriginY + (_tileset.texture.height - (sprt.rect.center.y)) / tilesetScale;
                int x = Mathf.FloorToInt((spriteCenterX - tilesetOriginX) / scaledSpriteWidth);
                int y = Mathf.FloorToInt((spriteCenterY - tilesetOriginY) / scaledSpriteHeight);
                _sprites[x, y] = sprt;
            }

            _spriteSelection.Init();
        }

        // select sprites
        Event e = Event.current;
        if (e.button == 0)
        {
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                Vector2 mousePos = e.mousePosition;

                foreach (Sprite sprt in _sprites)
                {
                    if (sprt == null)
                        continue;

                    float spriteX = tilesetOriginX + sprt.rect.x / tilesetScale;
                    float spriteY = tilesetOriginY + (_tileset.texture.height - (sprt.rect.y + sprt.rect.height)) / tilesetScale;

                    if (mousePos.x > spriteX && mousePos.x < spriteX + scaledSpriteWidth &&
                        mousePos.y > spriteY && mousePos.y < spriteY + scaledSpriteHeight)
                    {
                        int selX = Mathf.FloorToInt((mousePos.x - tilesetOriginX) / scaledSpriteWidth);
                        int selY = Mathf.FloorToInt((mousePos.y - tilesetOriginY) / scaledSpriteHeight);

                        if (e.type == EventType.MouseDown)
                        {
                            _spriteSelection.dragSttX = _spriteSelection.dragEndX = selX;
                            _spriteSelection.dragSttY = _spriteSelection.dragEndY = selY;
                            _spriteSelection.changed = true;
                        }
                        else if (e.type == EventType.MouseDrag)
                        {
                            _spriteSelection.dragEndX = selX;
                            _spriteSelection.dragEndY = selY;
                            _spriteSelection.changed = true;
                        }

                        _spriteSelection.xMin = Mathf.Min(_spriteSelection.dragSttX, _spriteSelection.dragEndX);
                        _spriteSelection.yMin = Mathf.Min(_spriteSelection.dragSttY, _spriteSelection.dragEndY);
                        _spriteSelection.xMax = Mathf.Max(_spriteSelection.dragSttX, _spriteSelection.dragEndX);
                        _spriteSelection.yMax = Mathf.Max(_spriteSelection.dragSttY, _spriteSelection.dragEndY);

                        e.Use();
                        break;
                    }
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (_spriteSelection.changed)
                {
                    _spriteSelection.changed = false;
                    _brush.ResetSprites(_spriteSelection.xMin, _spriteSelection.yMin, _spriteSelection.xMax, _spriteSelection.yMax, _sprites);
                    e.Use();
                }
            }
        }

        // draw selection
        if (_spriteSelection.dragSttY >= 0 && _spriteSelection.dragSttX >= 0 &&
            _spriteSelection.dragEndY >= 0 && _spriteSelection.dragEndX >= 0)
        {
            for (int y = _spriteSelection.yMin; y <= _spriteSelection.yMax; y++)
            {
                for (int x = _spriteSelection.xMin; x <= _spriteSelection.xMax; x++)
                {
                    float scaledRectX = tilesetOriginX + scaledSpriteWidth * x;
                    float scaledRectY = tilesetOriginY + scaledSpriteHeight * y;
                    Rect selectionRect = new Rect(scaledRectX, scaledRectY, scaledSpriteWidth, scaledSpriteHeight);
                    Texture2D selectionTex = SrpgMtUtils.MakeAlphaTexture(1, 1, new Color(0, 0.5f, 0.5f, 0.5f));
                    GUI.DrawTexture(selectionRect, selectionTex, ScaleMode.StretchToFill);
                }
            }
        }
    }

    private void GUI_Title(string title)
    {
        GUIStyle titleStyle = GUI.skin.GetStyle("Label");
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 20;
        EditorGUILayout.LabelField(title, titleStyle, GUILayout.Height(30));
    }
}
