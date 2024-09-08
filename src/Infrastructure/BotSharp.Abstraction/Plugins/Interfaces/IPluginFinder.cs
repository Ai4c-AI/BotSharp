using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Plugins.Interfaces;

public interface IPluginFinder
{
    /// <summary>
    ///  Implements an enabling plug-in for a specified interface or type
    /// </summary>
    /// <typeparam name="TPlugin">It can be an interface, an abstract class, or a common implementation class, as long as <see cref="IPlugin"/> is implemented</typeparam>
    /// <returns></returns>
    IEnumerable<TPlugin> EnablePlugins<TPlugin>()
       where TPlugin : IPlugin;

    /// <summary>
    ///  Implements an enabling plug-in for a specified interface or type
    /// </summary>
    /// <typeparam name="TPlugin">It can be an interface, an abstract class, or a common implementation class, as long as <see cref="IPlugin"/> is implemented</typeparam>
    /// <returns></returns>
    IEnumerable<(TPlugin PluginInstance, string PluginId)> EnablePluginsFull<TPlugin>()
        where TPlugin : IPlugin; 
    /// <summary>
    /// 所有启用的插件 的 PluginId 
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> EnablePluginIds<TPlugin>()
        where TPlugin : IPlugin; // BasePlugin


    /// <summary>
    /// All enabled plugins
    /// </summary>
    /// <returns></returns>
    IEnumerable<IPlugin> EnablePlugins();

    /// <summary>
    /// All enabled plugins
    /// </summary>
    /// <returns></returns>
    IEnumerable<(IPlugin PluginInstance, string PluginId)> EnablePluginsFull();

    /// <summary>
    /// All enabled plugins的 PluginId 
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> EnablePluginIds();

    /// <summary>
    /// Get the enabled plugin with the specified pluginId
    /// </summary>
    /// <param name="pluginId"></param>
    /// <returns>
    /// 1.The plugin is not enabled to return null, 
    /// 2.This plugin context cannot be found and returns null 
    /// 3.The plugin master dll cannot be found and returns null 
    /// 4.The plugin master dll cannot be found to return null with IPlugin's implementation, 
    /// 5.The plugin cannot instantiate and return null
    /// </returns>
    IPlugin Plugin(string pluginId);
}
