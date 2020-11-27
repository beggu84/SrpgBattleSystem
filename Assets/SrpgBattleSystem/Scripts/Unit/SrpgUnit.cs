using System;
using UnityEngine;

public abstract class SrpgUnit : MonoBehaviour
{
    public int x { get; set; }
    public int y { get; set; }

    public int boundWidth { get; set; }
    public int boundHeight { get; set; }

    public float offsetX { get; set; }
    public float offsetY { get; set; }

    public bool player { get; set; }

    public abstract Vector3 GetSize();
    public abstract Vector3 GetCenter();
    public abstract void SetZIndex(int zIndex);

    public virtual void Scale(float unitScale)
    {
        transform.localScale = new Vector3(unitScale, unitScale, 1);
    }

    public abstract void Enliven();
    public abstract bool IsEnd();

    public delegate bool MouseEventHandler(object sender, EventArgs e);
    public event MouseEventHandler clicked;

    void OnMouseDown()
    {
        if (clicked != null)
            clicked.Invoke(this, new EventArgs());
    }

    public abstract void BeginIdleAnimation();
    public abstract void BeginMoveAnimation(SrpgNode startNode, SrpgNode destNode);
}
