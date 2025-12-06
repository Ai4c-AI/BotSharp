using BotSharp.Abstraction.Conversations;

namespace BotSharp.Plugin.LangGraph.Services;

/// <summary>
/// Maps BotSharp conversation state to LangGraph thread state
/// </summary>
public class LangGraphStateMapper
{
    private readonly IConversationStateService _conversationState;
    private readonly ILogger<LangGraphStateMapper> _logger;

    public LangGraphStateMapper(
        IConversationStateService conversationState,
        ILogger<LangGraphStateMapper> logger)
    {
        _conversationState = conversationState;
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
    public async Task SaveInterruptStateAsync(string conversationId, string interruptType, string interruptMessage)
    {
        var states = await _conversationState.GetStatesAsync(conversationId);
        states["langgraph_interrupt_type"] = interruptType;
        states["langgraph_interrupt_message"] = interruptMessage;
        states["langgraph_interrupted"] = "true";
        
        await _conversationState.SaveStateAsync(conversationId, states);
        
        _logger.LogInformation("Saved interrupt state for conversation {ConversationId}", conversationId);
    }

    /// <summary>
    /// Clear interrupt state after resumption
    /// </summary>
    public async Task ClearInterruptStateAsync(string conversationId)
    {
        var states = await _conversationState.GetStatesAsync(conversationId);
        states.Remove("langgraph_interrupt_type");
        states.Remove("langgraph_interrupt_message");
        states.Remove("langgraph_interrupted");
        
        await _conversationState.SaveStateAsync(conversationId, states);
        
        _logger.LogInformation("Cleared interrupt state for conversation {ConversationId}", conversationId);
    }

    /// <summary>
    /// Check if conversation is in interrupted state
    /// </summary>
    public async Task<bool> IsInterruptedAsync(string conversationId)
    {
        var states = await _conversationState.GetStatesAsync(conversationId);
        return states.ContainsKey("langgraph_interrupted") && states["langgraph_interrupted"] == "true";
    }

    /// <summary>
    /// Get interrupt message for display to user
    /// </summary>
    public async Task<string?> GetInterruptMessageAsync(string conversationId)
    {
        var states = await _conversationState.GetStatesAsync(conversationId);
        return states.ContainsKey("langgraph_interrupt_message") 
            ? states["langgraph_interrupt_message"] 
            : null;
    }
}
