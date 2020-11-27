using System;
using UnityEngine;

public class Example1Cell : SrpgColorCell
{
    public void SetEnemyStateToMovable()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
    }
}
