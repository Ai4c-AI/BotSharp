# BotSharp Agent Protocol (A2A) Plugin

## Overview

The Agent Protocol plugin enables BotSharp to act as an **Agent-to-Agent (A2A) client**, facilitating communication with remote agents through standardized protocols. This plugin implements the core requirements for:

1. **A2A Client Communication**: Router Agent can invoke remote agents through A2AClient instead of direct HTTP calls
2. **Semantic Routing**: Dynamic agent discovery through Agent Registry with capability-based matching
3. **Bidirectional Communication**: Support for async callbacks and push notifications for long-running tasks

## Features

### 1. A2A Client

The A2A Client (`IA2AClient`) enables BotSharp to communicate with remote agents:

- **Agent Card Resolution**: Automatically fetches and parses agent capabilities from remote endpoints
- **Intelligent Invocation**: Sends requests to remote agents with full conversation context
- **Async Support**: Handles long-running operations with callback mechanisms
- **Progress Tracking**: Monitors progress of remote agent operations

### 2. Agent Registry

The Agent Registry (`IAgentRegistry`) provides semantic routing capabilities:

- **Semantic Query**: Query agents using natural language (e.g., "who handles tax calculations?")
- **Capability Matching**: Find agents based on required capabilities
- **Confidence Scoring**: Ranks agents by relevance to the query
- **Local & Remote**: Supports both local agent cache and remote registry endpoints

### 3. Bidirectional Communication

Support for push notifications and async callbacks:

- **Async Callbacks**: Remote agents can send responses asynchronously
- **Progress Notifications**: Receive progress updates during long-running operations
- **Push Notifications**: Remote agents can proactively send updates to BotSharp

## Architecture

### Components

```
BotSharp.Plugin.AgentProtocol/
├── Services/
│   ├── IA2AClient.cs               # Interface for A2A client
│   ├── A2AClient.cs                # Implementation of A2A client
│   ├── IA2ACardResolver.cs         # Interface for card resolution
│   ├── A2ACardResolver.cs          # Parses agent capabilities
│   ├── IAgentRegistry.cs           # Interface for agent registry
│   └── AgentRegistry.cs            # Semantic routing implementation
├── Models/
│   ├── A2AAgentCard.cs             # Agent capability card model
│   ├── A2AInvokeModels.cs          # Request/response models
│   └── AgentRegistryModels.cs      # Registry query models
├── Hooks/
│   └── A2AAgentHook.cs             # Integration with routing system
├── Functions/
│   ├── QueryAgentRegistryFn.cs     # Function for semantic agent search
│   └── InvokeA2AAgentFn.cs         # Function for remote agent invocation
├── Settings/
│   └── AgentProtocolSettings.cs    # Plugin configuration
└── AgentProtocolPlugin.cs          # Plugin registration
```

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "AgentProtocol": {
    "EnableA2AClient": true,
    "RegistryEndpoint": "https://your-registry-endpoint/api",
    "TimeoutSeconds": 300,
    "EnableAsyncCallbacks": true,
    "CallbackEndpoint": "https://your-botsharp-instance/api/a2a/callback",
    "EnableProgressNotifications": true,
    "RemoteAgents": {
      "tax-agent-id": "https://tax-agent.example.com",
      "data-analysis-agent-id": "https://analytics.example.com"
    }
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `EnableA2AClient` | bool | true | Enable A2A client functionality |
| `RegistryEndpoint` | string | null | Agent registry endpoint URL for semantic routing |
| `TimeoutSeconds` | int | 300 | Timeout for A2A operations in seconds |
| `EnableAsyncCallbacks` | bool | true | Enable async callbacks from remote agents |
| `CallbackEndpoint` | string | null | Callback endpoint URL for receiving push notifications |
| `EnableProgressNotifications` | bool | true | Enable progress notifications for long-running tasks |
| `RemoteAgents` | dictionary | {} | Map of agent IDs to their endpoint URLs |

## Usage

### 1. Querying the Agent Registry

Use semantic queries to find capable agents:

```csharp
var registry = serviceProvider.GetRequiredService<IAgentRegistry>();

var query = new AgentRegistryQuery
{
    Query = "who handles tax calculations?",
    RequiredCapabilities = new List<string> { "tax-calculation" },
    MaxResults = 5
};

var result = await registry.QueryAgentsAsync(query);

foreach (var agent in result.Agents)
{
    var confidence = result.ConfidenceScores[agent.Id];
    Console.WriteLine($"Agent: {agent.Name}, Confidence: {confidence:P}");
}
```

### 2. Invoking Remote Agents

Invoke a remote agent with conversation context:

```csharp
var a2aClient = serviceProvider.GetRequiredService<IA2AClient>();

var request = new A2AInvokeRequest
{
    AgentId = "tax-agent-id",
    Parameters = new Dictionary<string, object>
    {
        ["income"] = 50000,
        ["deductions"] = 10000
    },
    Context = dialogs // Current conversation context
};

var response = await a2aClient.InvokeAgentAsync(request);

if (response.Status == A2AResponseStatus.Success)
{
    Console.WriteLine($"Response: {response.Response.Content}");
}
else if (response.IsAsync)
{
    Console.WriteLine($"Processing asynchronously. RequestId: {response.RequestId}");
}
```

### 3. Handling Async Callbacks

Register callbacks for async operations:

```csharp
// Register completion callback
a2aClient.RegisterCallback(requestId, (response) =>
{
    Console.WriteLine($"Async operation completed: {response.Response.Content}");
});

// Register progress callback
a2aClient.RegisterProgressCallback(requestId, (notification) =>
{
    Console.WriteLine($"Progress: {notification.Progress}% - {notification.Message}");
    
    if (notification.IsCompleted)
    {
        Console.WriteLine($"Final result: {notification.Result.Content}");
    }
});
```

### 4. Retrieving Agent Capabilities

Fetch agent card to understand capabilities:

```csharp
var cardResolver = serviceProvider.GetRequiredService<IA2ACardResolver>();

var card = await cardResolver.ResolveCardAsync("https://remote-agent.example.com");

Console.WriteLine($"Agent: {card.Name}");
Console.WriteLine($"Description: {card.Description}");
Console.WriteLine("Capabilities:");
foreach (var capability in card.Capabilities)
{
    Console.WriteLine($"  - {capability}");
}
```

## Integration with Routing

The plugin automatically integrates with BotSharp's routing system through `A2AAgentHook`:

1. **Agent Discovery**: When routing instructions are loaded, the hook queries the agent registry and adds discovered A2A agents to the list of routable agents
2. **Transparent Invocation**: When a remote A2A agent is selected by the router, the hook automatically uses A2AClient to invoke it
3. **Fallback**: If A2A invocation fails, the system falls back to standard routing

## Functions

The plugin provides two functions that can be called by agents:

### `a2a-query_agent_registry`

Query the agent registry for capable agents:

**Parameters:**
- `Query` (string): Natural language query
- `RequiredCapabilities` (array): List of required capabilities
- `MaxResults` (int): Maximum number of results

**Example:**
```json
{
  "Query": "who can process invoices?",
  "RequiredCapabilities": ["invoice-processing"],
  "MaxResults": 3
}
```

### `a2a-invoke_remote_agent`

Invoke a remote A2A agent:

**Parameters:**
- `AgentId` (string): Target agent ID
- `Parameters` (object): Agent-specific parameters
- `Context` (array): Conversation context (optional)

**Example:**
```json
{
  "AgentId": "invoice-processing-agent",
  "Parameters": {
    "invoice_file": "invoice-2024-001.pdf",
    "action": "extract_data"
  }
}
```

## Remote Agent Protocol

### Agent Card Endpoint

Remote agents should expose an agent card at `/a2a/card`:

```json
{
  "Id": "tax-calculation-agent",
  "Name": "Tax Calculator",
  "Description": "Calculates taxes based on income and deductions",
  "Capabilities": ["tax-calculation", "tax-planning"],
  "Endpoint": "https://tax-agent.example.com",
  "SupportedProtocols": ["A2A/1.0"],
  "RequiredParameters": [
    {
      "Name": "income",
      "Description": "Annual income",
      "Type": "number",
      "Required": true
    }
  ],
  "OptionalParameters": [
    {
      "Name": "deductions",
      "Description": "Tax deductions",
      "Type": "number",
      "Required": false
    }
  ]
}
```

### Invocation Endpoint

Remote agents should accept invocations at `/a2a/invoke`:

**Request:**
```json
{
  "AgentId": "tax-calculation-agent",
  "Parameters": {
    "income": 50000,
    "deductions": 10000
  },
  "Context": [
    {
      "Role": "user",
      "Content": "Calculate my taxes"
    }
  ],
  "CallbackUrl": "https://botsharp.example.com/api/a2a/callback"
}
```

**Response:**
```json
{
  "RequestId": "req-12345",
  "Status": "Success",
  "Response": {
    "Role": "assistant",
    "Content": "Your estimated tax is $8,000"
  },
  "IsAsync": false
}
```

### Progress Notification

For long-running operations, remote agents can send progress updates:

**POST to CallbackUrl:**
```json
{
  "RequestId": "req-12345",
  "Progress": 50,
  "Message": "Processing data...",
  "IsCompleted": false
}
```

**Final notification:**
```json
{
  "RequestId": "req-12345",
  "Progress": 100,
  "Message": "Analysis complete",
  "IsCompleted": true,
  "Result": {
    "Role": "assistant",
    "Content": "Analysis results: ..."
  }
}
```

## Benefits

1. **Dynamic Routing**: Router can discover and connect to new agents without code changes
2. **Scalability**: Distribute agent capabilities across multiple services
3. **Async Operations**: Handle long-running tasks without blocking
4. **Decoupling**: Agents can be developed and deployed independently
5. **Semantic Discovery**: Find agents based on capabilities, not just names

## Example Scenario

**User Request**: "Calculate taxes for my business income"

**Flow:**
1. Router Agent receives the request
2. A2AAgentHook queries the Agent Registry: "who handles tax calculations?"
3. Registry returns matching agents with confidence scores
4. Router selects the best agent based on confidence and context
5. A2AClient invokes the remote tax calculation agent
6. If the calculation takes time:
   - Agent returns async response with RequestId
   - Progress notifications are sent periodically
   - Final result is pushed via callback
7. Router receives the result and responds to the user

## License

This plugin is part of BotSharp and follows the same license terms.
