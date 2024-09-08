using BotSharp.Core.Plugins.Models;
using System.IO;

namespace BotSharp.Core.Plugins;

public class PluginConfigModelFactory
{    
    public static PluginConfigModel Read()
    {
        var currentDir = Directory.GetCurrentDirectory();
        PluginConfigModel pluginConfigModel = new PluginConfigModel();
        string pluginConfigFilePath = Path.Combine(currentDir, "App_Data", "plugin.config.json");
        if (!File.Exists(pluginConfigFilePath))
        {
            return new PluginConfigModel();
        }
        string pluginConfigJsonStr = File.ReadAllText(pluginConfigFilePath, Encoding.UTF8);
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        pluginConfigModel = JsonSerializer.Deserialize<PluginConfigModel>(pluginConfigJsonStr, jsonSerializerOptions);
        pluginConfigModel = EnabledPluginsSort(pluginConfigModel);

        return pluginConfigModel;
    }

 
    public static void Save(PluginConfigModel pluginConfigModel)
    {
        if (pluginConfigModel == null)
        {
            throw new ArgumentNullException(nameof(pluginConfigModel));
        }
        try
        {
            var currentDir = Directory.GetCurrentDirectory();
            pluginConfigModel = EnabledPluginsSort(pluginConfigModel);
            string pluginConfigJsonStr = JsonSerializer.Serialize(pluginConfigModel);
            string pluginConfigFilePath = Path.Combine(currentDir, "App_Data", "plugin.config.json");
            File.WriteAllText(pluginConfigFilePath, pluginConfigJsonStr, Encoding.UTF8);
        }
        catch (Exception ex)
        { }

    }
 
    public static PluginConfigModel EnabledPluginsSort(PluginConfigModel pluginConfigModel)
    {
        var dependencySorter = new DependencySorter<string>();
        dependencySorter.AddObjects(pluginConfigModel.EnabledPlugins.ToArray());
        foreach (var plugin in pluginConfigModel.EnabledPlugins)
        {
            try
            {
                IList<string> dependPlugins = PluginInfoModelFactory.Read(plugin).DependPlugins;
                if (dependPlugins != null && dependPlugins.Count >= 1)
                {
                    dependencySorter.SetDependencies(obj: plugin, dependsOnObjects: dependPlugins.ToArray());
                }
            }
            catch (System.Exception ex)
            {
            }
        }
        try
        {
            var sortedPlugins = dependencySorter.Sort();
            if (sortedPlugins != null && sortedPlugins.Length >= 1)
            {
                pluginConfigModel.EnabledPlugins = sortedPlugins.ToList();
            }
        }
        catch (System.Exception ex)
        {
        }

        return pluginConfigModel;
    }
}