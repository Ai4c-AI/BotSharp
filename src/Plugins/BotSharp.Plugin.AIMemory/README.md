# BotSharp AI Memory Plugin

This plugin provides AI memory management capabilities for BotSharp, inspired by Microsoft Agent Framework's AIContextProvider pattern.

## Overview

The AI Memory Plugin implements a memory system that manages conversation context before and after AI model invocations. This enables agents to maintain better context awareness and provide more coherent responses.

## Architecture

### Core Components

1. **IAIContextProvider Interface** - Base abstraction for context providers
2. **AIContextProviderBase** - Base implementation class
3. **ConversationMemoryProvider** - Default memory provider implementation

### Context Models

- **InvokingContext** - Context available before AI model invocation
- **InvokedContext** - Context available after AI model invocation  
- **AIContext** - Context data to inject into AI requests

## How It Works

### Lifecycle

The memory system operates through two key lifecycle hooks:

#### 1. InvokingAsync (Before AI Call)
- Called before the AI model is invoked
- Retrieves relevant context from memory
- Can inject additional messages into the conversation
- Allows for memory retrieval, knowledge base queries, etc.

```csharp
public override async Task<AIContext?> InvokingAsync(InvokingContext context)
{
    // Retrieve relevant context
    // Return AIContext to inject into the request
}
```

#### 2. InvokedAsync (After AI Call)
- Called after the AI model responds
- Saves the interaction to memory
- Enables memory consolidation and learning

```csharp
public override async Task InvokedAsync(InvokedContext context)
{
    // Save the interaction to memory
    // Update embeddings, preferences, etc.
}
```

## Integration

The memory plugin is integrated into `RoutingService.InvokeAgent` method:

1. Before calling the AI model:
   - All registered `IAIContextProvider` instances are called in priority order
   - Context from providers is collected and injected into the request

2. After the AI model responds:
   - All providers' `InvokedAsync` methods are called
   - Response is saved to memory systems

## Usage

### Registering the Plugin

The plugin is registered in your application's service collection:

```csharp
services.AddScoped<IAIContextProvider, ConversationMemoryProvider>();
```

### Creating Custom Memory Providers

To create a custom memory provider:

```csharp
public class MyCustomMemoryProvider : AIContextProviderBase
{
    public override int Priority => 10; // Higher priority = executes later
    
    public override async Task<AIContext?> InvokingAsync(InvokingContext context)
    {
        // Your custom logic to retrieve context
        return new AIContext
        {
            ContextMessages = relevantMessages,
            SystemInstruction = "Additional instructions..."
        };
    }
    
    public override async Task InvokedAsync(InvokedContext context)
    {
        // Your custom logic to save memory
    }
}
```

Then register it:

```csharp
services.AddScoped<IAIContextProvider, MyCustomMemoryProvider>();
```

## Future Enhancements

Potential enhancements for the memory system:

1. **Long-term Memory Storage** - Persist conversations to database
2. **Vector Embeddings** - Store and retrieve semantically similar conversations
3. **Memory Summarization** - Automatically summarize old conversations
4. **User Preferences** - Learn and store user preferences
5. **Knowledge Graphs** - Build relationships between concepts
6. **Forgetting Mechanisms** - Implement memory decay for irrelevant information

## Example Use Cases

1. **Conversation Continuity** - Maintain context across sessions
2. **Personalization** - Remember user preferences and history
3. **Knowledge Retrieval** - Inject relevant information from knowledge base
4. **Learning** - Improve responses based on past interactions
5. **Multi-turn Reasoning** - Maintain complex reasoning chains

## Plugin Information

- **Plugin ID**: `8c6f9e42-5a3b-4d1e-9f2a-7b8c9d0e1f2a`
- **Plugin Name**: AI Memory Plugin
- **Description**: Provides AI memory management similar to Microsoft Agent Framework's AIContextProvider pattern
