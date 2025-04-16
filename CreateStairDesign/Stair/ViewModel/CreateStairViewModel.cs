using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using CreateStairDesign.Stair.View;
using CreateStairDesign.Stair.Models;
using Line = Autodesk.Revit.DB.Line;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;
using Autodesk.Revit.Creation;
using Document = Autodesk.Revit.DB.Document;
using Plane = Autodesk.Revit.DB.Plane;
using CreateStairDesign.Stair.Model;
using System.Xml.Linq;
using System.IO;
using Autodesk.Revit.DB.Architecture;
using System.Windows;

namespace CreateStairDesign.Stair.ViewModel
{
    public class CreateStairViewModel : ObservableObject
    {
        public UIDocument UiDocument { get; set; }

        private Document _doc { get; set; }

        public CreateStairView MainView { get; set; }


        private string path { get; set; }
        public string PathXml
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged(nameof(PathXml));
            }
        }
        private List<ElementId> _createdExtrusions = new List<ElementId>();
        public Double OffsetStair3 { get; set; }
        public XmlStair LandingXml { get; set; }
        public XmlStair Stair1Xml { get; set; }
        public XmlStair Stair2Xml { get; set; }
        public XmlStair Stair3Xml { get; set; }
        public RelayCommand OpenFileCmd { get; set; }
        public RelayCommand RunCmd { get; set; }

        public CreateStairViewModel(Document doc)
        {
            _doc = doc;
            OpenFileCmd = new RelayCommand(OpenFileXml);
            RunCmd = new RelayCommand(Run);

        }
        private void Run()
        {
            MainView.Close();

            ReadXmlFromPath(path);

            var width = 1000.0.MmToFoot();

            var X1 = Stair1Xml.LowLevel.MmToFoot();
            var Y1 = Stair1Xml.LowLevel.MmToFoot();
            var X2 = Stair1Xml.HighLevel.MmToFoot();
            var Y2 = Stair1Xml.HighLevel.MmToFoot();
            var X3 = X2 + width;
            var Y3 = Y2;
            var X4 = X3 + (Stair2Xml.HighLevel.MmToFoot()- Stair2Xml.LowLevel.MmToFoot());
            var Y4 = Y3 - (Stair2Xml.HighLevel.MmToFoot() - Stair2Xml.LowLevel.MmToFoot());
            var X5 = X2 + (Stair3Xml.HighLevel.MmToFoot() - Stair3Xml.LowLevel.MmToFoot());
            var Y5 = Y2 + (Stair3Xml.HighLevel.MmToFoot() - Stair3Xml.LowLevel.MmToFoot());

            var stairModel = new StairModel()
            {
                Step1 = new StepModel()
                {
                    Thickness = Stair1Xml.Thickness.MmToFoot(),
                    StartLevel = Stair1Xml.LowLevel.MmToFoot(),
                    EndLevel = Stair1Xml.HighLevel.MmToFoot(),
                    StepNumber = Stair1Xml.StepNumber,
                    Start = new XYZ(X1, 0, Y1),
                    End = new XYZ(X2,0, Y2),
                    StepWidth = width,
                },
                Step2 = new StepModel()
                {
                    Thickness = Stair2Xml.Thickness.MmToFoot(),
                    StartLevel =Stair2Xml.LowLevel.MmToFoot(),
                    EndLevel = Stair2Xml.HighLevel.MmToFoot(),
                    StepNumber = Stair2Xml.StepNumber,
                    Start = new XYZ(X3,0,Y3),
                    End = new XYZ(X4, 0, Y4),
                    StepWidth = width,
                },
                Step3 = new StepModel()
                {
                    Thickness = Stair3Xml.Thickness.MmToFoot(),
                    StartLevel = Stair3Xml.LowLevel.MmToFoot(),
                    EndLevel = Stair3Xml.HighLevel.MmToFoot(),
                    StepNumber = Stair3Xml.StepNumber,
                    Start = new XYZ(0,X2,Y2),
                    End = new XYZ(0, X5, Y5),
                },
                Landing = new LandingModel()
                {
                    Thickness = LandingXml.Thickness.MmToFoot(),
                    Width =width,
                }
            };
            stairModel.Landing.Start = new XYZ(X2,0,Y2);
            stairModel.Landing.End = new XYZ(X3, 0, Y3);
            stairModel.Landing.Length = Math.Abs(width);
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

                if (_createdExtrusions.Count < 2)
                {
                 
                }
                else
                {
                    foreach (ElementId id in _createdExtrusions)
                    {
                        Element e = _doc.GetElement(id);
                        if (e is CombinableElement combinable)
                        {
                            combineArray.Append(combinable);
                        }
                    }
                    _doc.CombineElements(combineArray);

                }

                tx.Commit();
            }
        }
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
            XYZ P2 = new XYZ(Model.Start.X, Model.Start.Y, Model.Start.Z - Model.Thickness);
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
            Model.Path = CreatePathStair(Model);

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
        private void OpenFileXml()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.Title = "Open XML File";

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                PathXml = openFileDialog.FileName; // Nếu bạn muốn, đổi tên thành PathXml cho đúng ngữ nghĩa
            }
        }

        private void ReadXmlFromPath(string xmlPath)
        {
            XDocument xdoc = XDocument.Load(xmlPath);
            var targetCompo = xdoc.Descendants("XMLCompoG")
            .Where(g =>
                g.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"))?.Value == "XMLStairG" &&
                g.Descendants("XMLStairLandingG").Count() == 1 &&
                g.Descendants("XMLStairStepRunG").Count() == 3
            )
            .ToList();

            XElement landing = targetCompo
      .Descendants("XMLStairLandingG")
      .FirstOrDefault(x =>
          x.Element("Thick") != null &&
          x.Element("Level") != null &&
          x.Element("StartLevel") == null);


            if (landing != null)
            {
                int id = int.Parse(landing.Element("ID")?.Value ?? "0");
                double level = double.Parse(landing.Element("Level")?.Value ?? "0");
                double thick = double.Parse(landing.Element("Thick")?.Value ?? "0");

                LandingXml = new XmlStair
                {
                    ID = id,
                    LowLevel = level - thick,
                    HighLevel = level,
                    Thickness = thick,
                    StepNumber = 0,
                    Type = StairType.Landing
                };
            }
            else
            {
                System.Windows.MessageBox.Show("❌ Không tìm thấy Landing.");
            }

            var stairs = targetCompo.Descendants("XMLStairStepRunG")
        .Select(x => new
        {
            Element = x,
            ID = int.Parse(x.Element("ID")?.Value ?? "0"),
            StartLevel = double.Parse(x.Element("StartLevel")?.Value ?? "0"),
            EndLevel = double.Parse(x.Element("EndLevel")?.Value ?? "0"),
            Thick = double.Parse(x.Element("Thick")?.Value ?? "0"),
            StepNum = int.Parse(x.Element("StepsNum")?.Value ?? "0")
        })
        .ToList();

            var stair1 = stairs.OrderBy(s => s.EndLevel).FirstOrDefault();
            Stair1Xml = new XmlStair
            {
                ID = stair1.ID,
                LowLevel = stair1.StartLevel,
                HighLevel = stair1.EndLevel,
                Thickness = stair1.Thick,
                StepNumber = stair1.StepNum,
                Type = StairType.Stair
            };
            var stair2 = stairs.FirstOrDefault(s =>
                        s.ID != stair1.ID &&
                        Math.Abs(s.EndLevel - stair1.EndLevel) < 1e-6);

            Stair2Xml = new XmlStair
            {
                ID = stair2.ID,
                LowLevel = stair2.StartLevel,
                HighLevel = stair2.EndLevel,
                Thickness = stair2.Thick,
                StepNumber = stair2.StepNum,
                Type = StairType.Stair
            };
            var stair3 = stairs
                        .Where(s => s.ID != stair1.ID && s.ID != (stair2?.ID ?? -1))
                        .FirstOrDefault(s => s.EndLevel > stair1.EndLevel);
            Stair3Xml = new XmlStair
            {
                ID = stair3.ID,
                LowLevel = stair3.StartLevel,
                HighLevel = stair3.EndLevel,
                Thickness = stair3.Thick,
                StepNumber = stair3.StepNum,
                Type = StairType.Stair
            };
            if (Stair1Xml == null || Stair2Xml == null || Stair3Xml == null)
            {
                System.Windows.MessageBox.Show("❌ Không tìm thấy Stair.");
            }

            if (LandingXml != null && Stair1Xml != null && Stair2Xml != null && Stair3Xml != null)
            {
                string result =
                    "✅ Landing:\n" + LandingXml + "\n\n" +
                    "🟦 Stair 1:\n" + Stair1Xml + "\n\n" +
                    "🟩 Stair 2:\n" + Stair2Xml + "\n\n" +
                    "🟥 Stair 3:\n" + Stair3Xml;

                System.Windows.MessageBox.Show(result, "Thông tin thang & sàn nghỉ", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("❌ Không đầy đủ dữ liệu Landing hoặc Stair.", "Lỗi", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Warning);
            }
        }

    }

}
