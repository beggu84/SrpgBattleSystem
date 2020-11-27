using UnityEngine;

public abstract class SrpgCell : MonoBehaviour
{
    public int x { get; set; }
    public int y { get; set; }

    public abstract Vector3 GetSize();
    public abstract void SetZIndex(int zIndex);

    public abstract void SetStateToMovable();
    public abstract void SetStateToAttackable();
}
