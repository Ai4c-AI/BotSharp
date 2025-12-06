namespace BotSharp.Plugin.Dify.Models;

/// <summary>
/// Response model from Dify workflow execution
/// </summary>
public class DifyWorkflowResponse
{
    /// <summary>
    /// Workflow run ID
    /// </summary>
    [JsonPropertyName("workflow_run_id")]
    public string WorkflowRunId { get; set; } = string.Empty;

    /// <summary>
    /// Task ID for async workflows
    /// </summary>
    [JsonPropertyName("task_id")]
    public string? TaskId { get; set; }

    /// <summary>
    /// Workflow execution status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Output data from the workflow
    /// </summary>
    [JsonPropertyName("data")]
    public DifyWorkflowData? Data { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Workflow output data
/// </summary>
public class DifyWorkflowData
{
    /// <summary>
    /// Output data as dictionary
    /// </summary>
    [JsonPropertyName("outputs")]
    public Dictionary<string, object>? Outputs { get; set; }

    /// <summary>
    /// Raw workflow result as JSON element for flexible parsing
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
