# Quick Start Guide

This guide shows you how to get started with the BotSharp MAF integration plugin in 5 minutes.

## Prerequisites

- BotSharp installed and running
- A MAF service deployed (or use our mock service for testing)
- .NET 8.0 or later

## Step 1: Enable the Plugin

Add to your `appsettings.json`:

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

## Step 2: Create a Remote Agent

Create an agent configuration file (e.g., `agents/hr-assistant-remote.json`):

```json
{
  "id": "hr-assistant-remote",
  "name": "HR Assistant (Remote)",
  "description": "Handles employee leave requests and policy inquiries via remote MAF service",
  "type": "a2a-remote",
  "isPublic": true,
  "disabled": false,
  "profiles": ["default"],
  "templateDict": {
    "a2a_endpoint": "https://your-maf-service.azurewebsites.net"
  }
}
```

**Key Points:**
- Set `type` to `"a2a-remote"`
- Add `a2a_endpoint` in `templateDict` pointing to your MAF service
- The description will be automatically updated from the remote agent card

## Step 3: Deploy Your MAF Service

Your MAF service needs two endpoints:

### Agent Card Endpoint
```
GET https://your-maf-service.azurewebsites.net/.well-known/agent-card.json
```

Returns:
```json
{
  "name": "HR_Assistant",
  "description": "Handles employee leave requests, policy inquiries, and HR questions. Can check leave balances, book leave, and answer HR policy questions.",
  "version": "1.0.0",
  "capabilities": [
    "leave_management",
    "policy_inquiry",
    "employee_information"
  ]
}
```

### Task Execution Endpoint
```
POST https://your-maf-service.azurewebsites.net/a2a/tasks
```

Handles both `sendTask` and `getTask` methods via JSON-RPC 2.0.

## Step 4: Test the Integration

### Using BotSharp UI

1. Start BotSharp
2. Select the HR Assistant (Remote) agent
3. Send a message: "I want to book leave for next week"
4. The message will be automatically routed to your MAF service
5. You'll receive the response from the remote agent

### Using BotSharp API

```bash
curl -X POST https://your-botsharp-instance/api/conversation/send \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "hr-assistant-remote",
    "message": "What is the leave policy?"
  }'
```

## Step 5: Monitor Logs

Check BotSharp logs to see the integration in action:

```
[INFO] Loading agent card for remote A2A agent hr-assistant-remote from https://your-maf-service...
[INFO] Successfully loaded agent card for hr-assistant-remote: Name=HR_Assistant, Version=1.0.0
[INFO] Intercepting message for A2A remote agent hr-assistant-remote
[INFO] Invoking remote A2A agent hr-assistant-remote at https://your-maf-service...
[INFO] Task submitted successfully. TaskId: task_123, Status: completed
[INFO] Successfully handled message via A2A remote agent hr-assistant-remote
```

## Development: Create a Mock MAF Service

For testing, create a simple mock service:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Agent Card endpoint
app.MapGet("/.well-known/agent-card.json", () =>
{
    return Results.Json(new
    {
        name = "HR_Assistant",
        description = "Mock HR assistant for testing BotSharp MAF integration",
        version = "1.0.0-test",
        capabilities = new[] { "leave_management", "policy_inquiry" },
        metadata = new { environment = "test" }
    });
});

// Task execution endpoint
app.MapPost("/a2a/tasks", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<JsonDocument>();
    var method = request.RootElement.GetProperty("method").GetString();
    var id = request.RootElement.GetProperty("id").GetString();

    if (method == "sendTask")
    {
        var input = request.RootElement.GetProperty("params")
            .GetProperty("input").GetString();

        return Results.Json(new
        {
            jsonrpc = "2.0",
            id = id,
            result = new
            {
                taskId = Guid.NewGuid().ToString(),
                status = "completed",
                output = $"Mock response: I received your request: '{input}'. " +
                        "This is a test response from the mock MAF service."
            }
        });
    }
    else if (method == "getTask")
    {
        var taskId = request.RootElement.GetProperty("params")
            .GetProperty("taskId").GetString();

        return Results.Json(new
        {
            jsonrpc = "2.0",
            id = id,
            result = new
            {
                taskId = taskId,
                status = "completed",
                output = "Task completed successfully"
            }
        });
    }

    return Results.BadRequest("Unknown method");
});

app.Run("http://localhost:5555");
```

Run the mock service:
```bash
dotnet run
```

Update your agent configuration to point to the mock service:
```json
{
  "templateDict": {
    "a2a_endpoint": "http://localhost:5555"
  }
}
```

## Troubleshooting

### Agent not loading
- Check that the `a2a_endpoint` URL is correct and accessible
- Verify the MAF service returns a valid JSON response from `/.well-known/agent-card.json`
- Check BotSharp logs for HTTP errors

### Messages not being routed
- Ensure `AgentFramework.Enabled` is `true` in settings
- Verify the agent's `type` is `"a2a-remote"`
- Check that the agent is not `disabled`
- Review routing logs to see if the agent is being matched

### Timeout errors
- Increase `TimeoutSeconds` in settings
- Increase `MaxPollingAttempts` if tasks take longer
- Verify the MAF service is responding quickly
- Check network connectivity

### Cache issues
- Clear cache by restarting BotSharp
- Reduce `AgentCardCacheDurationMinutes` for more frequent updates
- Use forced refresh by temporarily disabling caching

## Next Steps

1. **Deploy to Production**: Move from mock service to real MAF deployment
2. **Add More Agents**: Create additional remote agents for different capabilities
3. **Monitor Performance**: Track response times and optimize polling intervals
4. **Handle Errors**: Implement fallback strategies for service unavailability
5. **Scale**: Deploy multiple MAF services and load balance

## Support

- [BotSharp Documentation](https://github.com/Ai4c-AI/BotSharp)
- [Plugin README](./README.md)
- [Examples](./Examples/)
- [Implementation Summary](./IMPLEMENTATION_SUMMARY.md)

## Example Conversation Flow

```
User: "I need to book 3 days of leave next week"
  ↓
BotSharp Router: Analyzes intent → Matches HR_Assistant
  ↓
MAF Plugin: 
  1. Fetches agent card (cached)
  2. Converts to A2A format
  3. POST /a2a/tasks (sendTask)
  ↓
MAF Service: Processes leave request
  ↓
MAF Plugin: Polls for completion (if needed)
  ↓
BotSharp: Returns response to user
  ↓
User sees: "Your leave request for 3 days next week has been submitted successfully. 
           Request ID: LR-2024-001. Your manager will be notified."
```

Congratulations! You've successfully integrated BotSharp with Microsoft Agent Framework using the A2A protocol.
