using System;
using UnityEngine;

public abstract class SrpgSingleImageUnit : SrpgUnit
{
    public virtual void Init(Sprite sprite, RuntimeAnimatorController animCtrl)
    {
        SpriteRenderer unitRndr = GetComponent<SpriteRenderer>();
        unitRndr.sprite = sprite;
        gameObject.AddComponent<BoxCollider2D>();
        Animator unitAnimator = GetComponent<Animator>();
        unitAnimator.runtimeAnimatorController = animCtrl;
    }

    public override Vector3 GetSize()
    {
        return GetComponent<SpriteRenderer>().bounds.size;
    }

    public override Vector3 GetCenter()
    {
        return GetComponent<SpriteRenderer>().bounds.center;
    }

    public override void SetZIndex(int zIndex)
    {
        GetComponent<SpriteRenderer>().sortingOrder = zIndex;
    }
}
