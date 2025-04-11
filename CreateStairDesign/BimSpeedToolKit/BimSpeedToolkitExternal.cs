using System.ComponentModel;
using System.Reflection;
using Autodesk.Revit.UI;

namespace CreateStair.BimSpeedToolKit
{
    [PublicAPI]
    public abstract class BimSpeedToolkitExternal : IExternalApplication
    {
        private string _callerAssemblyDirectory;

        private Autodesk.Revit.UI.UIApplication _uiApplication;

        public Result Result { get; set; }

        //
        // Summary:
        //     Reference to the Autodesk.Revit.UI.UIControlledApplication that is needed by
        //     an external application
        public UIControlledApplication Application { get; private set; }

        public Autodesk.Revit.UI.UIApplication UiApplication => _uiApplication ?? (_uiApplication = (Autodesk.Revit.UI.UIApplication)(Application?.GetType().GetField("m_uiapplication", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Application)));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Result OnShutdown(UIControlledApplication application)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyOnShutdown;
            try
            {
                OnShutdown();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssemblyOnShutdown;
            }

            return Result.Succeeded;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Result OnStartup(UIControlledApplication application)
        {
            Application = application;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyOnStartup;
            try
            {
                OnStartup();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssemblyOnStartup;
            }

            return Result;
        }

        public virtual void OnShutdown()
        {
        }
        public abstract void OnStartup();

        private Assembly ResolveAssemblyOnStartup(object sender, ResolveEventArgs args)
        {
            return ResolveHelper.ResolveAssembly("OnStartup", args, ref _callerAssemblyDirectory);
        }

        private Assembly ResolveAssemblyOnShutdown(object sender, ResolveEventArgs args)
        {
            return ResolveHelper.ResolveAssembly("OnShutdown", args, ref _callerAssemblyDirectory);
        }
    }
}
