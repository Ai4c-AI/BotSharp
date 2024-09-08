namespace BotSharp.Core.Plugins.Models;

public class PluginRegistryResponseModel : PluginInfoModel
{
    public string DownloadUrl { get; set; }

    /// <summary>
    /// The value of this property is assigned according to the local plug-in after it is obtained
    /// </summary>
    public PluginStatus Status { get; set; }

    public enum PluginStatus
    {
        /// <summary>
        /// Plugins that don't have this PluginId natively
        /// </summary>
        LocalWithout,

        /// <summary>
        /// Plugins that already have this PluginId locally
        /// </summary>
        LocalExist
    }
}