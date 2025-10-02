namespace BotSharp.Plugin.Langfuse.Settings;

public class LangfuseSettings
{
    /// <summary>
    /// Langfuse API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://cloud.langfuse.com";

    /// <summary>
    /// Langfuse public key
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Langfuse secret key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Enable Langfuse integration
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Flush interval in seconds
    /// </summary>
    public int FlushIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Maximum batch size for events
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;
}
