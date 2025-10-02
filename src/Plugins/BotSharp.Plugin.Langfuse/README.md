# BotSharp.Plugin.Langfuse

Langfuse integration plugin for BotSharp that provides comprehensive observability for multi-agent conversations, LLM calls, and function executions.

## Features

- **Conversation Tracking**: Automatically creates traces for each conversation
- **Agent Lifecycle Monitoring**: Tracks agent loading, routing, and utility usage
- **LLM Observability**: Monitors LLM generations with token usage statistics
- **Function Execution Tracking**: Records function calls and their parameters
- **Multi-Agent Routing**: Captures routing decisions in the BotSharp multi-agent framework
- **Hook Integration**: Seamlessly integrates with BotSharp's hook system:
  - `IContentGeneratingHook` - Tracks LLM content generation
  - `IConversationHook` - Monitors conversation lifecycle
  - `IAgentHook` - Observes agent loading and configuration

## Installation

1. Add the plugin to your BotSharp project:
   ```bash
   dotnet add package BotSharp.Plugin.Langfuse
   ```

2. Register the plugin in your `Program.cs`:
   ```csharp
   builder.Services.AddBotSharpLogger(builder.Configuration);
   ```

## Configuration

Add Langfuse settings to your `appsettings.json`:

```json
{
  "Langfuse": {
    "Enabled": true,
    "BaseUrl": "https://cloud.langfuse.com",
    "PublicKey": "your-public-key",
    "SecretKey": "your-secret-key",
    "FlushIntervalSeconds": 5,
    "MaxBatchSize": 100
  }
}
```

### Configuration Options

- `Enabled` (bool): Enable or disable Langfuse integration
- `BaseUrl` (string): Langfuse server URL (default: https://cloud.langfuse.com)
- `PublicKey` (string): Your Langfuse public API key
- `SecretKey` (string): Your Langfuse secret API key
- `FlushIntervalSeconds` (int): How often to flush events to Langfuse (default: 5 seconds)
- `MaxBatchSize` (int): Maximum number of events to batch before flushing (default: 100)

## Usage

Once configured, the plugin automatically tracks:

### Conversations
- New conversation initialization with user ID and metadata
- Message reception and response generation
- Conversation ending and cleanup

### Agents
- Agent loading and configuration
- Routing decisions between agents
- Utility and MCP tool loading

### LLM Operations
- Model generations with input/output
- Token usage statistics
- Function calls and responses

### Example Trace Structure

```
Trace: Conversation-{conversationId}
├── Span: Message-User
├── Generation: LLM-{AgentName}
│   ├── Input: conversation context
│   └── Output: agent response
├── Span: Function-Execute-{FunctionName}
│   ├── Input: function arguments
│   └── Output: function result
└── Span: Response-Generated
```

## Architecture

The plugin integrates with BotSharp through multiple hook interfaces:

1. **LangfuseConversationHook**: Tracks conversation lifecycle events
2. **LangfuseRoutingHook**: Monitors multi-agent routing decisions
3. **LangfuseContentGeneratingHook**: Observes LLM content generation
4. **LangfuseAgentHook**: Tracks agent loading and configuration

### Hook Priority

- `LangfuseRoutingHook`: Priority 5 (earlier execution)
- `LangfuseConversationHook`: Priority 10 (standard execution)

## Integration with RoutingService

The plugin fully leverages BotSharp's `RoutingService` to track:

- Current agent in routing context
- Agent transitions and handoffs
- Routing reasoning and decisions
- Function execution from routing

## Development

To extend the plugin:

1. Implement additional hooks by extending the base hook classes
2. Add custom metadata to traces via the `LangfuseService`
3. Create custom spans for specific tracking needs

## Dependencies

- `zborek.LangfuseDotnet` (>= 0.2.0): Official Langfuse .NET client
- `BotSharp.Core`: BotSharp core framework

## License

Same as BotSharp - Apache License 2.0

## Support

For issues and feature requests, please use the BotSharp GitHub repository.
