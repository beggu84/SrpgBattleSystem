using System;
using UnityEngine;

public class SrpgColorCell : SrpgCell
{
    public override Vector3 GetSize()
    {
        return GetComponent<SpriteRenderer>().bounds.size;
    }

    public override void SetZIndex(int zIndex)
    {
        GetComponent<SpriteRenderer>().sortingOrder = zIndex;
    }

    public override void SetStateToMovable()
    {
        GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 0.5f);
    }
    public override void SetStateToAttackable()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
    }
}
