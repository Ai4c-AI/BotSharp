# BotSharp.Plugin.AgentFramework

This plugin integrates BotSharp with Microsoft Agent Framework (MAF) using the Agent2Agent (A2A) protocol based on JSON-RPC 2.0.

## Overview

The Agent Framework plugin enables BotSharp to communicate with remote MAF agents deployed on Azure App Service or other hosting platforms. This integration allows BotSharp to leverage specialized agents built with Microsoft's Agent Framework while maintaining BotSharp's routing and orchestration capabilities.

## Features

- **A2A Protocol Support**: Full implementation of Agent2Agent (A2A) protocol based on JSON-RPC 2.0
- **Agent Discovery**: Automatic discovery of remote agent capabilities via `.well-known/agent-card.json`
- **Task Management**: Support for asynchronous task submission and polling
- **Agent Card Caching**: Intelligent caching of agent metadata to reduce network overhead
- **Dynamic Routing**: Automatic agent description updates for intelligent routing decisions

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "AgentFramework": {
    "Enabled": true,
    "TimeoutSeconds": 30,
    "PollingIntervalMs": 2000,
    "MaxPollingAttempts": 60,
    "AgentCardCacheDurationMinutes": 30
  }
}
```

### Configuration Options

- **Enabled**: Enable or disable the plugin (default: false)
- **TimeoutSeconds**: HTTP request timeout in seconds (default: 30)
- **PollingIntervalMs**: Interval between task status polls in milliseconds (default: 2000)
- **MaxPollingAttempts**: Maximum polling attempts before timeout (default: 60)
- **AgentCardCacheDurationMinutes**: How long to cache agent cards in minutes (default: 30)

## Agent Configuration

To configure an agent to use MAF via A2A protocol, set the agent type to `a2a-remote` and provide the endpoint in the agent's template dictionary:

```json
{
  "id": "hr-assistant",
  "name": "HR Assistant",
  "description": "Handles employee leave requests and policy inquiries",
  "type": "a2a-remote",
  "templateDict": {
    "a2a_endpoint": "https://your-maf-service.azurewebsites.net"
  }
}
```

## How It Works

### 1. Agent Discovery

When an A2A remote agent is loaded, the plugin automatically fetches its Agent Card from the standard path:

```
GET https://your-service.com/.well-known/agent-card.json
```

The Agent Card contains:
- Agent name and description
- Version information
- Capabilities list
- Additional metadata

### 2. Message Routing

When a user message is routed to an A2A remote agent:

1. BotSharp's router matches the intent to the remote agent based on its description
2. The Agent Framework conversation hook intercepts the message
3. The conversation history is converted to A2A format
4. A task is submitted via JSON-RPC `sendTask` method

### 3. Task Execution

```
POST https://your-service.com/a2a/tasks
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "method": "sendTask",
  "id": "request-id",
  "params": {
    "input": "I want to book leave for next week",
    "context": {
      "history": [...]
    }
  }
}
```

### 4. Status Polling

If the task is not completed immediately, the plugin polls for completion:

```
POST https://your-service.com/a2a/tasks
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "method": "getTask",
  "id": "request-id",
  "params": {
    "taskId": "task_123"
  }
}
```

### 5. Response Handling

Once the task is completed, the output is returned to the user through BotSharp's conversation flow.

## Sequence Diagram

```
User → BotSharp Router → MAF Plugin → MAF Service
  ↓          ↓              ↓             ↓
"Book leave" → Intent Analysis → sendTask → Process
              ↓              ↓             ↓
              Match HR_Assistant → Poll Status → Complete
              ↓              ↓             ↓
              ← Response ← getTask ← "Leave booked"
```

## Architecture

The plugin consists of the following components:

- **AgentFrameworkPlugin**: Main plugin registration and DI setup
- **A2AClient**: HTTP client for JSON-RPC communication
- **A2AClientFactory**: Factory for creating A2A clients
- **A2ACardResolver**: Service for fetching and caching agent cards
- **A2AAgentService**: High-level service for invoking remote agents
- **AgentFrameworkHook**: Agent hook for loading agent metadata
- **AgentFrameworkConversationHook**: Conversation hook for intercepting and handling messages

## Development

### Building the Plugin

```bash
dotnet build BotSharp.Plugin.AgentFramework.csproj
```

### Testing

Create a test MAF service that implements the A2A protocol endpoints:
- `GET /.well-known/agent-card.json`
- `POST /a2a/tasks` (with `sendTask` and `getTask` methods)

Configure your test agent with the test service endpoint and test the integration.

## Best Practices

1. **Cache Management**: Adjust cache duration based on how frequently your remote agent capabilities change
2. **Timeout Configuration**: Set appropriate timeouts based on your remote agent's typical response time
3. **Error Handling**: Monitor logs for connection issues or task failures
4. **Description Updates**: Keep agent descriptions synchronized for optimal routing

## Troubleshooting

### Agent not responding

- Verify the `a2a_endpoint` is correct and accessible
- Check network connectivity and firewall rules
- Verify the remote service implements the A2A protocol correctly
- Check timeout settings are appropriate for your use case

### Routing issues

- Ensure agent description is clear and specific
- Verify agent type is set to `a2a-remote`
- Check that the agent is enabled and not disabled
- Review router logs for intent matching details

### Polling timeouts

- Increase `MaxPollingAttempts` if tasks take longer to complete
- Adjust `PollingIntervalMs` to balance between responsiveness and server load
- Consider implementing SSE (Server-Sent Events) for long-running tasks

## References

- [Agent2Agent Protocol Specification](https://github.com/microsoft/agent-framework)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/azure/ai-services/agents/)
