using BotSharp.Abstraction.Conversations;

namespace BotSharp.Plugin.LangGraph.Services;

/// <summary>
/// Maps BotSharp conversation state to LangGraph thread state
/// </summary>
public class LangGraphStateMapper
{
    private readonly ILogger<LangGraphStateMapper> _logger;

    public LangGraphStateMapper(ILogger<LangGraphStateMapper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get or create thread_id for a conversation
    /// </summary>
    public string GetThreadId(string conversationId)
    {
        return $"botsharp_conversation_{conversationId}";
    }

    /// <summary>
    /// Save interrupt state for later resumption
    /// </summary>
    public void SaveInterruptState(IConversationStateService conversationState, string interruptType, string interruptMessage)
    {
        conversationState.SetState("langgraph_interrupt_type", interruptType);
        conversationState.SetState("langgraph_interrupt_message", interruptMessage);
        conversationState.SetState("langgraph_interrupted", "true");
        conversationState.Save();
        
        _logger.LogInformation("Saved interrupt state for conversation {ConversationId}", conversationState.GetConversationId());
    }

    /// <summary>
    /// Clear interrupt state after resumption
    /// </summary>
    public void ClearInterruptState(IConversationStateService conversationState)
    {
        conversationState.RemoveState("langgraph_interrupt_type");
        conversationState.RemoveState("langgraph_interrupt_message");
        conversationState.RemoveState("langgraph_interrupted");
        conversationState.Save();
        
        _logger.LogInformation("Cleared interrupt state for conversation {ConversationId}", conversationState.GetConversationId());
    }

    /// <summary>
    /// Check if conversation is in interrupted state
    /// </summary>
    public bool IsInterrupted(IConversationStateService conversationState)
    {
        return conversationState.ContainsState("langgraph_interrupted") && 
               conversationState.GetState("langgraph_interrupted") == "true";
    }

    /// <summary>
    /// Get interrupt message for display to user
    /// </summary>
    public string? GetInterruptMessage(IConversationStateService conversationState)
    {
        return conversationState.ContainsState("langgraph_interrupt_message") 
            ? conversationState.GetState("langgraph_interrupt_message") 
            : null;
    }
}
