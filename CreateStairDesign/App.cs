using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using CreateStair.BimSpeedToolKit;
using Autodesk.Revit.UI;
using Serilog.Events;
using GetDiameterInRevitDesign.GetDiameterAndPatternInRevit;


namespace CreateStair
{
    [UsedImplicitly]
    public class App : BimSpeedToolkitExternal
    {
        public override void OnStartup()
        {
            CreateLogger();
            CreateRibbon();
        }

        public override void OnShutdown()
        {
            Log.CloseAndFlush();
        }

        private void CreateRibbon()
        {
            var pathIcons = "/CreateStair;component/Resources/Icons/";

            string tabName = "ExportRebarClash";

            // Kiểm tra nếu tab chưa tồn tại thì tạo mới
            try
            {
                Application.CreateRibbonTab(tabName);
            }
            catch
            {
                // Tab đã tồn tại, không cần tạo lại
            }

            RibbonPanel panel = Application.CreateRibbonPanel(tabName, "ExportRebarClash");

            var getDiameter = new PushButtonData(typeof(CreateStairRevitCmd).FullName, "Export Rebar Clash", Assembly.GetAssembly(typeof(CreateStairRevitCmd)).Location, typeof(CreateStairRevitCmd).FullName);

            getDiameter.Image = new BitmapImage(new Uri(pathIcons + "app-16.png", UriKind.RelativeOrAbsolute));
            getDiameter.LargeImage = new BitmapImage(new Uri(pathIcons + "app-32.png", UriKind.RelativeOrAbsolute));

            panel.AddItem(getDiameter);

        }
        private static void CreateLogger()
        {
            const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug(LogEventLevel.Debug, outputTemplate)
                .MinimumLevel.Debug()
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var e = (Exception)args.ExceptionObject;
                Log.Fatal(e, "Domain unhandled exception");
            };
        }
    }
}