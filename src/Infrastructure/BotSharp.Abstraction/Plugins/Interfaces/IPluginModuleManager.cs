using BotSharp.Abstraction.Plugins.Models;
using BotSharp.Abstraction.Utilities;
using System.Reflection;

namespace BotSharp.Abstraction.Plugins.Interfaces;

public interface IPluginModuleManager
{
    void AddPluginModules(Assembly assembly);

    void RemovePluginModules(string pluginId);

    List<PluginDef> GetPlugins(IServiceProvider services);

    PagedItems<PluginDef> GetPagedPlugins(IServiceProvider services,PluginFilter filter);

    PluginDef UpdatePluginStatus(IServiceProvider services,string id, bool enable);

    List<PluginMenuDef> GetPluginMenuByRoles(List<PluginMenuDef> plugins, string userRole);
}
