# BotSharp.Plugin.MicrosoftAgentFramework

Microsoft Agent Framework (MAF) Integration Plugin for BotSharp

## Overview

This plugin enables BotSharp to integrate with Microsoft Agent Framework, implementing a **layered governance architecture** that separates the **Control Plane** (BotSharp) from the **Execution Plane** (MAF).

### Architecture Philosophy

BotSharp and Microsoft Agent Framework should not be seen as competitors, but as complementary technology stacks. An ideal enterprise AI architecture combines:

- **BotSharp's high-level intent management capabilities** (Control Plane)
- **MAF's low-level execution robustness** (Execution Plane)

## Architecture Blueprint

### Control Plane: BotSharp's Role

BotSharp is deployed as the **unified access gateway**:

- **Responsibilities**: 
  - User authentication
  - Session lifecycle management
  - Intent recognition (Router)
  - Integration with messaging channels (Slack, Teams, WebChat)

- **Configuration**: 
  - Business analysts define high-level Agents in BotSharp UI (e.g., "IT Support", "HR Service", "Order Management")

- **Value**: 
  - BotSharp's Router handles ambiguous natural language input, converging diverse user expressions into clear business domain intents

### Execution Plane: Microsoft Agent Framework's Role

MAF is encapsulated as concrete **business execution units**:

- **Responsibilities**: 
  - Execute specific business processes
  - Handle multi-step transaction processing
  - Deep integration with enterprise ERP/CRM systems

- **Implementation**: 
  - When BotSharp routes a request to "Order Management" Agent, the underlying implementation triggers a predefined **MAF Workflow** rather than directly calling LLM

- **Value**: 
  - MAF's strongly-typed workflows ensure business logic rigor
  - Example: Refund processing uses MAF Sequential Workflow with strict steps: "Validate Order → Calculate Amount → Lock Inventory → Initiate Refund → Send Notification"
  - Each step can be precisely error-handled through code logic, rather than relying on LLM "luck"

## Code-Level Integration Pattern: Plugin Wrapping

The key to this architecture is connecting the two systems. BotSharp's strong plugin extensibility allows MAF to be embedded by implementing the `IBotSharpPlugin` interface.

### Integration Design

1. **Define MAF Wrapper**: Create a .NET class library that references both `Microsoft.Agents.AI` and `BotSharp.Core`

2. **Implement IFunctionCallback**: Wrap MAF workflow startup logic in BotSharp's tool callbacks

3. **Register Plugin**: Enable the plugin in BotSharp's configuration

4. **Configure Agent**: In BotSharp UI, add MAF workflow tools to relevant Agents

## Data Flow and Context Passing

### Input Parameters
- BotSharp passes Conversation History summary or key parameters (from Slot Filling) to MAF
- JSON serialization is used as the standard carrier

### State Persistence
- If MAF workflow needs to suspend (e.g., waiting for human approval), use `AgentThread.SerializeAsync` to serialize state as a string
- State is returned to BotSharp for storage
- When user responds with confirmation, BotSharp passes the state string back to MAF for restoration

### Output Parameters
- MAF returns structured results (not plain text) after execution
- BotSharp's Template Engine renders these into user-friendly natural language

## Installation

### Prerequisites

- .NET 8.0 or later
- BotSharp.Core package
- Microsoft.Extensions.AI.Abstractions package (or Microsoft.Agents.AI when available)

### Configuration

Add to your `appsettings.json`:

```json
{
  "MicrosoftAgentFramework": {
    "Enabled": true,
    "OpenAIEndpoint": "https://api.openai.com/v1",
    "OpenAIApiKey": "your-api-key-here",
    "DefaultModel": "gpt-4",
    "EnableStateSerialization": true,
    "WorkflowTimeoutSeconds": 300,
    "EnableDetailedLogging": false
  }
}
```

### Register the Plugin

In your BotSharp startup configuration:

```csharp
services.AddBotSharp(config, (options) =>
{
    // ... other configurations
    options.LoadPlugin<MicrosoftAgentFrameworkPlugin>();
});
```

## Usage

### Example 1: Order Processing Workflow

The plugin provides an `execute_order_workflow` function that can be called by BotSharp agents:

```json
{
  "order_id": "ORD-12345",
  "customer_id": "CUST-67890",
  "items": [
    {
      "product_id": "PROD-001",
      "product_name": "Product A",
      "quantity": 2,
      "price": 99.99
    }
  ],
  "total_amount": 199.98,
  "action": "refund",
  "reason": "Customer request"
}
```

### Example 2: Generic Workflow Execution

Use the `execute_maf_workflow` function for other business processes:

```json
{
  "workflow_type": "it_support",
  "input_data": {
    "issue_type": "Network Issue",
    "description": "Unable to connect to VPN"
  },
  "conversation_history": [
    {
      "role": "user",
      "content": "I can't connect to the company VPN",
      "timestamp": "2025-12-06T01:00:00Z"
    }
  ]
}
```

### Configuring Agents in BotSharp

1. Navigate to BotSharp UI
2. Create or edit an Agent (e.g., "Order Management Agent")
3. Add the function tool:
   - Function Name: `execute_order_workflow`
   - Provider: `Botsharp`
   - Description: "Execute order processing, refund, or cancellation workflow"

## Workflow Types

The plugin currently supports demonstration workflows for:

1. **Order Management**
   - Order Processing
   - Order Refund
   - Order Cancellation

2. **IT Support**
   - Issue resolution
   - System configuration
   - Ticket management

3. **HR Service**
   - Employee requests
   - Document processing
   - Approval workflows

4. **Customer Service**
   - General inquiries
   - Case resolution
   - Customer satisfaction tracking

## Extending with Real MAF Implementation

To integrate actual Microsoft Agent Framework:

1. Add reference to `Microsoft.Agents.AI` package (when available)

2. Update the workflow execution methods:

```csharp
// Example with real MAF SDK
var validatorAgent = new ChatClientAgent(
    new OpenAIChatClient(_settings.OpenAIApiKey, _settings.DefaultModel), 
    "Validator"
);

var processorAgent = new ChatClientAgent(
    new OpenAIChatClient(_settings.OpenAIApiKey, _settings.DefaultModel), 
    "Processor"
);

var workflow = new WorkflowBuilder(validatorAgent)
    .AddEdge(validatorAgent, processorAgent)
    .Build();

var result = await workflow.RunAsync(inputData);
```

## Benefits

1. **Separation of Concerns**: BotSharp handles user interaction and intent, MAF handles business logic
2. **Robustness**: Critical business processes use deterministic code-based workflows
3. **Flexibility**: Easy to add new workflows by implementing new function callbacks
4. **Maintainability**: Business logic in MAF workflows is easier to test and maintain than prompt engineering
5. **Enterprise Integration**: MAF workflows can directly integrate with existing enterprise systems

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Control Plane (BotSharp)                 │
│  ┌─────────────┐    ┌──────────┐    ┌─────────────────┐    │
│  │   Router    │───▶│  Agent   │───▶│  MAF Function   │    │
│  │  (Intent)   │    │ (Domain) │    │    Callback     │    │
│  └─────────────┘    └──────────┘    └─────────┬───────┘    │
└────────────────────────────────────────────────┼────────────┘
                                                  │
                                                  ▼
┌─────────────────────────────────────────────────────────────┐
│                  Execution Plane (MAF)                       │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │  Validator   │───▶│  Processor   │───▶│  Notifier    │  │
│  │    Agent     │    │    Agent     │    │    Agent     │  │
│  └──────────────┘    └──────────────┘    └──────────────┘  │
│                                                              │
│  Deterministic Workflow: Validate → Process → Notify        │
└─────────────────────────────────────────────────────────────┘
```

## License

This plugin is part of BotSharp and follows the same license.

## Contributing

Contributions are welcome! Please follow BotSharp's contribution guidelines.
