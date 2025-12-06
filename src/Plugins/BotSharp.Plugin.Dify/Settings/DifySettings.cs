namespace BotSharp.Plugin.Dify.Settings;

/// <summary>
/// Configuration settings for Dify API integration
/// </summary>
public class DifySettings
{
    /// <summary>
    /// Base URL for Dify API (e.g., https://api.dify.ai)
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.dify.ai";

    /// <summary>
    /// API key for Dify authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Polling interval in seconds for checking workflow status
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum polling attempts before giving up
    /// </summary>
    public int MaxPollingAttempts { get; set; } = 180;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
}
