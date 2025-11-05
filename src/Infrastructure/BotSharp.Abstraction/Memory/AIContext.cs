namespace BotSharp.Abstraction.Memory;

/// <summary>
/// AI Context that can be injected into the AI model request.
/// Contains additional context information like memory, knowledge, etc.
/// </summary>
public class AIContext
{
    /// <summary>
    /// Context messages to be injected into the conversation
    /// </summary>
    public List<RoleDialogModel> ContextMessages { get; set; } = new();

    /// <summary>
    /// System instructions to be added
    /// </summary>
    public string? SystemInstruction { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
