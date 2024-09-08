using BotSharp.Abstraction.Plugins.Interfaces;

namespace BotSharp.Core.Plugins.lmplements;

/// <summary>
/// All dlls for a plugin are managed by a <see cref="IPluginContext"/>
/// <see cref="PluginContextManager"/> records manage the <see cref="IPluginContext"/> for all plugins
/// <see cref="PluginManager"/> is an encapsulation of <see cref="PluginContextManager"/>, making it better manage the loading and releasing behavior of plugins
/// </summary>
public class PluginManager : IPluginManager
{
    public IPluginContextManager PluginContextManager { get; set; }

    public IPluginContextPack PluginContextPack { get; set; }

    public PluginManager(IPluginContextManager pluginContextManager, IPluginContextPack pluginContextPack)
    {
        this.PluginContextManager = pluginContextManager;
        this.PluginContextPack = pluginContextPack;
    }

    public void LoadPlugin(string pluginId)
    {
        IPluginContext context = this.PluginContextPack.Pack(pluginId);
        this.PluginContextManager.Add(pluginId, context);
    }

    public void UnloadPlugin(string pluginId)
    {
        this.PluginContextManager.Remove(pluginId);
    }
}
