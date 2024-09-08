using BotSharp.Abstraction.Plugins.Interfaces;

namespace BotSharp.Core.Plugins.lmplements;

/// <summary>
/// <para>Load when the plugin is enabled, remove and release when the plugin is disabled</para>
/// <para>Only enabled plugins have context</para>
/// https://www.cnblogs.com/lwqlun/p/11395828.html
/// 1. When loading a plugin, we need to put the current plugin's assembly loading context into the _pluginContexts dictionary. The key of the dictionary is the name of the plugin, and the value of the dictionary is the plugin's assembly loading context.
/// 2. When removing a plugin, we need to use the Unload method to release the current assembly loading context.
/// </summary>
public class PluginContextManager : IPluginContextManager
{
    #region Fields

    private static readonly object _lockObject = new Object();

    private Dictionary<string, IPluginContext> _pluginContexts
    {
        get
        {
            return PluginContextStore.PluginContexts;
        }
    }

    #endregion

    #region Ctor
    public PluginContextManager()
    {
       
    }
    #endregion

    #region Methods

    public List<IPluginContext> All()
    {
        return _pluginContexts.Select(p => p.Value).ToList();
    }

    public bool Any(string pluginId)
    {
        return _pluginContexts.ContainsKey(pluginId);
    }

    public void Remove(string pluginId)
    {
        lock (_lockObject)
        {
            if (_pluginContexts.ContainsKey(pluginId))
            {
                _pluginContexts[pluginId].Unload();
                _pluginContexts.Remove(pluginId);
            }
        }
    }

    public IPluginContext Get(string pluginId)
    {
        return _pluginContexts[pluginId];
    }

    public void Add(string pluginId, IPluginContext context)
    {
        lock (_lockObject)
        {
            _pluginContexts.Add(pluginId, context);
        }
    }
    #endregion
}

public static class PluginContextStore
{
    public static Dictionary<string, IPluginContext> PluginContexts = new Dictionary<string, IPluginContext>();
}