using System;

public class SrpgNode : IEquatable<SrpgNode>
{
    public int x { get; private set; }
    public int y { get; private set; }

    public SrpgNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(SrpgNode other)
    {
        if (other == null) return false;
        return (x == other.x && y == other.y);
    }
}
