using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using TTController.Common.Plugin;

namespace TTController.Service.Utils
{
    public static class PluginLoader
    {
        private static readonly AssemblyLoadContext PluginsAssemblyLoadContext = new PluginLoaderAssemblyLoadContext();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Type[] PluginTypes = new [] {
                typeof(IEffectBase),
                typeof(ISpeedControllerBase),
                typeof(ITriggerBase),
                typeof(IControllerDefinition)
        };

        internal static void Load(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Could not find directory {path}");

            foreach (var dllFile in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
            {
                var context = new AssemblyDependencyLoadContext(dllFile);
                var assembly = context.LoadFromAssemblyPath(dllFile);
                var valid = PluginTypes.Any(t => assembly.GetExportedTypes().Any(t.IsAssignableFrom));

                if (valid)
                {
                    foreach (var contextAssembly in context.Assemblies)
                    {
                        Logger.Info("Loading plugin assembly: {0} [{1}]", contextAssembly.GetName().Name, contextAssembly.GetName().Version);
                        PluginsAssemblyLoadContext.LoadFromAssemblyPath(contextAssembly.Location);
                    }
                }

                context.Unload();
            }
        }

        private class AssemblyDependencyLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            public AssemblyDependencyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
            }

            protected override Assembly Load(AssemblyName name)
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(name);
                if (assemblyPath != null)
                    return LoadFromAssemblyPath(assemblyPath);

                return null;
            }
        }

        private class PluginLoaderAssemblyLoadContext : AssemblyLoadContext
        {
            protected override Assembly Load(AssemblyName name)
            {
                return null;
            }
        }

    }
}
