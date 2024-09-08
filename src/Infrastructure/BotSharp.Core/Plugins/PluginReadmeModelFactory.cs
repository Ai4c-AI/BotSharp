using BotSharp.Core.Plugins.Models;
using System.IO;

namespace BotSharp.Core.Plugins;

/// <summary>
/// Documentation for the plugin
/// </summary>
public class PluginReadmeModelFactory
{
    private const string ReadmeFile = "README.md";

    public static PluginReadmeModel Read(string pluginId)
    {
        PluginReadmeModel readmeModel = new PluginReadmeModel();
        string pluginDir = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId);
        string pluginReadmeFilePath = Path.Combine(pluginDir, ReadmeFile);

        if (!File.Exists(pluginReadmeFilePath))
        {
            return null;
        }
        try
        {
            string readmeStr = File.ReadAllText(pluginReadmeFilePath, Encoding.UTF8);
            readmeModel.PluginId = pluginId;
            readmeModel.Content = readmeStr;
        }
        catch (Exception ex)
        {
            readmeModel = null;
        }

        return readmeModel;
    } 
}