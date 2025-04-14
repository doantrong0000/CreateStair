using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreateStairDesign.Stair.ViewModel;
using CreateStairDesign.Stair.View;

namespace GetDiameterInRevitDesign.GetDiameterAndPatternInRevit
{
    [Transaction(TransactionMode.Manual)]
    public class CreateStairRevitCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData.Application.ActiveUIDocument;

            var viewModel = new CreateStairViewModel(uiDocument);

            var view = new CreateStairView() { DataContext = viewModel };

            viewModel.MainView = view;

            view.ShowDialog();


            return Result.Succeeded;
        }
    }
}
