# AI Memory Plugin Usage Guide

## Quick Start

### 1. Register the Plugin

In your application startup (e.g., `Program.cs` or `Startup.cs`):

```csharp
using BotSharp.Plugin.AIMemory;

// Register the plugin
services.AddScoped<IAIContextProvider, ConversationMemoryProvider>();
```

Or if using plugin loader:

```csharp
var pluginLoader = new PluginLoader();
pluginLoader.Load<AIMemoryPlugin>();
```

### 2. How It Works

The memory plugin automatically integrates into the `RoutingService.InvokeAgent` method:

```
User Message → InvokingAsync (retrieve context) → AI Model Call → InvokedAsync (save memory) → Response
```

## Creating Custom Memory Providers

### Example: Long-term Memory Provider

```csharp
using BotSharp.Abstraction.AIContext;

public class LongTermMemoryProvider : AIContextProviderBase
{
    private readonly IVectorDatabase _vectorDb;
    private readonly ILogger<LongTermMemoryProvider> _logger;

    public LongTermMemoryProvider(
        IVectorDatabase vectorDb,
        ILogger<LongTermMemoryProvider> logger)
    {
        _vectorDb = vectorDb;
        _logger = logger;
    }

    public override int Priority => 10; // Execute after ConversationMemoryProvider

    public override async Task<AIContext?> InvokingAsync(InvokingContext context)
    {
        // Retrieve semantically similar past conversations
        var userMessage = context.Dialogs.LastOrDefault()?.Content;
        if (string.IsNullOrEmpty(userMessage))
            return null;

        var similarConversations = await _vectorDb.SearchAsync(
            userMessage, 
            limit: 5,
            threshold: 0.7
        );

        if (!similarConversations.Any())
            return null;

        // Create context messages from similar conversations
        var contextMessages = new List<RoleDialogModel>();
        foreach (var conv in similarConversations)
        {
            contextMessages.Add(new RoleDialogModel
            {
                Role = AgentRole.System,
                Content = $"Related past conversation: {conv.Content}"
            });
        }

        return new AIContext
        {
            ContextMessages = contextMessages,
            SystemInstruction = "Use the related past conversations to provide better context."
        };
    }

    public override async Task InvokedAsync(InvokedContext context)
    {
        // Store conversation in vector database
        var userMessage = context.RequestDialogs.LastOrDefault(d => d.Role == AgentRole.User);
        var assistantResponse = context.Response;

        if (userMessage != null && assistantResponse != null)
        {
            await _vectorDb.StoreAsync(new ConversationMemory
            {
                UserMessage = userMessage.Content,
                AssistantResponse = assistantResponse.Content,
                Timestamp = DateTime.UtcNow,
                AgentId = context.Agent.Id,
                ConversationId = context.ConversationId
            });

            _logger.LogInformation($"Stored conversation in long-term memory");
        }
    }
}
```

### Example: User Preferences Provider

```csharp
public class UserPreferencesProvider : AIContextProviderBase
{
    private readonly IUserPreferenceService _preferenceService;

    public UserPreferencesProvider(IUserPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    public override int Priority => 5;

    public override async Task<AIContext?> InvokingAsync(InvokingContext context)
    {
        // Retrieve user preferences
        var userId = context.Metadata.GetValueOrDefault("userId") as string;
        if (string.IsNullOrEmpty(userId))
            return null;

        var preferences = await _preferenceService.GetPreferencesAsync(userId);
        
        return new AIContext
        {
            SystemInstruction = $@"User preferences:
- Language: {preferences.Language}
- Tone: {preferences.PreferredTone}
- Expertise Level: {preferences.ExpertiseLevel}
Please adjust your responses accordingly."
        };
    }

    public override async Task InvokedAsync(InvokedContext context)
    {
        // Analyze response to learn user preferences
        var userId = context.Metadata.GetValueOrDefault("userId") as string;
        if (!string.IsNullOrEmpty(userId))
        {
            await _preferenceService.UpdateFromInteractionAsync(
                userId,
                context.RequestDialogs,
                context.Response
            );
        }
    }
}
```

## Priority System

Providers execute in priority order (lower numbers first):

```csharp
// Priority 0 - Session context (default)
services.AddScoped<IAIContextProvider, ConversationMemoryProvider>();

// Priority 5 - User preferences
services.AddScoped<IAIContextProvider, UserPreferencesProvider>();

// Priority 10 - Long-term memory
services.AddScoped<IAIContextProvider, LongTermMemoryProvider>();
```

## Best Practices

1. **Keep Priorities Organized**: Use consistent priority ranges for different types of providers
   - 0-9: Basic context (conversation history)
   - 10-19: User-specific context (preferences, profile)
   - 20-29: Knowledge retrieval (vector search, knowledge base)
   - 30+: Advanced context (reasoning chains, external data)

2. **Handle Errors Gracefully**: The routing service wraps provider calls in try-catch, but avoid throwing exceptions

3. **Limit Context Size**: Don't inject too many context messages (can exceed token limits)

4. **Async Operations**: Use async/await properly for database operations

5. **Log Appropriately**: Use structured logging for debugging

## Common Patterns

### Pattern: Conditional Context Injection

```csharp
public override async Task<AIContext?> InvokingAsync(InvokingContext context)
{
    // Only inject context for specific agents
    if (context.Agent.Id != "knowledge-agent")
        return null;

    // Or check metadata
    if (!context.Metadata.ContainsKey("enable_memory"))
        return null;

    // Inject context...
}
```

### Pattern: Context Summarization

```csharp
public override async Task<AIContext?> InvokingAsync(InvokingContext context)
{
    // If conversation is long, summarize old messages
    if (context.Dialogs.Count > 20)
    {
        var oldDialogs = context.Dialogs.Take(10).ToList();
        var summary = await SummarizeDialogsAsync(oldDialogs);
        
        return new AIContext
        {
            ContextMessages = new List<RoleDialogModel>
            {
                new RoleDialogModel
                {
                    Role = AgentRole.System,
                    Content = $"Previous conversation summary: {summary}"
                }
            }
        };
    }
    
    return null;
}
```

## Troubleshooting

### Provider Not Being Called

1. Check that provider is registered in DI container
2. Verify the agent is being invoked through RoutingService.InvokeAgent
3. Check priority order

### Context Not Appearing in Model

1. Verify AIContext is being returned (not null)
2. Check that ContextMessages list is populated
3. Ensure messages are being inserted correctly

### Performance Issues

1. Limit the number of context messages
2. Use caching for expensive operations
3. Consider async/parallel processing for multiple providers
