namespace BotSharp.Abstraction.Memory;

/// <summary>
/// Context information available after the AI model has been invoked (after call).
/// </summary>
public class InvokedContext
{
    /// <summary>
    /// The agent that was invoked
    /// </summary>
    public Agent Agent { get; set; } = null!;

    /// <summary>
    /// The request dialogs sent to the AI model
    /// </summary>
    public List<RoleDialogModel> RequestDialogs { get; set; } = new();

    /// <summary>
    /// The response from the AI model
    /// </summary>
    public RoleDialogModel Response { get; set; } = null!;

    /// <summary>
    /// The conversation ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
