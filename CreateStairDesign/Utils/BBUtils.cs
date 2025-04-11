using Autodesk.Revit.DB;

namespace CreateStair.Utils;

public static class BBUtils
{

    public static bool IsRectangleOverlap(XYZ point1, XYZ point2, XYZ point3, XYZ point4)
    {
        if (Math.Abs(point1.X - point2.X) < 1E-9 || Math.Abs(point1.Y - point2.Y) < 1E-9 ||
            Math.Abs(point3.X - point4.X) < 1E-9 || Math.Abs(point3.Y - point4.Y) < 1E-9)
            return false;

        if (point1.X > point4.X || point3.X > point2.X)
            return false;

        if (point1.Y > point4.Y || point2.Y < point3.Y)
            return false;

        return true;
    }
}