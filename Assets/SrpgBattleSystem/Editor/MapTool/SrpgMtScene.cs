using UnityEngine;
using UnityEditor;

public class SrpgMtScene
{
    private static SrpgMtScene _instance = null;
    public static SrpgMtScene Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SrpgMtScene();
            return _instance;
        }
    }

    //private SrpgCanvas _canvas = null;
    private SrpgBrush _brush = null;
    private SrpgMap _currMap = null;
    private SrpgLayer _currLayer = null;

    private int iconWidth = 35;

    public void OnSceneGUI(SceneView sceneView, SrpgCanvas canvas, SrpgBrush brush,
        SrpgMap map, SrpgLayer layer)
    {
        if (canvas == null || brush == null || map == null)
            return;
        //_canvas = canvas;
        _brush = brush;
        _currMap = map;
        _currLayer = layer;

        _brush.ShowHide(SrpgMapTool.toolMode == SrpgMapTool.ToolMode.Edit &&
            SrpgMapTool.editMenu == SrpgMapTool.EditMenu.Brush);

        if (SrpgMapTool.toolMode != SrpgMapTool.ToolMode.Edit)
            return;

        DrawButtons();

        DoMouseEvent();
    }

    private void DrawButtons()
    {
        Handles.BeginGUI();

        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

        for (int i = 0; i < SrpgMapTool.config.sceneMenuIcons.Count; i++)
        {
            GUIContent buttonContent = new GUIContent(SrpgMapTool.config.sceneMenuIcons[i]);
            Rect toggleRect = new Rect(10, i * iconWidth + 10, iconWidth, iconWidth);
            bool pressed = (SrpgMapTool.editMenu == (SrpgMapTool.EditMenu)i);
            if (GUI.Toggle(toggleRect, pressed, buttonContent, GUI.skin.button))
                SrpgMapTool.editMenu = (SrpgMapTool.EditMenu)i;
        }

        Handles.EndGUI();
    }

    private void DoMouseEvent()
    {
        Event e = Event.current;
        if (e == null || !e.isMouse || e.button != 0)
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            return;

        switch(SrpgMapTool.editMenu)
        {
            case SrpgMapTool.EditMenu.Brush:
                DoBrushEvent(hit.point);
                break;

            case SrpgMapTool.EditMenu.Paint:
                DoPaintEvent(hit.point);
                break;

            case SrpgMapTool.EditMenu.Erase:
                DoEraseEvent(hit.point);
                break;

            //case SrpgMapTool.EditMenu.Select:
            //    DoSelectEvent(hit.point);
            //    break;
        }
    }

    private void DoBrushEvent(Vector3 hitPoint)
    {
        if (_brush.spriteObjects == null)
            return;

        Event e = Event.current;
        if (e.type == EventType.MouseMove)
        {
            _brush.Relocate(hitPoint);
            e.Use();
        }
        else if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
        {
            int minMapX = 0, minMapY = 0;
            _currMap.WorldToGrid(hitPoint, out minMapX, out minMapY);

            for (int brushY = 0; brushY < _brush.brushHeight; brushY++)
            {
                for (int brushX = 0; brushX < _brush.brushWidth; brushX++)
                {
                    int mapX = minMapX + brushX;
                    int mapY = minMapY + _brush.brushHeight - 1 - brushY;
                    SpriteRenderer sprtRndr = _brush.spriteObjects[brushX, brushY];
                    if (sprtRndr == null)
                        continue;
                    Sprite tileSprt = sprtRndr.sprite;

                    CreateTileIntoMap(mapX, mapY, tileSprt);
                }
            }

            e.Use();
        }
    }

    private void DoPaintEvent(Vector3 hitPoint)
    {
        if (_brush.spriteObjects == null)
            return;

        Event e = Event.current;
        if (e.type != EventType.MouseDown)
            return;

        SpriteRenderer sprtRndr = _brush.spriteObjects[0, 0];
        if (sprtRndr == null)
            return;
        Sprite tileSprt = sprtRndr.sprite;

        for (int mapY = 0; mapY < _currMap.mapWidth; mapY++)
        {
            for (int mapX = 0; mapX < _currMap.mapHeight; mapX++)
                CreateTileIntoMap(mapX, mapY, tileSprt);
        }

        e.Use();
    }

    private void DoEraseEvent(Vector3 hitPoint)
    {
        Event e = Event.current;
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag)
            return;

        int mapX = 0, mapY = 0;
        _currMap.WorldToGrid(hitPoint, out mapX, out mapY);

        SrpgTile tile = _currMap.GetTile(_currLayer, mapX, mapY);
        if (tile != null)
        {
            Undo.DestroyObjectImmediate(tile.gameObject);
            e.Use();
        }
    }

    //private void DoSelectEvent(Vector3 hitPoint)
    //{
    //    Event e = Event.current;
    //    if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag)
    //        return;
    //}

    private void CreateTileIntoMap(int x, int y, Sprite sprite)
    {
        SrpgTile oldTile = _currMap.GetTile(_currLayer, x, y);
        if (oldTile != null)
            Undo.DestroyObjectImmediate(oldTile.gameObject);

        SrpgTile newTile = _currMap.CreateTileInto(_currLayer, sprite, x, y);
        Undo.RegisterCreatedObjectUndo(newTile.gameObject, newTile.name);
        EditorUtility.SetSelectedRenderState(newTile.GetComponent<Renderer>(), EditorSelectedRenderState.Hidden);
    }
}
