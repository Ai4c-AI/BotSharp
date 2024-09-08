using BotSharp.Abstraction.Plugins.Interfaces;
using System.IO;
using System.Reflection;

namespace BotSharp.Core.Plugins.lmplements;

public class PluginContextPack : IPluginContextPack
{
    public IPluginContext Pack(string pluginId)
    {
        // The main DLL of the plugin, excluding the DLL referenced by the plugin project
        string pluginMainDllFilePath = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId, $"{pluginId}.dll");
        // The loading context for this plugin
        var context = new PluginLoadContext(pluginId, pluginMainDllFilePath);
        Assembly pluginMainAssembly = context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginMainDllFilePath)));
 
        return context;
    }
}
