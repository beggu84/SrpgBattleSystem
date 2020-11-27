using System;

public class SrpgAStarNode : SrpgNode, IEquatable<SrpgAStarNode>, IComparable<SrpgAStarNode>
{
    public int GCost { get; set; }
    public int HCost { get; set; }

    public SrpgAStarNode Parent { get; set; }

    public SrpgAStarNode(int x, int y)
        : base(x, y)
    {
    }

    public SrpgAStarNode(int x, int y, int gCost)
        : base(x, y)
    {
        GCost = gCost;
    }

    public int GetCostSum()
    {
        return GCost + HCost;
    }

    public bool Equals(SrpgAStarNode other)
    {
        return base.Equals(other);
    }

    public int CompareTo(SrpgAStarNode other)
    {
        if (GetCostSum() < other.GetCostSum())
        {
            return 1;
        }
        else if (GetCostSum() > other.GetCostSum())
        {
            return -1;
        }
        else
        {
            if (HCost < other.HCost)
                return 1;
            else if (HCost > other.HCost)
                return -1;
        }

        return 0;
    }
}
