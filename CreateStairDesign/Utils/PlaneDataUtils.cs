using Autodesk.Revit.DB;

namespace CreateStair.Utils
{
    public class PlaneDataUtils
    {
        public static BPlane CreateByThreePoints(XYZ p1, XYZ p2, XYZ p3)
        {
            var v1 = p1 - p2;
            var v2 = p2 - p3;
            return new BPlane(v1.CrossProduct(v2).Normalize(), p1);
        }
    }
}
