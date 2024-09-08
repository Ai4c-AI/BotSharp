using BotSharp.Abstraction.Plugins.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Plugins;

public class PluginSettingsModelFactory
{
    private const string SettingsFile = "settings.json";

    #region 即时读取
    public static T Create<T>(string pluginId)
        where T : PluginSettingsModel
    {
        PluginSettingsModel rtnModel = new PluginSettingsModel();
        string pluginDir = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId);
        string pluginSettingsFilePath = Path.Combine(pluginDir, SettingsFile);

        if (!File.Exists(pluginSettingsFilePath))
        {
            return null;
        }
        try
        {
            string settingsStr = File.ReadAllText(pluginSettingsFilePath, Encoding.UTF8);
            rtnModel = JsonSerializer.Deserialize<T>(settingsStr);
        }
        catch (Exception ex)
        {
            rtnModel = null;
        }

        return rtnModel as T;
    }

    public static string Create(string pluginId)
    {
        string rtnStr = string.Empty;
        string pluginDir = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId);
        string pluginSettingsFilePath = Path.Combine(pluginDir, SettingsFile);

        if (!File.Exists(pluginSettingsFilePath))
        {
            return null;
        }
        try
        {
            rtnStr = File.ReadAllText(pluginSettingsFilePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            rtnStr = null;
        }

        return rtnStr;
    }
    #endregion

    #region 保存
    public static void Save<T>(T pluginSettingsModel, string pluginId)
        where T : PluginSettingsModel
    {
        if (pluginSettingsModel == null)
        {
            throw new ArgumentNullException(nameof(pluginSettingsModel));
        }
        try
        {
            string pluginSettingsJsonStr = System.Text.Json.JsonSerializer.Serialize<T>(pluginSettingsModel);
            string pluginSettingsFilePath = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId, SettingsFile);
            File.WriteAllText(pluginSettingsFilePath, ConvertJsonString(pluginSettingsJsonStr), Encoding.UTF8);
        }
        catch (Exception ex)
        { }

    }

    public static void Save(string pluginSettingsJsonStr, string pluginId)
    {
        if (pluginSettingsJsonStr == null)
        {
            throw new ArgumentNullException(nameof(pluginSettingsJsonStr));
        }
        try
        {
            string pluginSettingsFilePath = Path.Combine(PluginPathProvider.PluginsRootPath(), pluginId, SettingsFile);
            File.WriteAllText(pluginSettingsFilePath, ConvertJsonString(pluginSettingsJsonStr), Encoding.UTF8);
        }
        catch (Exception ex)
        { }

    }
    #endregion

    #region 格式化JSON字符串
    private static string ConvertJsonString(string str)
    {
        // https://blog.csdn.net/essity/article/details/84644510
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.WriteIndented = true;

        object jsonObj = JsonSerializer.Deserialize<object>(str);
        string rtnStr = System.Text.Json.JsonSerializer.Serialize(jsonObj, options);

        return rtnStr;
        #endregion
    }

}