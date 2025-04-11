using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreateStairDesign.CreateStair.Json;
using CreateStairDesign.CreateStair.View;
using CreateStairDesign.ProgressBar;
using CreateStairDesign.Utils;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CreateStairDesign.CreateStair.ViewModel
{
    public class CreateStairViewModel : ObservableObject
    {
        #region Fileds / Commands

        private int selcectedIndexLang { get; set; } = 1;

        public int SelcectedIndexLang
        {
            get => selcectedIndexLang;
            set
            {
                if (Equals(selcectedIndexLang, value)) return;
                selcectedIndexLang = value;
                OnPropertyChanged();
            }
        }
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

        public CreateStairView MainView { get; set; }

        public UIDocument UiDocument { get; set; }

        public RelayCommand OpenFileCmd { get; set; }
        public RelayCommand CloseFormCmd { get; set; }
        public RelayCommand<CreateStairView> UpdateFileExcelCmd { get; set; }
        public RelayCommand CloseWindowCommand { get; set; }

        #endregion

        public CreateStairViewModel(UIDocument uiDocument)
        {
        } 

    }
}
