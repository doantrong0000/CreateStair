using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CreateStair.BimSpeedToolKit
{
    [PublicAPI]
    public static class ResolveHelper
    {
        //
        // Summary:
        //     Represents a method that handles the System.AppDomain.AssemblyResolve event of
        //     an System.AppDomain
        //
        // Parameters:
        //   sender:
        //     The source of the event
        //
        //   args:
        //     The event data
        //
        // Returns:
        //     The assembly that resolves the type, assembly, or resource; or null if the assembly
        //     cannot be resolved
        //
        // Remarks:
        //     Optimized assembly resolver is enabled by default for Nice3point.Revit.Toolkit.External.ExternalApplication
        //     and Nice3point.Revit.Toolkit.External.ExternalCommand
        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            if (frames == null)
            {
                return null;
            }

            string name = new AssemblyName(args.Name).Name;
            List<string> list = new List<string>();
            for (int i = 0; i < frames.Length; i++)
            {
                MethodBase method = frames[i].GetMethod();
                if ((object)method.DeclaringType == null)
                {
                    continue;
                }

                string directoryName = Path.GetDirectoryName(method.DeclaringType.Assembly.Location);
                if (list.Contains(directoryName))
                {
                    continue;
                }

                list.Add(directoryName);
                foreach (string item in Directory.EnumerateFiles(directoryName, "*.dll"))
                {
                    if (name == Path.GetFileNameWithoutExtension(item))
                    {
                        return Assembly.LoadFile(item);
                    }
                }
            }

            return null;
        }

        internal static Assembly ResolveAssembly(string callerMethod, ResolveEventArgs arguments, ref string assembliesDirectory)
        {
            if (assembliesDirectory == null)
            {
                StackFrame[] frames = new StackTrace().GetFrames();
                if (frames != null)
                {
                    StackFrame[] array = frames;
                    for (int i = 0; i < array.Length; i++)
                    {
                        MethodBase method = array[i].GetMethod();
                        if (method.Name == callerMethod && method.IsVirtual && (object)method.DeclaringType != null)
                        {
                            assembliesDirectory = Path.GetDirectoryName(method.DeclaringType.Assembly.Location);
                            break;
                        }
                    }
                }

                if (assembliesDirectory == null)
                {
                    return null;
                }
            }

            string name = new AssemblyName(arguments.Name).Name;
            foreach (string item in Directory.EnumerateFiles(assembliesDirectory, "*.dll"))
            {
                if (name == Path.GetFileNameWithoutExtension(item))
                {
                    return Assembly.LoadFile(item);
                }
            }

            return null;
        }
    }
}
