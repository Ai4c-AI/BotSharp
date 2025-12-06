namespace BotSharp.Plugin.Dify.Models;

/// <summary>
/// Request model for calling Dify workflow
/// </summary>
public class DifyWorkflowRequest
{
    /// <summary>
    /// Input parameters for the workflow as key-value pairs
    /// </summary>
    [JsonPropertyName("inputs")]
    public Dictionary<string, object> Inputs { get; set; } = new();

    /// <summary>
    /// Response mode: blocking or streaming
    /// </summary>
    [JsonPropertyName("response_mode")]
    public string ResponseMode { get; set; } = "blocking";

    /// <summary>
    /// User identifier
    /// </summary>
    [JsonPropertyName("user")]
    public string User { get; set; } = "default-user";
}
