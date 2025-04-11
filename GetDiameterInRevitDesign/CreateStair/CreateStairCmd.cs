using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreateStairDesign.CreateStair.View;
using CreateStairDesign.CreateStair.ViewModel;

namespace CreateStairDesign.CreateStair
{
    [Transaction(TransactionMode.Manual)]
    public class CreateStairCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Sử dụng TaskDialog của Revit để hiển thị thông báo.
                TaskDialog.Show("TestTemplateCmd", "Hello from TestTemplateCmd!");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
