using System;
using System.Collections.Generic;
using UnityEngine;

public class SrpgMap : MonoBehaviour
{
    //public enum ViewType
    //{
    //    ORTHOGONAL = 0,
    //    ISO_DIAGONAL,
    //    ISO_STAGGERED,
    //    HEX_HORIZONTAL,
    //    HEX_VERTICAL
    //}
    //[SerializeField]
    //public ViewType viewType = ViewType.ORTHOGONAL;

    [SerializeField]
    private int _mapWidth = 20; // Unit: tile
	public int mapWidth
	{
		get { return _mapWidth; }
		set { _mapWidth = value; }
	}

    [SerializeField]
    private int _mapHeight = 20; // Unit: tile
    public int mapHeight
	{
		get { return _mapHeight; }
		set { _mapHeight = value; }
    }

    //[SerializeField]
    //private int _altitude = 0;
    //public int altitude
    //{
    //    get { return _altitude; }
    //    set { _altitude = value; }
    //}

    [SerializeField]
    private int _tileWidth = 128; // Unit: px
    public int tileWidth
	{
		get { return _tileWidth; }
		set { _tileWidth = value; }
	}

    [SerializeField]
    private int _tileHeight = 128; // Unit: px
    public int tileHeight
	{
		get { return _tileHeight; }
		set { _tileHeight = value; }
    }

    // for editor
    [SerializeField]
    private Texture2D _lastTileset = null;
    public Texture2D lastTileset
    {
        get { return _lastTileset; }
        set { _lastTileset = value; }
    }

    [SerializeField]
    public long lastEditedTime = 0L;

    public List<SrpgLayer> GetSortedLayers()
    {
        List<SrpgLayer> layers = new List<SrpgLayer>();
        for(int i=0; i<transform.childCount; i++)
            layers.Add(transform.GetChild(i).GetComponent<SrpgLayer>());

        layers.Sort((layer1, layer2) =>
        {
            if (layer1.zIndexStart < layer2.zIndexStart)
                return 1;
            else if (layer1.zIndexStart > layer2.zIndexStart)
                return -1;
            else
                return 0;
        });

        return layers;
    }

    public SrpgLayer GetLayer(string layerName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == layerName)
                return child.GetComponent<SrpgLayer>();
        }

        return null;
    }

    public SrpgLayer GetLastestEditedLayer()
    {
        if (transform.childCount == 0)
            return null;

        List<SrpgLayer> layers = new List<SrpgLayer>();
        for (int i = 0; i < transform.childCount; i++)
            layers.Add(transform.GetChild(i).GetComponent<SrpgLayer>());

        layers.Sort((layer1, layer2) =>
        {
            if (layer1.lastEditedTime < layer2.lastEditedTime)
                return 1;
            else if (layer1.lastEditedTime > layer2.lastEditedTime)
                return -1;
            else
                return 0;
        });

        return layers[0];
    }

    public SrpgLayer CreateLayer(string layerName, bool touchable = false, int zIndexStart = -1)
    {
        if(zIndexStart < 0)
        {
            // magic number 5!?
            List<SrpgLayer> layers = GetSortedLayers();
            if (layers.Count > 0)
                zIndexStart = layers[0].zIndexStart + _mapHeight + 5;
            else
                zIndexStart = 5;
        }

        GameObject layerObj = new GameObject(layerName);
        layerObj.transform.parent = transform;
        layerObj.transform.position = Vector3.zero;

        SrpgLayer newLayer = layerObj.AddComponent<SrpgLayer>();
        newLayer.touchable = touchable;
        newLayer.SetZIndex(zIndexStart, _mapHeight);

        return newLayer;
    }

    public void WorldToGrid(Vector3 worldPos, out int x, out int y)
    {
        Vector3 origin = transform.position;
        x = Mathf.FloorToInt((worldPos.x - origin.x) / (float)_tileWidth);
        y = Mathf.FloorToInt((worldPos.y - origin.y) / (float)_tileHeight);
    }

    public Vector3 ConvertToSnappedPosition(Vector3 worldPos)
    {
        int x = -1, y = -1;
        WorldToGrid(worldPos, out x, out y);

        return new Vector3(_tileWidth*x, _tileHeight*y);
    }

    public SrpgTile GetTile(SrpgLayer layer, int x, int y)
    {
        return layer.GetTile(x, y);
    }

    public SrpgTile CreateTileInto(SrpgLayer layer, Sprite tileSprt, int x, int y)
    {
        string tileName = string.Format("[{0}, {1}] {2}", x, y, tileSprt.name);
        GameObject tileObj = new GameObject(tileName);
        tileObj.transform.parent = layer.transform;

        SpriteRenderer tileRndr = tileObj.AddComponent<SpriteRenderer>();
        tileRndr.sprite = tileSprt;
        tileRndr.sortingOrder = CalcZIndex(layer, y);

		float tileScale = _tileWidth / tileRndr.bounds.size.x;
        tileObj.transform.localScale = new Vector3(tileScale, tileScale, 1);
        tileObj.transform.position = CalcTilePosition(x, y);

        if (layer.touchable)
            tileObj.AddComponent<BoxCollider2D>();

        SrpgTile tile = tileObj.AddComponent<SrpgTile>();
        tile.x = x;
        tile.y = y;

        return tile;
    }

    private int CalcZIndex(SrpgLayer layer, int y)
    {
        return CalcZIndex(layer.zIndexStart, _mapHeight, y);
    }

    static public int CalcZIndex(int zIndexStart, int mapHeight, int y)
    {
        return zIndexStart + ((mapHeight-1) - y);
    }

    public void ZIndexUp(int layerIndex)
    {
        if (layerIndex <= 0)
            return;

        List<SrpgLayer> layers = GetSortedLayers();
        SrpgLayer destLayer = layers[layerIndex-1];
        SrpgLayer targetLayer = layers[layerIndex];
        int destLayerZIndexStart = destLayer.zIndexStart;
        destLayer.SetZIndex(targetLayer.zIndexStart, _mapHeight);
        targetLayer.SetZIndex(destLayerZIndexStart, _mapHeight);
        layers[layerIndex-1] = targetLayer;
        layers[layerIndex] = destLayer;
    }

    public void ZIndexDown(int layerIndex)
    {
        if (layerIndex >= transform.childCount-1)
            return;

        List<SrpgLayer> layers = GetSortedLayers();
        SrpgLayer destLayer = layers[layerIndex+1];
        SrpgLayer targetLayer = layers[layerIndex];
        int destLayerZIndexStart = destLayer.zIndexStart;
        destLayer.SetZIndex(targetLayer.zIndexStart, _mapHeight);
        targetLayer.SetZIndex(destLayerZIndexStart, _mapHeight);
        layers[layerIndex+1] = targetLayer;
        layers[layerIndex] = destLayer;
    }

    public void LocateUnitFirst(SrpgLayer layer, SrpgUnit unit, int x, int y,
        float fitRatio = 1f, SrpgBattle.UnitFitMode scaleMode = SrpgBattle.UnitFitMode.Inside)
    {
        unit.boundWidth = _tileWidth;
        unit.boundHeight = _tileHeight;

        Vector3 unitSize = unit.GetSize();
        float unitScale = 0f;
        if (scaleMode == SrpgBattle.UnitFitMode.Inside)
        {
            float widthScale = _tileWidth * fitRatio / unitSize.x;
            float heightScale = _tileHeight * fitRatio / unitSize.y;

            if (widthScale >= 1f && heightScale >= 1f) // tile width >= unit width && tile height >= unit height
                unitScale = Mathf.Min(widthScale, heightScale);
            else if(widthScale < 1f && heightScale < 1f) // tile width < unit width && tile height < unit height
                unitScale = Mathf.Max(widthScale, heightScale);
            else if(widthScale >= 1f && heightScale < 1f) // tile width >= unit width && tile height < unit height
                unitScale = heightScale;
            else if (widthScale < 1f && heightScale >= 1f) // tile width < unit width && tile height >= unit height
                unitScale = widthScale;
        }
        else if (scaleMode == SrpgBattle.UnitFitMode.Width)
        {
            unitScale = _tileWidth * fitRatio / unitSize.x;
        }
        else if (scaleMode == SrpgBattle.UnitFitMode.Height)
        {
            unitScale = _tileHeight * fitRatio / unitSize.y;
        }

        unit.Scale(unitScale);

        Vector3 unitCenter = unit.GetCenter();
        unit.offsetX = _tileWidth / 2 + unitCenter.x;
        unit.offsetY = _tileHeight / 2 - unitCenter.y;

        LocateUnit(layer, unit, x, y);
    }

    private void LocateUnit(SrpgLayer layer, SrpgUnit unit, int x, int y)
    {
        unit.SetZIndex(CalcZIndex(layer, y));

        unit.gameObject.name = string.Format("[{0}, {1}] {2}", x, y, unit.gameObject.name);
        unit.x = x;
        unit.y = y;

        float unitX = _tileWidth * x + unit.offsetX;
        float unitY = _tileHeight * y + unit.offsetY;
        unit.transform.position = new Vector3(unitX, unitY);
    }

    public SrpgCell InstantiateCellIntoLayer(SrpgLayer layer, GameObject cellPrefab, int x, int y)
    {
        GameObject cellObj = Instantiate(cellPrefab, layer.transform);
        cellObj.name = string.Format("[{0}, {1}] {2}", x, y, cellObj.name);

        SrpgCell cell = cellObj.GetComponent<SrpgCell>();
        cell.x = x;
        cell.y = y;
        cell.SetZIndex(layer.zIndexStart);

        float cellScale = _tileWidth / cell.GetSize().x;
        cellObj.transform.localScale = new Vector3(cellScale, cellScale, 1);
        cellObj.transform.position = CalcTilePosition(x, y);

        return cell;
    }

    public Vector3 CalcTilePosition(int x, int y)
    {
        float offsetX = (_tileWidth * x) + (_tileWidth / 2);
        float offsetY = _tileHeight * y;
        Vector3 origin = transform.position;
        return origin + new Vector3(offsetX, offsetY);
    }

    public List<SrpgTile> GetAdjacentTiles(SrpgLayer layer, int x, int y)
    {
        List<SrpgNode> toFindNodes = new List<SrpgNode>();
        // depending on tile types such as rectangle, hexagon..
        if (x > 0)
            toFindNodes.Add(new SrpgNode(x - 1, y));
        if (x < _mapHeight - 1)
            toFindNodes.Add(new SrpgNode(x + 1, y));
        if (y > 0)
            toFindNodes.Add(new SrpgNode(x, y - 1));
        if (y < mapHeight - 1)
            toFindNodes.Add(new SrpgNode(x, y + 1));

        SrpgTile[] allTiles = layer.GetTiles();
        List<SrpgTile> adjacentTiles = new List<SrpgTile>();
        foreach (SrpgTile tile in allTiles)
        {
            if (toFindNodes.Contains(new SrpgNode(tile.x, tile.y)))
                adjacentTiles.Add(tile);
        }

        return adjacentTiles;
    }
}
