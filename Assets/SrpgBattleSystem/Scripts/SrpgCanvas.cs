using UnityEngine;

public class SrpgCanvas : MonoBehaviour
{
	private SrpgMap _currMap = null;

    public void SetCurrentMap(SrpgMap newMap)
    {
        _currMap = newMap;

        float mapTotalWidth = _currMap.mapWidth * _currMap.tileWidth;
        float mapTotalHeight = _currMap.mapHeight * _currMap.tileHeight;
        transform.position = new Vector3(mapTotalWidth/2, mapTotalHeight/2);
        transform.localScale = new Vector3(mapTotalWidth, mapTotalHeight, 1);
    }

    void OnDrawGizmos()
	{
		if(_currMap == null)
			return;

        //switch(_currMap.viewType)
        //{
        //	case SrpgMap.ViewType.ORTHOGONAL:
        //		Draw_Orthgonal();
        //		break;

        //	case SrpgMap.ViewType.ISO_DIAGONAL:
        //		Draw_IsoDiagonal();
        //		break;

        //	case SrpgMap.ViewType.ISO_STAGGERED:
        //		Draw_IsoStaggered();
        //		break;

        //	case SrpgMap.ViewType.HEX_HORIZONTAL:
        //		Draw_HexHorizontal();
        //		break;

        //	case SrpgMap.ViewType.HEX_VERTICAL:
        //		Draw_HexVertical();
        //		break;
        //}

        Draw_Orthgonal();

        // origin
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(Vector3.zero, 5);
    }

    private void Draw_Orthgonal()
	{
		float mapPixelWidth = _currMap.mapWidth * _currMap.tileWidth;
		float mapPixelHeight = _currMap.mapHeight * _currMap.tileHeight;

		Gizmos.color = Color.gray;
		if(_currMap.mapWidth < 2 || _currMap.mapHeight < 2)
		{
			Debug.Assert(false);
			return;
		}

		// vertical lines
		for(int i=1; i<_currMap.mapWidth; i++)
		{
			float x = _currMap.tileWidth * i;
			Gizmos.DrawLine(new Vector3(x, 0), new Vector3(x, mapPixelHeight));
        }

        // horizontal lines
        for (int i = 1; i < _currMap.mapHeight; i++)
        {
            float y = _currMap.tileHeight * i;
            Gizmos.DrawLine(new Vector3(0, y), new Vector3(mapPixelWidth, y));
        }

        // border
        Gizmos.color = Color.green;

        Vector3[] points =
        {
            Vector3.zero,
            new Vector3(mapPixelWidth, 0),
            new Vector3(mapPixelWidth, mapPixelHeight),
            new Vector3(0, mapPixelHeight)
        };

        for (int i = 0; i < points.Length; i++)
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
    }

    private void Draw_IsoDiagonal()
    {
        Gizmos.color = Color.white;
		// grid

        Gizmos.color = Color.green;
		// border line
    }

    private void Draw_IsoStaggered()
    {
        Gizmos.color = Color.white;
		// grid

        Gizmos.color = Color.green;
		// border line
    }

    private void Draw_HexHorizontal()
    {
        Gizmos.color = Color.white;
		// grid

        Gizmos.color = Color.green;
		// border line
    }

    private void Draw_HexVertical()
    {
        Gizmos.color = Color.white;
		// grid

        Gizmos.color = Color.green;
		// border line
    }
}
