using System;
using System.IO;
using System.Reflection;

namespace SimpleAnalyzer
{
    internal class AssemblyResolveHelper : IDisposable
    {
        public static AssemblyResolveHelper CreateHelper()
        {
            return new AssemblyResolveHelper();
        }

        private AssemblyResolveHelper()
        {
            AppDomain.CurrentDomain.AssemblyResolve += HelpTheFrameworkToResolveTheAssembly;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= HelpTheFrameworkToResolveTheAssembly;
        }
        private Assembly HelpTheFrameworkToResolveTheAssembly(object sender, ResolveEventArgs args)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{new AssemblyName(args.Name).Name}.dll");

            if (!File.Exists(path))
            {
                if (GloablOptions.AssemblyResolverDebugOutput)
                    Console.WriteLine($"Failed to load {args.Name} from {path}");
                return null;
            }

            if (GloablOptions.AssemblyResolverDebugOutput)
                Console.WriteLine($"Failed to load {args.Name}, loaded from {path}");

            return Assembly.LoadFrom(path);
        }
    }
}