using BotSharp.Abstraction.Plugins.Interfaces;
using BotSharp.AspNetCore.Interfaces;
using System.Reflection;

namespace BotSharp.AspNetCore.lmplements;

/// <summary>
/// All dlls of a plugin are managed by a <see cref="IPluginContext"/>
/// <see cref="PluginContextManager"/> records and manages the <see cref="IPluginContext"/> of all plugins
/// <see cref="AspNetCorePluginManager"/> is a wrapper of <see cref="PluginContextManager"/>, making it better manage the behavior of plugin loading and release
/// </summary>
public class AspNetCorePluginManager : IPluginManager
{
    private readonly IPluginControllerManager _pluginControllerManager;
    private readonly IPluginModuleManager _pluginModuleManager;

    public IPluginContextManager PluginContextManager { get; set; }

    public IPluginContextPack PluginContextPack { get; set; }

    public AspNetCorePluginManager(IPluginContextManager pluginContextManager, IPluginContextPack pluginContextPack,
        IPluginControllerManager pluginControllerManager, IPluginModuleManager pluginModuleManager)
    {
        this.PluginContextManager = pluginContextManager;
        this.PluginContextPack = pluginContextPack;
        _pluginControllerManager = pluginControllerManager;
        _pluginModuleManager = pluginModuleManager;
    }

    public void LoadPlugin(string pluginId)
    {
        IPluginContext context = this.PluginContextPack.Pack(pluginId);
        Assembly pluginMainAssembly = context.LoadFromAssemblyName(new AssemblyName(pluginId));

        _pluginControllerManager.AddControllers(pluginMainAssembly);
        _pluginModuleManager.AddPluginModules(pluginMainAssembly);

        this.PluginContextManager.Add(pluginId, context);
    }

    public void UnloadPlugin(string pluginId)
    {
        this.PluginContextManager.Remove(pluginId);
        _pluginControllerManager.RemoveControllers(pluginId);
        _pluginModuleManager.RemovePluginModules(pluginId);
    }

}