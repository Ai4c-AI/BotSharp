# Microsoft Agent Framework Integration Guide

This guide explains how to integrate and use the Microsoft Agent Framework plugin with BotSharp to implement a layered governance architecture.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [Creating MAF-Enabled Agents](#creating-maf-enabled-agents)
5. [Using the Integration](#using-the-integration)
6. [Advanced Topics](#advanced-topics)

## Architecture Overview

### The Layered Governance Model

```
┌───────────────────────────────────────────────────────────────┐
│                  User Interaction Layer                       │
│         (Slack, Teams, WebChat, API)                          │
└───────────────────────┬───────────────────────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────────────────────┐
│                Control Plane (BotSharp)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ Authentication│  │   Router     │  │  Session Mgmt    │   │
│  │   & Access   │  │ (Intent Rec) │  │                  │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
│                                                                │
│  ┌────────────────────────────────────────────────────────┐  │
│  │            Domain Agents                               │  │
│  │  • Order Management                                    │  │
│  │  • IT Support                                          │  │
│  │  • HR Service                                          │  │
│  └────────────────────┬───────────────────────────────────┘  │
└───────────────────────┼───────────────────────────────────────┘
                        │ Function Callbacks
                        ▼
┌───────────────────────────────────────────────────────────────┐
│              Execution Plane (MAF)                             │
│  ┌──────────────────────────────────────────────────────┐    │
│  │    MAF Workflow Functions                            │    │
│  │  • execute_order_workflow                            │    │
│  │  • execute_maf_workflow                              │    │
│  └────────────────────┬─────────────────────────────────┘    │
│                       │                                        │
│  ┌────────────────────┴─────────────────────────────────┐    │
│  │         MAF Agent Workflows                          │    │
│  │  ┌─────────────┐  ┌──────────────┐  ┌────────────┐  │    │
│  │  │ Validator   │→ │  Processor   │→ │  Notifier  │  │    │
│  │  │   Agent     │  │    Agent     │  │   Agent    │  │    │
│  │  └─────────────┘  └──────────────┘  └────────────┘  │    │
│  │                                                      │    │
│  │  Deterministic Business Logic Execution             │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                                │
│  ┌──────────────────────────────────────────────────────┐    │
│  │         Enterprise System Integration                 │    │
│  │  • ERP Systems                                       │    │
│  │  • CRM Systems                                       │    │
│  │  • Database Systems                                  │    │
│  └──────────────────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────┘
```

### Key Principles

1. **Separation of Concerns**: BotSharp handles user interaction and intent, MAF handles business logic
2. **Deterministic Execution**: Critical workflows use code-based MAF workflows instead of prompt engineering
3. **Context Preservation**: Conversation context flows from BotSharp to MAF and back
4. **State Management**: Long-running workflows can be serialized and resumed

## Installation

### Step 1: Add Plugin to BotSharp

1. Copy the `BotSharp.Plugin.MicrosoftAgentFramework` folder to your BotSharp plugins directory
2. Add the project reference to your BotSharp solution:

```bash
cd /path/to/BotSharp
dotnet sln add src/Plugins/BotSharp.Plugin.MicrosoftAgentFramework/BotSharp.Plugin.MicrosoftAgentFramework.csproj
```

3. Build the plugin:

```bash
dotnet build src/Plugins/BotSharp.Plugin.MicrosoftAgentFramework/BotSharp.Plugin.MicrosoftAgentFramework.csproj
```

### Step 2: Register the Plugin

In your BotSharp startup code (typically in `Program.cs` or `Startup.cs`):

```csharp
using BotSharp.Plugin.MicrosoftAgentFramework;

// In your service configuration
services.AddBotSharp(Configuration, (options) =>
{
    // ... other plugin configurations
    
    // Add MAF plugin
    options.LoadPlugin<MicrosoftAgentFrameworkPlugin>();
});
```

## Configuration

### appsettings.json Configuration

Add the following to your `appsettings.json`:

```json
{
  "MicrosoftAgentFramework": {
    "Enabled": true,
    "OpenAIEndpoint": "https://api.openai.com/v1",
    "OpenAIApiKey": "your-openai-api-key",
    "DefaultModel": "gpt-4",
    "EnableStateSerialization": true,
    "WorkflowTimeoutSeconds": 300,
    "EnableDetailedLogging": false
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | boolean | `true` | Enable or disable the MAF plugin |
| `OpenAIEndpoint` | string | - | OpenAI API endpoint for MAF agents |
| `OpenAIApiKey` | string | - | OpenAI API key (can use environment variable) |
| `DefaultModel` | string | `"gpt-4"` | Default LLM model for MAF agents |
| `EnableStateSerialization` | boolean | `true` | Enable state saving for long-running workflows |
| `WorkflowTimeoutSeconds` | integer | `300` | Timeout for workflow execution (5 minutes) |
| `EnableDetailedLogging` | boolean | `false` | Enable verbose logging for debugging |

### Using Environment Variables

For production deployments, use environment variables:

```bash
export MicrosoftAgentFramework__OpenAIApiKey="sk-..."
export MicrosoftAgentFramework__OpenAIEndpoint="https://api.openai.com/v1"
```

Or in Docker:

```yaml
environment:
  - MicrosoftAgentFramework__OpenAIApiKey=sk-...
  - MicrosoftAgentFramework__OpenAIEndpoint=https://api.openai.com/v1
```

## Creating MAF-Enabled Agents

### Using the BotSharp UI

1. Navigate to the BotSharp Agent Management UI
2. Create a new agent or edit an existing one
3. In the **Functions** section, add MAF functions:

**For Order Management:**
- Function Name: `execute_order_workflow`
- Provider: `Botsharp`
- Add parameter schema (see example agent configs)

**For Generic Workflows:**
- Function Name: `execute_maf_workflow`
- Provider: `Botsharp`
- Add parameter schema

4. Update the agent's instruction to explain when to use MAF functions

### Using JSON Configuration

See the example agent configurations in the `Examples/` directory:
- `order-management-agent.json` - Order processing agent
- `it-support-agent.json` - IT support agent
- `hr-service-agent.json` - HR service agent

To import:

```bash
# Copy to your agents directory
cp Examples/*.json /path/to/botsharp/agents/
```

## Using the Integration

### Example 1: Order Refund Workflow

**User:** "I want to refund order ORD-12345"

**BotSharp Router:** Routes to "Order Management Agent"

**Agent Conversation:**
```
Agent: I'll help you process the refund. Can you provide your customer ID?
User: CUST-67890
Agent: What's the reason for the refund?
User: Product was damaged
```

**Function Call:**
```json
{
  "order_id": "ORD-12345",
  "customer_id": "CUST-67890",
  "items": [...],
  "total_amount": 199.98,
  "action": "refund",
  "reason": "Product was damaged"
}
```

**MAF Workflow Execution:**
1. Validates order exists and is refundable
2. Calculates refund amount
3. Locks inventory
4. Initiates refund transaction
5. Sends notification to customer

**Agent Response:** Presents formatted result to user with refund confirmation

### Example 2: IT Support Workflow

**User:** "I can't connect to the VPN"

**BotSharp Router:** Routes to "IT Support Agent"

**Function Call:**
```json
{
  "workflow_type": "it_support",
  "input_data": {
    "issue_type": "Network Issue",
    "description": "Unable to connect to VPN"
  },
  "conversation_history": [...]
}
```

**MAF Workflow:** Processes the IT issue through diagnostic and resolution steps

### Example 3: HR Service Request

**User:** "I need to request vacation leave"

**Function Call:**
```json
{
  "workflow_type": "hr_service",
  "input_data": {
    "request_type": "Leave Request",
    "employee_id": "EMP-123",
    "details": "Vacation leave from 2025-12-20 to 2025-12-31"
  }
}
```

## Advanced Topics

### State Serialization for Long-Running Workflows

When a workflow requires human approval:

```csharp
// In MAF workflow
var serializedState = await workflow.SerializeStateAsync();

// Return to BotSharp
return new WorkflowResult
{
    Success = true,
    Output = "Waiting for manager approval...",
    SerializedState = serializedState
};
```

BotSharp stores the state and resumes when user returns:

```csharp
// User returns with approval
var request = new WorkflowExecutionRequest
{
    WorkflowType = "order_approval",
    SerializedState = storedState // Retrieved from conversation context
};
```

### Custom Workflow Types

To add new workflow types:

1. Extend `MafWorkflowExecutorFunction.cs`:

```csharp
private async Task<string> ExecuteCustomWorkflow(
    WorkflowExecutionRequest request, 
    CancellationToken cancellationToken)
{
    // Your custom workflow logic
    // Integrate with MAF agents
    return result;
}
```

2. Add to the workflow type switch:

```csharp
string workflowOutput = request.WorkflowType.ToLowerInvariant() switch
{
    "custom_workflow" => await ExecuteCustomWorkflow(request, cancellationToken),
    // ... other workflows
};
```

### Integrating Real Microsoft Agent Framework

When the official `Microsoft.Agents.AI` SDK becomes available:

1. Update package reference in `.csproj`:

```xml
<PackageReference Include="Microsoft.Agents.AI" Version="x.x.x" />
```

2. Update workflow execution methods to use real MAF SDK:

```csharp
private async Task<WorkflowResult> ExecuteMafWorkflow(...)
{
    // Initialize MAF agents
    var chatClient = new OpenAIChatClient(
        _settings.OpenAIApiKey, 
        _settings.DefaultModel
    );
    
    var validatorAgent = new ChatClientAgent(chatClient, "Validator");
    var processorAgent = new ChatClientAgent(chatClient, "Processor");
    
    // Build workflow
    var workflow = new WorkflowBuilder(validatorAgent)
        .AddEdge(validatorAgent, processorAgent)
        .Build();
    
    // Execute
    var result = await workflow.RunAsync(inputData);
    return MapToWorkflowResult(result);
}
```

### Enterprise System Integration

Connect MAF workflows to your enterprise systems:

```csharp
// In your custom workflow
private async Task<string> ExecuteERPIntegration(OrderRequest order)
{
    // Connect to ERP
    using var erpClient = new ERPClient(_erpSettings);
    
    // MAF agent for validation
    var validator = CreateMafAgent("ERPValidator");
    
    // Validate through MAF
    var validationResult = await validator.ProcessAsync(order);
    
    // Execute in ERP
    var erpResult = await erpClient.ProcessOrder(order);
    
    return FormatResult(erpResult);
}
```

### Monitoring and Logging

Enable detailed logging in production:

```csharp
// In appsettings.Production.json
{
  "MicrosoftAgentFramework": {
    "EnableDetailedLogging": true
  },
  "Logging": {
    "LogLevel": {
      "BotSharp.Plugin.MicrosoftAgentFramework": "Information"
    }
  }
}
```

View MAF execution logs:
- Workflow IDs
- Execution times
- Success/failure rates
- Error details

## Best Practices

1. **Use MAF for Deterministic Workflows**: Anything involving money, inventory, or critical business logic
2. **Keep BotSharp for Conversation**: User interaction, intent recognition, and routing
3. **Design Clear Function Schemas**: Make it easy for the LLM to call MAF functions correctly
4. **Handle Errors Gracefully**: MAF workflows should return structured errors
5. **Test Workflows Separately**: Unit test MAF workflows independent of BotSharp
6. **Monitor Performance**: Track workflow execution times and success rates
7. **Version Your Workflows**: Maintain backward compatibility when updating workflows

## Troubleshooting

### Common Issues

**Problem:** Function not found
```
Solution: Ensure the plugin is registered and functions are added to the agent configuration
```

**Problem:** Workflow timeout
```
Solution: Increase WorkflowTimeoutSeconds or optimize the workflow
```

**Problem:** State serialization fails
```
Solution: Ensure all workflow state is serializable (avoid closures)
```

## Support

For issues or questions:
- Open an issue in the BotSharp repository
- Check the MAF plugin README
- Review example configurations

## License

This plugin is part of BotSharp and follows the same license terms.
