using BotSharp.Core.Plugins.Models;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Core.Plugins;

public class PluginInfoModelFactory
{
    private const string InfoJson = "info.json";

    public static PluginInfoModel Read(string pluginId)
    {
        PluginInfoModel pluginInfoModel = new PluginInfoModel();
        string pluginDir = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId);
        string pluginInfoFilePath = Path.Combine(pluginDir, InfoJson);

        if (!File.Exists(pluginInfoFilePath))
        {
            return null;
        }
        try
        {
            string pluginInfoJsonStr = File.ReadAllText(pluginInfoFilePath, Encoding.UTF8);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            pluginInfoModel = JsonSerializer.Deserialize<PluginInfoModel>(pluginInfoJsonStr, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            pluginInfoModel = null;
        }

        return pluginInfoModel;
    }

    public static IList<PluginInfoModel> ReadAll()
    {
        IList<PluginInfoModel> pluginInfoModels = new List<PluginInfoModel>();
        IList<string> pluginDirs = PluginPathProvider.AllPluginDir();
        foreach (var dir in pluginDirs)
        {
            string pluginId = PluginPathProvider.GetPluginFolderNameByDir(dir);
            PluginInfoModel model = Read(pluginId);
            pluginInfoModels.Add(model);
        }
        pluginInfoModels = pluginInfoModels.Where(m => m != null).ToList();

        return pluginInfoModels;
    }

    /// <summary>
    /// Reads the plug-in information from the specified plug-in directory
    /// It can be used to read the plug-in information in the temporary plug-in upload directory
    /// </summary>
    /// <param name="pluginDir"></param>
    /// <returns></returns>
    public static PluginInfoModel ReadPluginDir(string pluginDir)
    {
        PluginInfoModel pluginInfoModel = new PluginInfoModel();
        string pluginInfoFilePath = Path.Combine(pluginDir, InfoJson);

        if (!File.Exists(pluginInfoFilePath))
        {
            return null;
        }
        try
        {
            string pluginInfoJsonStr = File.ReadAllText(pluginInfoFilePath, Encoding.UTF8);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            pluginInfoModel = JsonSerializer.Deserialize<PluginInfoModel>(pluginInfoJsonStr, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            pluginInfoModel = null;
        }

        return pluginInfoModel;
    }
}
