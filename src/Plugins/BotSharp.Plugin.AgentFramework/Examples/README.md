# MAF Integration Examples

This directory contains example configurations and usage patterns for the BotSharp MAF integration plugin.

## Files

- **appsettings.example.json**: Example application settings configuration
- **agent-config.example.json**: Example agent configuration for a remote MAF agent
- **agent-card.example.json**: Example agent card that would be returned from a MAF service

## Quick Start

### 1. Configure the Plugin

Add the configuration to your `appsettings.json`:

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

### 2. Create a Remote Agent Configuration

Create an agent configuration file (e.g., `agents/hr-assistant.json`):

```json
{
  "id": "hr-assistant-remote",
  "name": "HR Assistant (Remote)",
  "description": "Handles employee leave requests and policy inquiries",
  "type": "a2a-remote",
  "templateDict": {
    "a2a_endpoint": "https://your-maf-service.azurewebsites.net"
  }
}
```

### 3. Deploy MAF Service

Your MAF service should expose the following endpoints:

#### Agent Discovery Endpoint
```
GET https://your-maf-service.azurewebsites.net/.well-known/agent-card.json

Response:
{
  "name": "HR_Assistant",
  "description": "Handles employee leave requests...",
  "version": "1.0.0",
  "capabilities": ["leave_management", "policy_inquiry"]
}
```

#### Task Execution Endpoint
```
POST https://your-maf-service.azurewebsites.net/a2a/tasks
Content-Type: application/json

Request (sendTask):
{
  "jsonrpc": "2.0",
  "method": "sendTask",
  "id": "unique-request-id",
  "params": {
    "input": "I want to book leave for next week",
    "context": {
      "history": [...]
    }
  }
}

Response:
{
  "jsonrpc": "2.0",
  "id": "unique-request-id",
  "result": {
    "taskId": "task_123",
    "status": "completed",
    "output": "Your leave request has been submitted successfully."
  }
}
```

#### Task Status Endpoint
```
POST https://your-maf-service.azurewebsites.net/a2a/tasks
Content-Type: application/json

Request (getTask):
{
  "jsonrpc": "2.0",
  "method": "getTask",
  "id": "unique-request-id",
  "params": {
    "taskId": "task_123"
  }
}

Response:
{
  "jsonrpc": "2.0",
  "id": "unique-request-id",
  "result": {
    "taskId": "task_123",
    "status": "completed",
    "output": "Your leave request has been submitted successfully."
  }
}
```

## Testing

### Manual Testing with curl

#### Test Agent Card Discovery
```bash
curl https://your-maf-service.azurewebsites.net/.well-known/agent-card.json
```

#### Test Task Submission
```bash
curl -X POST https://your-maf-service.azurewebsites.net/a2a/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "sendTask",
    "id": "test-1",
    "params": {
      "input": "What is the leave policy?"
    }
  }'
```

#### Test Task Status
```bash
curl -X POST https://your-maf-service.azurewebsites.net/a2a/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "getTask",
    "id": "test-2",
    "params": {
      "taskId": "task_123"
    }
  }'
```

## Integration Flow

1. **Agent Loading**: When BotSharp loads an agent with type "a2a-remote":
   - The plugin fetches the agent card from `/.well-known/agent-card.json`
   - Updates the agent description for routing purposes
   - Caches the agent card metadata

2. **Message Routing**: When a user message is routed to an A2A agent:
   - The conversation hook intercepts the message
   - Converts conversation history to A2A format
   - Submits task via `sendTask` method

3. **Response Handling**:
   - If task completes immediately, returns the output
   - If task is queued/running, polls using `getTask` method
   - Returns the final output to the user

## Troubleshooting

### Agent not loading
- Check that `a2a_endpoint` is set in agent's `templateDict`
- Verify the endpoint is accessible and returns a valid agent card
- Check logs for HTTP errors or network issues

### Messages not being intercepted
- Ensure `AgentFramework.Enabled` is set to `true`
- Verify agent `type` is set to `"a2a-remote"`
- Check that the agent is not disabled

### Polling timeouts
- Increase `MaxPollingAttempts` in settings
- Verify the MAF service is returning task status updates
- Check network latency between BotSharp and MAF service

## Development Notes

### Creating a Mock MAF Service

For development and testing, you can create a simple mock MAF service using ASP.NET Core:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Agent Card endpoint
app.MapGet("/.well-known/agent-card.json", () => 
{
    return Results.Json(new
    {
        name = "Test_Agent",
        description = "Test agent for development",
        version = "1.0.0",
        capabilities = new[] { "test" }
    });
});

// Task endpoint
app.MapPost("/a2a/tasks", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<JsonDocument>();
    var method = request.RootElement.GetProperty("method").GetString();
    
    if (method == "sendTask")
    {
        return Results.Json(new
        {
            jsonrpc = "2.0",
            id = request.RootElement.GetProperty("id").GetString(),
            result = new
            {
                taskId = Guid.NewGuid().ToString(),
                status = "completed",
                output = "Mock response from MAF service"
            }
        });
    }
    
    return Results.BadRequest();
});

app.Run();
```

### Testing with Postman

Import the example requests from this directory into Postman to test your MAF service endpoints before integrating with BotSharp.

## Additional Resources

- [BotSharp Documentation](https://github.com/Ai4c-AI/BotSharp)
- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)
