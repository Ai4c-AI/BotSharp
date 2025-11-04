namespace BotSharp.Abstraction.AIContext;

/// <summary>
/// Context information available when invoking the AI model (before call).
/// </summary>
public class InvokingContext
{
    /// <summary>
    /// The agent being invoked
    /// </summary>
    public Agent Agent { get; set; } = null!;

    /// <summary>
    /// The conversation dialogs
    /// </summary>
    public List<RoleDialogModel> Dialogs { get; set; } = new();

    /// <summary>
    /// The conversation ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
