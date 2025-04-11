
//using Autodesk.Revit.DB;

using Autodesk.Revit.DB;

namespace CreateStair.Utils
{
    public static class XyzUtils
    {

        public static bool AreVectorsInSameDirection(double[] vector1, double[] vector2)
        {
            // Check if vectors have the same length
            if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException("Vectors must have the same length.");
            }

            // Calculate the dot product
            double dotProduct = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
            }

            // Calculate the magnitudes
            double magnitude1 = CalculateMagnitude(vector1);
            double magnitude2 = CalculateMagnitude(vector2);

            // Check if vectors are in the same direction
            return Math.Abs(dotProduct - magnitude1 * magnitude2) < 1e-9;
        }

        public static double CalculateMagnitude(double[] vector)
        {
            double sumOfSquares = 0;
            foreach (var component in vector)
            {
                sumOfSquares += component * component;
            }

            return Math.Sqrt(sumOfSquares);
        }



        public static double[] CreateVector(XYZ point1, XYZ point2)
        {
            double[] vector = { point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z };

            return vector;
        }

        public static bool IsParallel(this XYZ p, XYZ q, double tolerance = 1E-09) => p.CrossProduct(q).GetLength() < tolerance;

        public static XYZ ProjectOnto(this XYZ p, BPlane plane)
        {
            XYZ source = p - plane.Origin;
            double num = plane.Normal.DotProduct(source);
            return p - num * plane.Normal;
        }


        public static BPlane CreateBPlane(XYZ direction, XYZ origin)
        {
            var bPlane = BPlane.CreateByNormalAndOrigin(direction, origin);

            return bPlane;

        }
    }
}
