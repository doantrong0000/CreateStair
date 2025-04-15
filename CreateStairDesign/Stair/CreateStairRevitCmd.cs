using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreateStairDesign.Stair.ViewModel;
using CreateStairDesign.Stair.View;
using Autodesk.Revit.ApplicationServices;

namespace GetDiameterInRevitDesign.GetDiameterAndPatternInRevit
{
    [Transaction(TransactionMode.Manual)]
    public class CreateStairRevitCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            var viewModel = new CreateStairViewModel(doc);

            var view = new CreateStairView() { DataContext = viewModel };

            viewModel.MainView = view;

            view.ShowDialog();


            return Result.Succeeded;
        }
    }
}
