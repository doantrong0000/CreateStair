using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using CreateStairDesign.Stair.View;
using BimSpeedUtils;
using System.Windows.Shapes;
using Serilog.Core;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace CreateStairDesign.Stair.ViewModel
{
    public class CreateStairViewModel : ObservableObject
    {
        public UIDocument UiDocument { get; set; }
        public Document doc { get; set; }
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

        public FamilySymbol FamilyStair { get; set; }

        public RelayCommand OpenFileCmd { get; set; }
        public RelayCommand RunCmd { get; set; }

        public CreateStairViewModel(UIDocument uiDocument)
        {
            UiDocument = uiDocument;
            doc = uiDocument.Document;
            OpenFileCmd = new RelayCommand(OpenFileJson);
            RunCmd = new RelayCommand(Run);
            LoadFamily();
            
        }
        private void LoadFamily()
        {
           try
            {
                var thang = new FilteredElementCollector(doc)
                   .OfClass(typeof(FamilySymbol))
                   .OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilySymbol>()
                   .FirstOrDefault(x => x.Name =="BS_Stair_test");
                FamilyStair = thang;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }   
        }
        private void AssignValue()
        {
            try
            {
             
            }
            catch (Exception ex)
            {
               System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void Run()
        {
       
            if (FamilyStair == null)
            {
                System.Windows.MessageBox.Show("Không tìm thấy family 'BS_Stair_test'");
                return;
            }

            if (!FamilyStair.IsActive)
            {
                FamilyStair.Activate();
                doc.Regenerate();
            }
            AssignValue();
            MainView.Close();
            XYZ point = UiDocument.Selection.PickPoint("Chọn điểm để đặt cầu thang");
            
            
            using (Transaction trans = new Transaction(doc, "Place Stair Family"))
            {
                trans.Start();

                // Dùng dạng NonStructural nếu đặt bằng điểm
                var stair = doc.Create.NewFamilyInstance(
                    point,
                    FamilyStair,
                    StructuralType.NonStructural // CHÚ Ý: beam thường yêu cầu Line
                );
                Parameter paramB1 = stair.LookupParameter("b1");
                if (paramB1 != null && !paramB1.IsReadOnly)
                {
                    paramB1.Set(5000.MmToFoot()); // đặt 500mm (chuyển về foot)
                }

                trans.Commit();
            }
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
