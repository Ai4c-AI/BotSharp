namespace BotSharp.Abstraction.Plugins;

public interface IPlugin
{
    /// <summary>
    /// After enabling the plugin: At this time, the plugin assemblies have been loaded (the plugin context has been loaded)
    /// </summary>
    /// <returns>
    /// <para>When IsSuccess is False, the main program will roll back the plugin status: (1) unload the plugin context (2) update plugin.config.json and mark it as disabled</para>
    /// </returns>
    (bool IsSuccess, string Message) AfterEnable();

    /// <summary>
    ///Before disabling the plugin: The plugin assemblies have not been released (the plugin context has not been unloaded)
    /// </summary>
    /// <returns>
    /// <para>Only when IsSuccess is True, the main program will release the plugin context and mark it as disabled</para>
    /// <para>When IsSuccess is False, the main program will not release the plugin context, nor mark it as disabled, and the plugin will maintain its original state</para>    /// </returns>
    (bool IsSuccess, string Message) BeforeDisable();

    /// <summary>
    /// Plugin Upgrade
    /// </summary>
    /// <param name="currentVersion"></param>
    /// <param name="targetVersion"></param>
    /// <returns></returns>
    (bool IsSuccess, string Message) Update(string currentVersion, string targetVersion);

    /// <summary>
    /// When the main application starts
    /// </summary>
    void AppStart();

    /// <summary>
    /// Startup order: The pre-installed plugins that this plugin depends on
    /// </summary>
    /// <value></value>
    List<string> AppStartOrderDependPlugins { get; }
}
