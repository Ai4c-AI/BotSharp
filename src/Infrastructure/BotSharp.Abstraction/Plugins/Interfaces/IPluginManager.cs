namespace BotSharp.Abstraction.Plugins.Interfaces;

public interface IPluginManager
{
    /// <summary>
    /// Load the plugin
    /// </summary>
    /// <param name="pluginId"></param>
    void LoadPlugin(string pluginId);

    /// <summary>
    /// UnLoad the plugin
    /// </summary>
    /// <param name="pluginId"></param>
    void UnloadPlugin(string pluginId);
}
