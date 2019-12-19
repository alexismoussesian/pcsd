using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace pcsd.plugins
{
    public class PluginLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Loads all plugins from the Plugins folder and specified on the command line
        /// </summary>
        /// <param name="pluginsFolder"></param>
        /// <param name="args"></param>
        /// <param name="pluginsCmdArgsHelp"></param>
        public ICollection<IPlugin> LoadPlugins(string pluginsFolder, string[] args, out List<string[]> pluginsCmdArgsHelp)
        {
            ICollection<IPlugin> enabledPlugins = new List<IPlugin>();
            pluginsCmdArgsHelp = new List<string[]>();
            string[] pluginNames = null;
            // Unit test fail because Assembly.GetEntryAssembly is Null - JR
            var fullPluginsPath = GetPluginsPath(pluginsFolder);

            try
            {
                // Get all DLLs in the plugins folder
                if (Directory.Exists(fullPluginsPath)) pluginNames = Directory.GetFiles(fullPluginsPath, "*.dll");

                // Load the plugins through reflection
                if (pluginNames != null)
                {
                    var assemblies = new List<Assembly>(pluginNames.Length);
                    assemblies.AddRange(pluginNames.Select(AssemblyName.GetAssemblyName).Select(Assembly.Load));

                    // Search for all types that implement our IPlugin interface
                    var pluginType = typeof(IPlugin);
                    var pluginTypes = new List<Type>();
                    foreach (var assembly in assemblies)
                    {
                        if (assembly == null) continue;
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }
                            if (type.GetInterface(pluginType.FullName) != null)
                            {
                                var name = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
                                Log.Debug($"Plugin {name} found.");
                                pluginTypes.Add(type);
                            }
                        }
                    }

                    // Create instances from our found plugins
                    var plugins = new List<IPlugin>(pluginTypes.Count);
                    plugins.AddRange(pluginTypes.Select(type => (IPlugin)Activator.CreateInstance(type)));

                    var interval = DateTime.UtcNow;

                    // Call Initialize for each required plugin
                    foreach (var plugin in plugins)
                    {
                        var requiredArgs = plugin.GetCommandLineParameters();
                        var argsHelp = plugin.GetCommandLineParametersHelp();
                        pluginsCmdArgsHelp.Add(argsHelp);
                        var pluginArgs = new List<string>();
                        foreach (var requiredArg in requiredArgs) { pluginArgs.AddRange((args.Where(a => a.StartsWith(requiredArg))).ToList()); }
                        if (!pluginArgs.Any()) continue;
                        plugin.Initialize(pluginArgs.ToArray());
                        plugin.ResetInterval(interval);
                        enabledPlugins.Add(plugin);
                        Log.Info($"Plugin {plugin} loaded.");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length > 0)
                {
                    foreach (var exception in ex.LoaderExceptions)
                        Log.Error("An error occurred while loading plugins", exception);
                }
                else
                    Log.Error("An error occurred while loading plugins", ex);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while loading plugins", ex);
            }
            return enabledPlugins;
        }

        private string GetPluginsPath(string pluginsFolder)
        {
            return Assembly.GetEntryAssembly() == null ? pluginsFolder : $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\{pluginsFolder}";
        }
    }
}
