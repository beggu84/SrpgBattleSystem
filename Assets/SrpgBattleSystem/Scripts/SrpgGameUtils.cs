using System;

public class SrpgGameUtils
{
    static public int CalcSimpleDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }

    static public int CalcSimpleDistance(SrpgUnit unit1, SrpgUnit unit2)
    {
        return CalcSimpleDistance(unit1.x, unit1.y, unit2.x, unit2.y);
    }
}
