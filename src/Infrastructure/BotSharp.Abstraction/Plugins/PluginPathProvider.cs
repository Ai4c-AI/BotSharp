using System.IO;
using System.Reflection;

namespace BotSharp.Abstraction.Plugins
{
    public class PluginPathProvider
    {
        static PluginPathProvider()
        {
            // Initialize plugin directory
            var currentDir = Directory.GetCurrentDirectory();
            string appDataDir = Path.Combine(currentDir, "App_Data");
            if (!Directory.Exists(appDataDir))
            {
                Directory.CreateDirectory(appDataDir);
            }

            string pluginConfigJsonFilePath = Path.Combine(currentDir, "App_Data", "plugin.config.json");
            string pluginConfigJson = "{\"EnabledPlugins\":[],\"DisabledPlugins\":[],\"UninstalledPlugins\":[]}";
            if (!File.Exists(pluginConfigJsonFilePath))
            {
                File.WriteAllText(pluginConfigJsonFilePath, pluginConfigJson, System.Text.Encoding.UTF8);
            }

            string tempPluginUploadDir = TempPluginUploadDir();
            if (!Directory.Exists(tempPluginUploadDir))
            {
                Directory.CreateDirectory(tempPluginUploadDir);
            }

            string pluginsDir = PluginsRootPath();
            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }

            string pluginsWwwRootDir = PluginsWwwRootDir();
            if (!Directory.Exists(pluginsWwwRootDir))
            {
                Directory.CreateDirectory(pluginsWwwRootDir);
            }

            string pluginAdminDir = PluginAdminDir();
            if (!Directory.Exists(pluginAdminDir))
            {
                Directory.CreateDirectory(pluginAdminDir);
            }
        }

        /// <summary>
        /// The path to the directory where the temporary plugin is uploaded
        /// Eg: App_Data\TempPluginUpload
        ///</summary>
        /// <returns></returns>
        public static string TempPluginUploadDir()
        {
            var currentDir = Directory.GetCurrentDirectory();
            string tempPluginUploadDir = Path.Combine(currentDir, "App_Data", "TempPluginUpload");
            return tempPluginUploadDir;
        }

        /// <summary>
        /// Get the path to the plugins
        /// </summary>
        /// <returns></returns>
        public static string PluginsRootPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            string pluginRootPath = Path.Combine(currentDir, "Plugins");

            return pluginRootPath;
        }

        /// <summary>
        /// Obtain the name of the destination plugin folder
        /// </summary>
        /// <param name="pluginDir">The full directory path of the target plug-in</param>
        /// <returns></returns>
        public static string GetPluginFolderNameByDir(string pluginDir)
        {
            string pluginRootPath = PluginsRootPath();
            string pluginFolderName = pluginDir.Replace(pluginRootPath + Path.DirectorySeparatorChar, "");

            return pluginFolderName;
        }

        /// <summary>
        /// The full directory path for all plugins
        /// </summary>
        /// <returns></returns>
        public static IList<string> AllPluginDir()
        {
            string pluginRootPath = PluginsRootPath();
            string[] pluginDirs = Directory.GetDirectories(pluginRootPath, "*");

            return pluginDirs;
        }

        /// <summary>
        /// Folder names for all plugins
        /// </summary>
        /// <returns></returns>
        public static IList<string> AllPluginFolderName()
        {
            IList<string> pluginFolderNames = new List<string>();
            IList<string> pluginDirs = AllPluginDir();
            foreach (var dir in pluginDirs)
            {
                string pluginFolderName = GetPluginFolderNameByDir(dir);
                pluginFolderNames.Add(pluginFolderName);
            }

            return pluginFolderNames;
        }

        /// <summary>
        /// Plugins/{pluginId}/wwwroot
        /// </summary>
        /// <returns></returns>

        public static string WwwRootDir(string pluginId)
        {
            string wwwrootDir = Path.Combine(PluginsRootPath(), pluginId, "wwwroot");

            return wwwrootDir;
        }


        /// <summary>
        /// Plugins_wwwroot
        /// </summary>
        /// <returns></returns>
        public static string PluginsWwwRootDir()
        {
            var currentDir = Directory.GetCurrentDirectory();
            string pluginsWwwRootDir = Path.Combine(currentDir, "Plugins_wwwroot");

            return pluginsWwwRootDir;
        }

        /// <summary>
        /// Plugins_wwwroot/pluginId
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public static string PluginWwwRootDir(string pluginId)
        {
            string pluginWwwRootDir = Path.Combine(PluginsWwwRootDir(), pluginId);

            return pluginWwwRootDir;
        }

        /// <summary>
        /// Plugins_wwwroot/currentPluginId
        /// </summary>
        /// <returns></returns>
        //public static string CurrentPluginWwwRootDir()
        //{
        //    string pluginId = CurrentPluginId();

        //    string pluginWwwRootDir = PluginWwwRootDir(pluginId);

        //    return pluginWwwRootDir;
        //}

        public static string PluginAdminDir()
        {
            var currentDir = Directory.GetCurrentDirectory();
            string pluginCoreAdminDir = Path.Combine(currentDir, "PluginAdmin");

            return pluginCoreAdminDir;
        }

    }
}
