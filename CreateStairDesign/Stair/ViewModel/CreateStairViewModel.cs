using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using CreateStairDesign.Stair.View;
using BimSpeedUtils;
using CreateStairDesign.Stair.Models;
using Line = Autodesk.Revit.DB.Line;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;
using Autodesk.Revit.Creation;
using Document = Autodesk.Revit.DB.Document;
using System.Numerics;
using Amazon.S3.Model;
using Plane = Autodesk.Revit.DB.Plane;
using System.Security.Cryptography;

namespace CreateStairDesign.Stair.ViewModel
{
    public class CreateStairViewModel : ObservableObject
    {
        public UIDocument UiDocument { get; set; }

        private Document _doc { get; set; }

        public CreateStairView MainView { get; set; }

        private string path { get; set; }
        public string PathJson
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged(nameof(PathJson));
            }
        }
        private List<ElementId> _createdExtrusions = new List<ElementId>();
        public Double OffsetStair3 { get; set; }
        public RelayCommand OpenFileCmd { get; set; }
        public RelayCommand RunCmd { get; set; }

        public CreateStairViewModel(Document doc)
        {
            _doc = doc;
            OpenFileCmd = new RelayCommand(OpenFileJson);
            RunCmd = new RelayCommand(Run);
            
        }
        private void Run()
        {
            MainView.Close();
            var width = 1000.0.MmToFoot();
            var stairModel = new StairModel()
            {
                Step1 = new StepModel()
                {
                    Thickness = 100.0.MmToFoot(),
                    StartLevel = 0,
                    EndLevel = 2000.MmToFoot(),
                    StepNumber = 6,
                    Start = new XYZ(-2115.5824587448105.MmToFoot(), 0.MmToFoot(), -918.07689499975186.MmToFoot()),
                    End = new XYZ(-115.58245874481645.MmToFoot(), 0.MmToFoot(), 81.92310500025998.MmToFoot()),
                    StepWidth = width,
                },
                Step2 = new StepModel()
                   {
                    Thickness = 100.0.MmToFoot(),
                    StartLevel = 0,
                    EndLevel = 2000.MmToFoot(),
                    StepNumber = 4,
                    Start = new XYZ(1384.4175412551836.MmToFoot(), 0.MmToFoot(), 81.92310500025998.MmToFoot()),
                    End = new XYZ(2884.4175412551895.MmToFoot(), 0, -918.07689499975254.MmToFoot()),
                    StepWidth = width,
                   },
                   Step3 = new StepModel()
                 {
                    Thickness = 100.0.MmToFoot(),
                    StartLevel = 0,
                    EndLevel = 2000.MmToFoot(),
                    StepNumber = 4,
                    Start = new XYZ(0.MmToFoot(), -115.58245874481645.MmToFoot(), 81.92310500025998.MmToFoot()),
                    End = new XYZ(0.MmToFoot(), 1384.4175412551836.MmToFoot(), 1581.9231050002504.MmToFoot()),
                 },
                Landing = new LandingModel()
                {
                    Thickness = 300.0.MmToFoot(),
                    Width = 1000.0.MmToFoot(),
                }
            }; 
            stairModel.Landing.Start = stairModel.Step1.End;
            stairModel.Landing.End = stairModel.Step2.Start;
            stairModel.Landing.Length = Math.Abs(stairModel.Step1.End.X - stairModel.Step2.Start.X);
            stairModel.Step3.StepWidth = stairModel.Landing.Length;
            XYZ vectoStep3 = stairModel.Step1.End - stairModel.Step3.Start;
            OffsetStair3 = width;

            using (var tx = new Transaction(_doc, "Stair"))
            {
                tx.Start();
                CreateModelStair(stairModel.Step1);
                CreateModelStair(stairModel.Step2);
                CreateModelStair(stairModel.Step3, vectoStep3);
                CreateLanding(stairModel.Landing);

                _doc.Regenerate();
                CombinableElementArray combineArray = new CombinableElementArray();

                foreach (ElementId id in _createdExtrusions)
                {
                    Element e = _doc.GetElement(id);
                    if (e is CombinableElement combinable)
                    {
                        combineArray.Append(combinable);
                    }
                }
                _doc.CombineElements(combineArray);
             
                _doc.Regenerate();
                tx.Commit();
            }
        }
        #region test
        //private void CreateModelStair(StepModel Model)
        //{

        //    var BoundStair = CreateLineStair(Model);
        //    Model.Curves = BoundStair;
        //    CreatePathStair(Model);


        //    var familyDoc = _doc;

        //    FamilyItemFactory factory = familyDoc.FamilyCreate;

        //    // Tạo mặt phẳng sketch
        //    XYZ origin = XYZ.Zero;
        //    XYZ normal = XYZ.BasisY;
        //    Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
        //    SketchPlane sketchPlane = SketchPlane.Create(familyDoc, plane);

        //    // Chiều cao extrusion
        //    double extrusionHeight = Model.StepWidth;
        //    bool isSolid = true;

        //    CurveArrArray profile = new CurveArrArray();
        //    CurveArray curveArray = new CurveArray();

        //    foreach (var line in Model.Curves)
        //    {
        //        ModelCurve modelCurve = _doc.FamilyCreate.NewModelCurve(line, sketchPlane);
        //        curveArray.Append(line);

        //    }
        //    profile.Append(curveArray);

        //    Extrusion extrusion = _doc.FamilyCreate.NewExtrusion(
        //        true,             // true = khối đặc
        //        profile,
        //        sketchPlane,
        //        extrusionHeight     // chiều cao đùn lên
        //    );

        //    double offsetX = Model.StepWidth / 2.0;
        //    XYZ translationVector = new XYZ(offsetX, 0, 0);
        //    Curve movedCurve = Model.Path.CreateTransformed(Transform.CreateTranslation(translationVector));
        //    ModelCurve modelCurve2 = _doc.FamilyCreate.NewModelCurve(movedCurve, sketchPlane);

        //    //var combine = new CombinableElementArray();
        //    //combine.Append(e1);
        //    //combine.Append(e2);
        //    //AC.Document.CombineElements(combine);
        //}

        //private List<Line> CreateLineStair(StepModel Model)
        //{
        //    List<Line> lines = new List<Line>();
        //    List<XYZ> points = new List<XYZ>();

        //    var l = (Model.End.X - Model.Start.X) / (Model.StepNumber - 1); // chiều ngang
        //    var h = (Model.End.Z - Model.Start.Z) / Model.StepNumber;       // chiều cao

        //    double offsetZ = CalculateValueSinByAngle(48, Model.Thickness);

        //    // Điểm bắt đầu dưới góc trái
        //    XYZ PointStart = Model.Start - new XYZ(0, 0, offsetZ); // offset theo Z
        //    points.Add(PointStart);

        //    // Điểm đầu thang
        //    XYZ currentPoint = Model.Start;
        //    points.Add(currentPoint);

        //    for (int i = 0; i < Model.StepNumber; i++)
        //    {
        //        // Lên theo Z
        //        currentPoint = new XYZ(currentPoint.X, currentPoint.Y, currentPoint.Z + h);
        //        points.Add(currentPoint);

        //        // Tiến theo X
        //        currentPoint = new XYZ(currentPoint.X + l, currentPoint.Y, currentPoint.Z);
        //        points.Add(currentPoint);
        //    }

        //    XYZ lastPoint = points.Last();
        //    double offsetX = CalculateValueCosByAngle(48, Model.Thickness);
        //    XYZ PointEnd = lastPoint + new XYZ(offsetX, 0, 0); // dịch theo X
        //    points.Add(PointEnd);

        //    List<XYZ> sortedPoints = points; // không cần sắp xếp lại

        //    for (int i = 0; i < sortedPoints.Count - 1; i++)
        //    {
        //        lines.Add(Line.CreateBound(sortedPoints[i], sortedPoints[i + 1]));
        //    }
        //    lines.Add(Line.CreateBound(points.Last(), points.First()));

        //    return lines;
        //}
        #endregion
        private void CreateLanding(LandingModel Model)
        {
            var BoundStair = CreateLineLanding(Model);
            var familyDoc = _doc;
            FamilyItemFactory factory = familyDoc.FamilyCreate;


            // Mặt phẳng sketch
            XYZ origin = XYZ.Zero;
            XYZ normal = XYZ.BasisY; // vuông góc với mặt XZ

            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            SketchPlane sketchPlane = SketchPlane.Create(familyDoc, plane);

            // Chiều cao extrusion là độ rộng mặt bậc
            double extrusionHeight = Math.Abs(Model.Width);
            bool isSolid = true;

            // Dữ liệu đường viền
            CurveArrArray profile = new CurveArrArray();
            CurveArray curveArray = new CurveArray();
            List<ElementId> tempCurveIds = new List<ElementId>();

            foreach (var line in BoundStair)
            {
                ModelCurve modelCurve = _doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                tempCurveIds.Add(modelCurve.Id);
                curveArray.Append(line);
            }

            profile.Append(curveArray);


            Extrusion extrusion = _doc.FamilyCreate.NewExtrusion(
                isSolid,
                profile,
                sketchPlane,
                extrusionHeight
            );
            _createdExtrusions.Add(extrusion.Id);
            foreach (var id in tempCurveIds)
            {
                familyDoc.Delete(id);
            }

        }
        private List<Line> CreateLineLanding(LandingModel Model)
        {
            List<Line> lines = new List<Line>();
            List<XYZ> points = new List<XYZ>();

            points.Add(Model.Start);
            XYZ P2 = new XYZ(Model.Start.X , Model.Start.Y, Model.Start.Z - Model.Thickness);
            points.Add(P2);
            XYZ P3 = new XYZ(Model.Start.X + Model.Length, Model.Start.Y, Model.Start.Z - Model.Thickness);
            points.Add(P3);
            points.Add(Model.End);

            for (int i = 0; i < points.Count - 1; i++)
            {
                lines.Add(Line.CreateBound(points[i], points[i + 1]));
            }

            lines.Add(Line.CreateBound(points.Last(), points.First()));

            return lines;
        }
        private void CreateModelStair(StepModel Model, XYZ vector = null)
        {
            var BoundStair = CreateLineStair(Model);
            Model.Curves = BoundStair;
            Model.Path= CreatePathStair(Model);

            var familyDoc = _doc;
            FamilyItemFactory factory = familyDoc.FamilyCreate;

            // Xác định mặt phẳng vẽ theo hướng thang
            bool isXZPlane = Model.End.X != Model.Start.X && Model.End.Y == Model.Start.Y;
            bool isYZPlane = Model.End.Y != Model.Start.Y && Model.End.X == Model.Start.X;

            // Mặt phẳng sketch
            XYZ origin = XYZ.Zero;
            XYZ normal;

            if (isXZPlane)
            {
                normal = XYZ.BasisY; // vuông góc với mặt XZ
            }
            else if (isYZPlane)
            {
                normal = XYZ.BasisX; // vuông góc với mặt YZ
            }
            else
            {
                throw new InvalidOperationException("Không xác định được mặt phẳng sketch phù hợp.");
            }

            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            SketchPlane sketchPlane = SketchPlane.Create(familyDoc, plane);

            // Chiều cao extrusion là độ rộng mặt bậc
            double extrusionHeight = Math.Abs(Model.StepWidth);
            bool isSolid = true;

            // Dữ liệu đường viền
            CurveArrArray profile = new CurveArrArray();
            CurveArray curveArray = new CurveArray();
            List<ElementId> tempCurveIds = new List<ElementId>();

            foreach (var line in Model.Curves)
            {
                ModelCurve modelCurve = _doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                tempCurveIds.Add(modelCurve.Id);
                curveArray.Append(line);
            }

            profile.Append(curveArray);

           
            Extrusion extrusion = _doc.FamilyCreate.NewExtrusion(
                isSolid,
                profile,
                sketchPlane,
                extrusionHeight
            );
            _createdExtrusions.Add(extrusion.Id);


            XYZ translationVector;

            if (isXZPlane)
            {
                translationVector = new XYZ(0, Model.StepWidth / 2.0, 0); // dịch theo Y
            }
            else
            {
                translationVector = new XYZ(Model.StepWidth / 2.0, 0, 0); // dịch theo X
            }
            if (vector != null)
            {
                translationVector = new XYZ(
                    translationVector.X + vector.X, // chỉ cộng trục X
                    translationVector.Y,
                    translationVector.Z
                );
            }
            // 2. Dịch curve (Model.Path)
            Curve movedPath = Model.Path.CreateTransformed(Transform.CreateTranslation(translationVector));

            // 3. Tạo SketchPlane mới đi qua movedPath
            XYZ midPoint = (movedPath.GetEndPoint(0) + movedPath.GetEndPoint(1)) / 2;
            Plane movedPlane = Plane.CreateByNormalAndOrigin(normal, midPoint);
            SketchPlane movedSketchPlane = SketchPlane.Create(_doc, movedPlane);

            // 4. Tạo ModelCurve từ đường đã dịch, trên mặt phẳng đã dịch
            ModelCurve modelCurve2 = _doc.FamilyCreate.NewModelCurve(movedPath, movedSketchPlane);
            if (vector != null)
            {
                    vector = vector + new XYZ(0, OffsetStair3, 0);
                    ElementTransformUtils.MoveElement(_doc, extrusion.Id, vector);
                    ElementTransformUtils.MoveElement(_doc, modelCurve2.Id, vector);
          
            }
            foreach (var id in tempCurveIds)
            {
                familyDoc.Delete(id);
            }
        }
        private List<Line> CreateLineStair(StepModel Model)
        {
            List<Line> lines = new List<Line>();
            List<XYZ> points = new List<XYZ>();

            bool isXZPlane = Model.End.X != Model.Start.X && Model.End.Y == Model.Start.Y;
            bool isYZPlane = Model.End.Y != Model.Start.Y && Model.End.X == Model.Start.X;

            int stepCount = Model.StepNumber;

            XYZ pointStart, currentPoint;

            if (isXZPlane)
            {
                // mặt đứng theo XZ
                double l = (Model.End.X - Model.Start.X) / (Model.StepNumber - 1);
                double h = (Model.End.Z - Model.Start.Z) / Model.StepNumber;
                if (h > 0)
                {
                    var alpha = CalculateAngleByTan(h, l);
                    double offsetVertical = CalculateValueCosByAngle(alpha, Model.Thickness);
                    double offsetHorizontal = CalculateValueSinByAngle(alpha, Model.Thickness);

                    pointStart = Model.Start - new XYZ(0, 0, offsetVertical);
                    points.Add(pointStart);
                    currentPoint = Model.Start;
                    points.Add(currentPoint);

                    for (int i = 0; i < stepCount; i++)
                    {
                        currentPoint = new XYZ(currentPoint.X, currentPoint.Y, currentPoint.Z + h);
                        points.Add(currentPoint);

                        currentPoint = new XYZ(currentPoint.X + l, currentPoint.Y, currentPoint.Z);
                        points.Add(currentPoint);
                    }

                    XYZ lastPoint = points.Last();
                    XYZ pointEnd = lastPoint + new XYZ(offsetHorizontal, 0, 0);

                    points.Add(pointEnd);
                }
                else
                {
                    var alpha = CalculateAngleByTan(h, l);
                    double offsetVertical = CalculateValueCosByAngle(alpha, Model.Thickness);
                    double offsetHorizontal = CalculateValueSinByAngle(alpha, Model.Thickness);
                    Model.Start = Model.Start - new XYZ(l, 0, 0);
                    Model.End = Model.End - new XYZ(l, 0, 0);
                    pointStart = Model.Start + new XYZ(offsetHorizontal, 0, 0);
                    points.Add(pointStart);
                    currentPoint = Model.Start;
                    points.Add(currentPoint);

                    for (int i = 0; i < stepCount; i++)
                    {
                        currentPoint = new XYZ(currentPoint.X + l, currentPoint.Y, currentPoint.Z);
                        points.Add(currentPoint);

                        currentPoint = new XYZ(currentPoint.X, currentPoint.Y, currentPoint.Z + h);
                        points.Add(currentPoint);  
                    }

                    XYZ lastPoint = points.Last();
                    XYZ pointEnd = lastPoint + new XYZ(0, 0, -offsetVertical);

                    points.Add(pointEnd);
                }

            }
            else if (isYZPlane)
            {
                // mặt đứng theo YZ
                double l = (Model.End.Y - Model.Start.Y) / (Model.StepNumber - 1);
                double h = (Model.End.Z - Model.Start.Z) / Model.StepNumber;

                var alpha = CalculateAngleByTan(h, l);
                double offsetVertical = CalculateValueCosByAngle(alpha, Model.Thickness);
                double offsetHorizontal = CalculateValueSinByAngle(alpha, Model.Thickness);
                pointStart = Model.Start - new XYZ(0, 0, offsetVertical);
                points.Add(pointStart);
                currentPoint = Model.Start;
                points.Add(currentPoint);

                for (int i = 0; i < stepCount; i++)
                {
                    currentPoint = new XYZ(currentPoint.X, currentPoint.Y, currentPoint.Z + h);
                    points.Add(currentPoint);

                    currentPoint = new XYZ(currentPoint.X, currentPoint.Y + l, currentPoint.Z);
                    points.Add(currentPoint);
                }

                XYZ lastPoint = points.Last();
                XYZ pointEnd = lastPoint + new XYZ(0, offsetHorizontal, 0);
                points.Add(pointEnd);
            }
            else
            {
                throw new InvalidOperationException("Không xác định được mặt phẳng (XZ hoặc YZ)");
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                lines.Add(Line.CreateBound(points[i], points[i + 1]));
            }

            lines.Add(Line.CreateBound(points.Last(), points.First()));

            return lines;
        }
        private Line CreatePathStair(StepModel Model)
        {
            Line pathLine;

            bool isXZPlane = Model.End.X != Model.Start.X && Model.End.Y == Model.Start.Y;
            bool isYZPlane = Model.End.Y != Model.Start.Y && Model.End.X == Model.Start.X;

            if (isXZPlane)
            {
                // Mặt phẳng XZ
                var l = (Model.End.X - Model.Start.X) / (Model.StepNumber - 1);
                XYZ endStep = Model.End + new XYZ(l, 0, 0);
                pathLine = Line.CreateBound(Model.Start, endStep);
            }
            else if (isYZPlane)
            {
                // Mặt phẳng YZ
                var l = (Model.End.Y - Model.Start.Y) / (Model.StepNumber - 1);
                XYZ endStep = Model.End + new XYZ(0, l, 0);
                pathLine = Line.CreateBound(Model.Start, endStep);
            }
            else
            {
                throw new InvalidOperationException("Không xác định được mặt phẳng cho CreatePathStair.");
            }

            Model.Path = pathLine;
            return pathLine;
        }
        public static double CalculateAngleByTan(double h, double b)
        {
            // Tránh chia cho 0
            if (b == 0)
                throw new ArgumentException("Cạnh kề (b) không được bằng 0");

            double radians = Math.Atan(h / b);        // trả về góc α (rad)
            double degrees = radians * (180 / Math.PI); // đổi rad sang độ

            return degrees;
        }
        public static double CalculateValueSinByAngle(double angleDeg, double thickness)
        {
            double angleRad = angleDeg * Math.PI / 180.0; // Đổi sang radian
            return thickness / Math.Sin(angleRad);
        }
        public static double CalculateValueCosByAngle(double angleDeg, double thickness)
        {
            double angleRad = angleDeg * Math.PI / 180.0; // Đổi sang radian
            return thickness / Math.Cos(angleRad);
        }
        private void OpenFileJson()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = @"JSON files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.Title = @"Open JSON File";

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                PathJson = openFileDialog.FileName;
            }
        }

    }
}
