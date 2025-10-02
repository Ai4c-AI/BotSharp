# Langfuse Integration for BotSharp

## Overview

The Langfuse plugin provides comprehensive observability for BotSharp applications, enabling you to track and analyze:
- Multi-agent conversations and routing
- LLM generations with token usage
- Function executions
- Agent lifecycle events

## Quick Start

### 1. Installation

The plugin is included in the BotSharp solution. No additional installation is required.

### 2. Configuration

Add Langfuse configuration to your `appsettings.json`:

```json
{
  "Langfuse": {
    "Enabled": true,
    "BaseUrl": "https://cloud.langfuse.com",
    "PublicKey": "pk-lf-...",
    "SecretKey": "sk-lf-...",
    "FlushIntervalSeconds": 5,
    "MaxBatchSize": 100
  }
}
```

**Note**: You can obtain your Langfuse API keys from the [Langfuse Dashboard](https://cloud.langfuse.com/settings).

### 3. Enable the Plugin

The plugin is registered via the BotSharp plugin system. Ensure your `Program.cs` includes:

```csharp
builder.Services.AddBotSharpCore(builder.Configuration, options =>
{
    // Other plugins...
    options.Plugins.Add<LangfusePlugin>();
});
```

## Features

### Conversation Tracking

Every conversation automatically creates a trace in Langfuse:

```
Trace: Conversation-{id}
├── User ID: {userId}
├── Agent ID: {agentId}
├── Channel: {channel}
└── Timestamp: {createdTime}
```

### Agent Routing Observability

The plugin integrates deeply with BotSharp's `RoutingService` to track:

- **Agent Selection**: Which agent was selected to handle the user's request
- **Routing Context**: Current agent in the routing stack
- **Agent Transitions**: When control is passed from one agent to another
- **Routing Reasoning**: Why a particular agent was chosen

Example trace structure for multi-agent routing:

```
Trace: Conversation-abc123
├── Span: Routing-MessageReceived
│   └── Metadata:
│       ├── current_agent_id: router-agent
│       ├── router_agent: AI Assistant
│       └── message_role: user
├── Span: Routing-FunctionExecution-task_agent
│   └── Metadata:
│       ├── function_name: task_agent
│       ├── current_agent_id: task-agent-123
│       └── from_source: Routing
└── Span: Response-Generated
```

### LLM Generation Tracking

Token usage and model performance are automatically tracked:

```json
{
  "generation": {
    "name": "LLM-AgentName",
    "model": "gpt-4",
    "input": {
      "agent": "Customer Service Agent",
      "messages": [...]
    },
    "output": {
      "content": "...",
      "role": "assistant"
    },
    "usage": {
      "prompt_tokens": 150,
      "completion_tokens": 50,
      "total_tokens": 200
    }
  }
}
```

### Function Execution Monitoring

Function calls are logged with their arguments and execution context:

```
Span: Function-Execute-{functionName}
├── Input: function arguments
├── Metadata:
│   ├── function_name: "get_user_data"
│   ├── current_agent_id: "data-agent"
│   └── from_source: "Routing"
└── Timestamp: execution time
```

## Use Cases

### 1. Debugging Multi-Agent Conversations

When debugging complex multi-agent workflows, Langfuse helps you:
- See which agent handled each part of the conversation
- Understand why routing decisions were made
- Identify bottlenecks in agent transitions

### 2. Monitoring LLM Performance

Track and optimize LLM usage:
- Monitor token consumption per agent
- Compare model performance across agents
- Identify expensive operations

### 3. Analyzing Conversation Flow

Understand user interactions:
- View complete conversation traces
- Identify common user paths
- Detect conversation failures

## Hook Integration

The plugin leverages BotSharp's hook system through multiple interfaces:

### IConversationHook
- `OnConversationInitialized`: Creates the main trace
- `OnMessageReceived`: Logs user messages
- `OnFunctionExecuting`: Tracks function calls
- `OnResponseGenerated`: Records agent responses

### IAgentHook
- `OnAgentLoaded`: Tracks agent initialization
- `OnAgentUtilityLoaded`: Monitors utility loading
- `OnFunctionsLoaded`: Records available functions

### IContentGeneratingHook
- `BeforeGenerating`: Prepares generation context
- `AfterGenerated`: Records token usage
- `BeforeFunctionInvoked`: Logs function invocation

## Advanced Configuration

### Custom Metadata

You can extend traces with custom metadata by modifying the hooks in your application.

### Filtering Events

Control which events are logged by implementing custom hook filters:

```csharp
public class FilteredLangfuseConversationHook : LangfuseConversationHook
{
    public override async Task OnMessageReceived(RoleDialogModel message)
    {
        // Only track messages from specific channels
        if (Conversation?.Channel == "production")
        {
            await base.OnMessageReceived(message);
        }
    }
}
```

### Performance Tuning

Adjust `FlushIntervalSeconds` and `MaxBatchSize` based on your needs:

- **High-volume applications**: Increase `MaxBatchSize` and reduce `FlushIntervalSeconds`
- **Low-latency requirements**: Decrease `FlushIntervalSeconds` for near real-time data
- **Memory-constrained environments**: Reduce `MaxBatchSize`

## Troubleshooting

### Events Not Appearing in Langfuse

1. Verify your API keys are correct
2. Check that `Enabled` is set to `true`
3. Ensure the BaseUrl is accessible from your environment
4. Look for error logs related to Langfuse

### High Memory Usage

If you experience high memory usage:
1. Reduce `MaxBatchSize`
2. Decrease `FlushIntervalSeconds`
3. Implement event filtering in custom hooks

### Slow Application Performance

If the plugin impacts performance:
1. Increase `FlushIntervalSeconds` to batch more events
2. Use async event processing
3. Consider filtering less critical events

## Best Practices

1. **Use Meaningful Names**: Ensure agent and function names are descriptive
2. **Add Context**: Include relevant metadata in conversations
3. **Monitor Regularly**: Set up Langfuse dashboards to track key metrics
4. **Filter Sensitive Data**: Avoid logging PII or sensitive information
5. **Test Configuration**: Start with logging enabled in development before production

## Resources

- [Langfuse Documentation](https://langfuse.com/docs)
- [BotSharp Documentation](https://docs.botsharp.io)
- [Plugin Source Code](../../src/Plugins/BotSharp.Plugin.Langfuse)

## Support

For issues or questions:
- BotSharp: [GitHub Issues](https://github.com/Ai4c-AI/BotSharp/issues)
- Langfuse: [Langfuse Discord](https://discord.gg/langfuse)
