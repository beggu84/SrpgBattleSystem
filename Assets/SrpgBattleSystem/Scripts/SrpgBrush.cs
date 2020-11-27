using UnityEngine;

public class SrpgBrush : MonoBehaviour
{
    private SrpgMap _currMap = null;

    private SpriteRenderer[,] _spriteObjects = null;
    public SpriteRenderer[,] spriteObjects
    {
        get { return _spriteObjects; }
    }

    public int brushHeight { get; set; }
    public int brushWidth { get; set; }
    private bool _visible { get; set; }

    private int _zIndexStart = 10000; // enough big int

    public void SetCurrentMap(SrpgMap newMap)
    {
        SrpgMap prevMap = _currMap;
        _currMap = newMap;

        if(prevMap != _currMap)
            RemoveSpriteObjects();
    }

    private void RemoveSpriteObjects()
    {
        if (_spriteObjects == null)
            return;

        foreach (SpriteRenderer sprtRndr in _spriteObjects)
        {
            if(sprtRndr != null)
                DestroyImmediate(sprtRndr.gameObject);
        }
    }

    public void ShowHide(bool show)
    {
        if (_spriteObjects == null)
            return;

        if (_visible == show)
            return;
        _visible = show;

        foreach (SpriteRenderer sprtRndr in _spriteObjects)
        {
            if (sprtRndr != null)
                sprtRndr.gameObject.SetActive(show);
        }
    }

    public void ResetSprites(int xMin, int yMin, int xMax, int yMax, Sprite[,] sprites)
    {
        RemoveSpriteObjects();

        brushWidth = xMax - xMin + 1;
        brushHeight = yMax - yMin + 1;
        _spriteObjects = new SpriteRenderer[brushWidth, brushHeight];

        float brushPixelHeight = _currMap.tileHeight * (yMax - yMin);
        for (int sprtY = yMin; sprtY <= yMax; sprtY++)
        {
            for(int sprtX = xMin; sprtX <= xMax; sprtX++)
            {
                Sprite tileSprt = sprites[sprtX, sprtY];
                if (tileSprt == null)
                    continue;

                int brushX = sprtX - xMin;
                int brushY = sprtY - yMin;

                GameObject sprtObj = new GameObject(string.Format("({0}, {1})", sprtX, sprtY));
                sprtObj.transform.parent = transform;

                float offsetX = (_currMap.tileWidth * brushX) + (_currMap.tileWidth / 2);
                float offsetY = brushPixelHeight - _currMap.tileHeight * brushY;
                Vector3 origin = transform.position;
                sprtObj.transform.position = origin + new Vector3(offsetX, offsetY);

                SpriteRenderer sprtRndr = sprtObj.AddComponent<SpriteRenderer>();
                sprtRndr.sortingOrder = _zIndexStart + sprtY;
                sprtRndr.color = new Color(1, 1, 1, 0.5f);
                sprtRndr.sprite = tileSprt;

                if(sprtRndr.sprite != null)
                {
                    float sprtScale = _currMap.tileWidth / sprtRndr.bounds.size.x;
                    sprtRndr.transform.localScale = new Vector3(sprtScale, sprtScale, 1);
                }

                _spriteObjects[brushX, brushY] = sprtRndr;
            }
        }
    }

    public void Relocate(Vector3 worldPos)
    {
        Vector3 snappedPos = _currMap.ConvertToSnappedPosition(worldPos);
        transform.position = snappedPos;
    }
}
