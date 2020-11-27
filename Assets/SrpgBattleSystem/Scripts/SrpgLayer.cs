using System;
using System.Collections.Generic;
using UnityEngine;

public class SrpgLayer : MonoBehaviour
{
    [SerializeField]
    private int _zIndexStart = 0;
    public int zIndexStart
    {
        get { return _zIndexStart; }
    }

    [SerializeField]
    private bool _touchable = false;
	public bool touchable
	{
		get { return _touchable; }
		set { _touchable = value; }
    }

    [SerializeField]
    private List<SrpgPropertyScheme> _propertySchemes = new List<SrpgPropertyScheme>();
    public List<SrpgPropertyScheme> propertySchemes
    {
        get { return _propertySchemes; }
    }

    // for editor
    [SerializeField]
    public long lastEditedTime = 0L;

    public SrpgTile GetTile(int x, int y)
    {
        SrpgTile[] tiles = GetComponentsInChildren<SrpgTile>();
        foreach (SrpgTile tile in tiles)
        {
            if (tile.x == x && tile.y == y)
                return tile;
        }

        return null;
    }

    public SrpgTile[] GetTiles()
    {
        return GetComponentsInChildren<SrpgTile>();
    }

    public void SetZIndex(int newZIndexStart, int mapHeight)
    {
        _zIndexStart = newZIndexStart;

        SrpgTile[] tiles = GetComponentsInChildren<SrpgTile>();
        foreach (SrpgTile tile in tiles)
        {
            SpriteRenderer rndr = tile.GetComponent<SpriteRenderer>();
            rndr.sortingOrder = SrpgMap.CalcZIndex(_zIndexStart, mapHeight, tile.y);
        }
    }
}
