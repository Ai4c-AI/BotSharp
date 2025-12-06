namespace BotSharp.Plugin.MicrosoftAgentFramework.Settings;

/// <summary>
/// Configuration settings for Microsoft Agent Framework integration
/// </summary>
public class MicrosoftAgentFrameworkSettings
{
    /// <summary>
    /// Enable or disable MAF plugin
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// OpenAI API endpoint for MAF agents
    /// </summary>
    public string? OpenAIEndpoint { get; set; }

    /// <summary>
    /// OpenAI API key for MAF agents
    /// </summary>
    public string? OpenAIApiKey { get; set; }

    /// <summary>
    /// Default model to use for MAF agents
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4";

    /// <summary>
    /// Enable state serialization for long-running workflows
    /// </summary>
    public bool EnableStateSerialization { get; set; } = true;

    /// <summary>
    /// Timeout for workflow execution in seconds
    /// </summary>
    public int WorkflowTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Enable detailed logging of MAF operations
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
