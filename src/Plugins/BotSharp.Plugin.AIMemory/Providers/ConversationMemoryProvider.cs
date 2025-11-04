using BotSharp.Abstraction.Agents.Enums;
using BotSharp.Abstraction.AIContext;
using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BotSharp.Plugin.AIMemory.Providers;

/// <summary>
/// Conversation Memory Provider that retrieves and stores conversation context.
/// This is a sample implementation that demonstrates the AIContextProvider pattern.
/// </summary>
public class ConversationMemoryProvider : AIContextProviderBase
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ConversationMemoryProvider> _logger;

    public ConversationMemoryProvider(
        IServiceProvider services,
        ILogger<ConversationMemoryProvider> logger)
    {
        _services = services;
        _logger = logger;
    }

    /// <summary>
    /// Priority for execution. Lower values execute first.
    /// This provider has medium priority (0).
    /// </summary>
    public override int Priority => 0;

    /// <summary>
    /// Called before AI model invocation to provide memory context.
    /// Retrieves relevant conversation history and context to inject into the request.
    /// </summary>
    public override async Task<AIContext?> InvokingAsync(InvokingContext context)
    {
        try
        {
            _logger.LogDebug($"ConversationMemoryProvider.InvokingAsync called for agent {context.Agent.Name}");

            // Get conversation storage
            var storage = _services.GetRequiredService<IConversationStorage>();
            
            // Get recent conversation history (e.g., last 5 messages before current)
            var recentDialogs = storage.GetDialogs(context.ConversationId)
                .Where(d => d.Role != AgentRole.Function) // Filter out function calls
                .TakeLast(10) // Get last 10 messages
                .ToList();

            // If we have relevant context, create AIContext
            if (recentDialogs.Any())
            {
                _logger.LogDebug($"Retrieved {recentDialogs.Count} messages from conversation memory");

                var aiContext = new AIContext
                {
                    SystemInstruction = "Use the conversation history to maintain context and provide relevant responses."
                };

                // Note: We're not adding context messages here as the dialogs are already in the main dialog list
                // This is just a demonstration. In a real implementation, you might:
                // 1. Retrieve from long-term memory storage
                // 2. Summarize old conversations
                // 3. Retrieve relevant knowledge from vector database
                // 4. Add user preferences or profile information

                return aiContext;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ConversationMemoryProvider.InvokingAsync");
            return null;
        }
    }

    /// <summary>
    /// Called after AI model invocation to save the interaction to memory.
    /// This allows the conversation to become part of future memory retrieval.
    /// </summary>
    public override async Task InvokedAsync(InvokedContext context)
    {
        try
        {
            _logger.LogDebug($"ConversationMemoryProvider.InvokedAsync called for agent {context.Agent.Name}");

            // Get conversation storage
            var storage = _services.GetRequiredService<IConversationStorage>();

            // Save the response to conversation storage (if not already saved)
            // In this implementation, the response is already saved by the conversation service,
            // but this hook allows you to:
            // 1. Store in long-term memory database
            // 2. Update vector embeddings
            // 3. Extract and store key information
            // 4. Update user preferences
            // 5. Trigger memory consolidation processes

            // Example: Log memory save operation
            _logger.LogInformation(
                $"Memory saved for conversation {context.ConversationId}: " +
                $"User message -> {context.Response.Role} response " +
                $"(Agent: {context.Agent.Name})"
            );

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ConversationMemoryProvider.InvokedAsync");
        }
    }
}
