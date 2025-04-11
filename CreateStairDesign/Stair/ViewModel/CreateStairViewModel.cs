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
using System.Windows.Shapes;

namespace CreateStairDesign.Stair.ViewModel
{
    public class CreateStairViewModel : ObservableObject
    {
        public UIDocument UiDocument { get; set; }
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

        public RelayCommand OpenFileCmd { get; set; }

        public CreateStairViewModel(UIDocument uiDocument)
        {
            UiDocument = uiDocument;
            OpenFileCmd = new RelayCommand(OpenFileJson);

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
