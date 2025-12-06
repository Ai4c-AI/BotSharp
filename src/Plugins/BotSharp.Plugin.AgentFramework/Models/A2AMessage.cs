using System.Text.Json.Serialization;

namespace BotSharp.Plugin.AgentFramework.Models;

/// <summary>
/// JSON-RPC 2.0 Request format for A2A protocol
/// </summary>
public class A2ARequest
{
    /// <summary>
    /// JSON-RPC version, always "2.0"
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    /// <summary>
    /// Request ID for matching responses
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Method name (e.g., "sendTask", "getTask", "sendTaskSubscribe")
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Method parameters
    /// </summary>
    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 Response format for A2A protocol
/// </summary>
public class A2AResponse<T>
{
    /// <summary>
    /// JSON-RPC version, always "2.0"
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    /// <summary>
    /// Request ID that this response corresponds to
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Result object if successful
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; set; }

    /// <summary>
    /// Error object if failed
    /// </summary>
    [JsonPropertyName("error")]
    public A2AError? Error { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 Error object
/// </summary>
public class A2AError
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error data
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
