using Autodesk.Revit.DB;

namespace CreateStair.Utils
{
    public class BPlane
    {
        public XYZ Normal { get; set; }

        public XYZ Origin { get; set; }

        public XYZ XVec { get; set; }

        public XYZ YVec { get; set; }

        public BPlane()
        {
        }

        public BPlane(Plane plane)
        {
            this.Normal = plane.Normal;
            this.Origin = plane.Origin;
        }

        public BPlane(XYZ normal, XYZ origin)
        {
            this.Normal = normal.Normalize();
            this.Origin = origin;
        }

        public static BPlane CreateByNormalAndOrigin(XYZ normal, XYZ origin) => new BPlane(normal, origin);


        public static BPlane CreateByThreePoints(XYZ p1, XYZ p2, XYZ p3)
        {
            var v1 = p1 - p2;
            var v2 = p2 - p3;
            return new BPlane(v1.CrossProduct(v2).Normalize(), p1);
        }

        public Plane ToPlane() => Plane.CreateByNormalAndOrigin(this.Normal, this.Origin);
    }
}
