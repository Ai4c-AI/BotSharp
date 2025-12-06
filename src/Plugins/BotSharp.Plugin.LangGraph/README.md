# BotSharp.Plugin.LangGraph

This plugin enables BotSharp to integrate with LangGraph/LangServe for stateful graph-based agent orchestration.

## Overview

The LangGraph plugin bridges the gap between BotSharp (C#) and LangGraph (Python), providing:

- **State Persistence**: Automatic mapping of BotSharp `ConversationId` to LangGraph `thread_id`
- **Streaming Support**: Real-time response streaming for long-running operations
- **Human-in-the-Loop**: Support for LangGraph's interrupt/resume mechanism
- **OpenTelemetry Integration**: Distributed tracing support

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "LangGraph": {
    "BaseUrl": "http://localhost:8000",
    "ApiKey": "your-api-key-here",
    "Timeout": 300,
    "EnableTracing": true,
    "DefaultAgentEndpoint": "research-agent"
  }
}
```

### Configuration Options

- `BaseUrl`: The base URL of your LangServe instance (required)
- `ApiKey`: Optional API key for authentication
- `Timeout`: Request timeout in seconds (default: 300)
- `EnableTracing`: Enable OpenTelemetry tracing (default: true)
- `DefaultAgentEndpoint`: Default agent endpoint if not specified

## Architecture

### State Mapping

The plugin automatically maps BotSharp conversation IDs to LangGraph thread IDs using the format:

```
botsharp_conversation_{conversationId}
```

This 1:1 mapping ensures that LangGraph's checkpointer (PostgreSQL, Redis, etc.) maintains state across conversation turns.

### API Endpoints

The plugin supports three LangServe endpoints:

1. `/invoke` - Synchronous invocation
2. `/stream` - Server-Sent Events (SSE) streaming
3. `/batch` - Batch processing (coming soon)

### Request Format

All requests to LangServe follow this structure:

```json
{
  "input": {
    "messages": [
      {
        "type": "human",
        "content": "Your message here"
      }
    ]
  },
  "config": {
    "configurable": {
      "thread_id": "botsharp_conversation_abc123"
    }
  }
}
```

## Usage

### Basic Invocation

```csharp
var client = serviceProvider.GetRequiredService<LangServeClient>();
var response = await client.InvokeAsync(
    agentEndpoint: "research-agent",
    conversationId: "abc123",
    userMessage: "Analyze the market trends"
);
```

### Streaming

```csharp
var client = serviceProvider.GetRequiredService<LangServeClient>();
await foreach (var chunk in client.StreamAsync(
    agentEndpoint: "research-agent",
    conversationId: "abc123",
    userMessage: "Generate a detailed report"
))
{
    Console.Write(chunk);
}
```

### Human-in-the-Loop

```csharp
var client = serviceProvider.GetRequiredService<LangServeClient>();
var stateMapper = serviceProvider.GetRequiredService<LangGraphStateMapper>();

// Initial invocation
var response = await client.InvokeAsync(
    agentEndpoint: "approval-agent",
    conversationId: conversationId,
    userMessage: "Execute the trade"
);

// Check for interrupt
if (client.IsInterrupted(response))
{
    // Save interrupt state
    stateMapper.SaveInterruptState(
        conversationState,
        response.Interrupt.Type,
        response.Interrupt.Message
    );
    
    // Show message to user and wait for approval
    Console.WriteLine($"Approval required: {response.Interrupt.Message}");
    var userApproval = Console.ReadLine();
    
    // Resume execution
    var resumeResponse = await client.ResumeAsync(
        agentEndpoint: "approval-agent",
        conversationId: conversationId,
        userResponse: userApproval
    );
    
    // Clear interrupt state
    stateMapper.ClearInterruptState(conversationState);
}
```

## Security Considerations

1. **API Keys**: Store API keys securely in configuration, never in code
2. **HTTPS**: Always use HTTPS in production for `BaseUrl`
3. **Timeout**: Set appropriate timeouts to prevent resource exhaustion
4. **Input Validation**: User messages are JSON-serialized automatically

## Troubleshooting

### Connection Refused

Ensure your LangServe instance is running:

```bash
curl http://localhost:8000/health
```

### State Not Persisting

Verify that your LangGraph application has a checkpointer configured:

```python
# In your LangGraph app
from langgraph.checkpoint.postgres import PostgresSaver

checkpointer = PostgresSaver.from_conn_string(conn_string)
app = graph.compile(checkpointer=checkpointer)
```

### Timeout Errors

For long-running operations:
1. Increase the `Timeout` setting
2. Use streaming instead of synchronous invocation
3. Consider using LangGraph's async checkpointing

## Further Reading

- See [QUICKSTART.md](/QUICKSTART.md) for detailed architecture documentation
- [LangGraph Documentation](https://python.langchain.com/docs/langgraph)
- [LangServe Documentation](https://python.langchain.com/docs/langserve)
