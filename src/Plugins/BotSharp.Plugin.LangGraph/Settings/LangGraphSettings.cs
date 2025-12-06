namespace BotSharp.Plugin.LangGraph.Settings;

public class LangGraphSettings
{
    /// <summary>
    /// Base URL of the LangServe API endpoint
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:8000";

    /// <summary>
    /// API key for authentication (if required)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int Timeout { get; set; } = 300;

    /// <summary>
    /// Enable OpenTelemetry tracing
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Default agent endpoint (if not specified in agent config)
    /// </summary>
    public string? DefaultAgentEndpoint { get; set; }
}
