# BotSharp.Plugin.Dify

## Overview

The BotSharp.Plugin.Dify integrates BotSharp with Dify workflows, implementing a hybrid architecture where:

- **BotSharp** acts as the upper-layer orchestrator and "gatekeeper" for:
  - User authentication and authorization
  - Task routing and decomposition
  - Macro-level planning
  - Audit logging
  
- **Dify** serves as the execution engine for:
  - Complex workflow orchestration
  - Content generation with flexible prompt engineering
  - Data transformation and insight extraction

## Architecture

This plugin implements the architecture described in the BotSharp + Dify integration blueprint:

```
User Request
    ↓
[BotSharp Layer - Authentication & Routing]
    ↓
[BotSharp Router - Macro Task Routing]
    ↓
[BotSharp Planner - Task Decomposition]
    ↓
[Dify Plugin - Workflow Execution]
    ↓
[Dify Workflow - Content Generation]
    ↓
[BotSharp - Result Delivery & Logging]
```

## Features

### 1. Synchronous Workflow Execution
Execute Dify workflows in blocking mode for quick tasks:
```csharp
{
  "workflow_id": "your-workflow-id",
  "inputs": "{\"data\": \"value\"}",
  "async": false
}
```

### 2. Asynchronous Workflow Execution with Polling
For long-running workflows, the plugin:
- Initiates the workflow asynchronously
- Returns a task ID immediately
- Polls the workflow status in background
- Resumes the conversation when complete

```csharp
{
  "workflow_id": "your-workflow-id",
  "inputs": "{\"data\": \"value\"}",
  "async": true
}
```

### 3. Background Task Polling
A `HostedService` continuously polls running tasks:
- Checks workflow status at configurable intervals
- Updates task state
- Handles completion, failure, and timeout scenarios
- Supports session suspension and resumption

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "Dify": {
    "BaseUrl": "https://api.dify.ai",
    "ApiKey": "your-dify-api-key",
    "PollingIntervalSeconds": 10,
    "MaxPollingAttempts": 180,
    "TimeoutSeconds": 300
  }
}
```

### Configuration Options

- **BaseUrl**: Dify API endpoint
- **ApiKey**: Authentication token for Dify API
- **PollingIntervalSeconds**: How often to check task status (default: 10 seconds)
- **MaxPollingAttempts**: Maximum polls before timeout (default: 180, ~30 minutes with 10s interval)
- **TimeoutSeconds**: HTTP request timeout (default: 300 seconds)

## Usage

### 1. Register the Plugin

The plugin is automatically registered when included in your BotSharp application:

```csharp
services.AddBotSharp(config, 
    enableDataSync: true, 
    enableLogger: true);
```

### 2. Call from Agent

Use the function in your agent's workflow:

```json
{
  "function": "dify-workflow-execute",
  "arguments": {
    "workflow_id": "abc-123-def-456",
    "inputs": "{\"sales_data\": \"Q4 2024\", \"format\": \"executive_summary\"}",
    "async": true,
    "conversation_id": "current-conversation-id"
  }
}
```

### 3. Monitor Task Status

For async tasks, the plugin:
1. Returns immediately with task ID
2. Stores task in memory (consider persistent storage for production)
3. Background service polls Dify API
4. Notifies when complete

## Technical Implementation Details

### Data Flow

1. **Request Initiation**
   - BotSharp receives user request
   - Router identifies need for Dify workflow
   - CallDifyWorkflowFn is invoked

2. **Workflow Execution**
   - Plugin serializes C# objects to JSON
   - Calls Dify REST API: `POST /v1/workflows/{id}/run`
   - Receives workflow_run_id and task_id

3. **Async Handling**
   - Task stored in DifyTaskStorageService
   - Conversation suspended with pending status
   - Background service begins polling

4. **Status Polling**
   - DifyTaskPollingService checks: `GET /v1/workflows/run/{task_id}`
   - Updates task status based on response
   - Continues until complete/failed/timeout

5. **Completion**
   - Result extracted from workflow output
   - Conversation resumed (placeholder for implementation)
   - Response delivered to user

### Key Components

- **DifyPlugin**: Main plugin registration
- **DifySettings**: Configuration binding
- **CallDifyWorkflowFn**: IFunctionCallback implementation
- **DifyWorkflowService**: API communication layer
- **DifyTaskStorageService**: In-memory task tracking
- **DifyTaskPollingService**: Background polling service

### Models

- **DifyWorkflowRequest**: API request structure
- **DifyWorkflowResponse**: API response structure
- **DifyWorkflowArgs**: Function arguments from LLM
- **DifyWorkflowTask**: Internal task tracking
- **DifyWorkflowData**: Workflow output data

## Production Considerations

### 1. Persistent Storage
Replace `DifyTaskStorageService` in-memory storage with:
- Database (SQL/NoSQL)
- Redis for distributed systems
- Azure Table Storage / AWS DynamoDB

### 2. Session Resumption
Implement the `OnTaskCompletedAsync` method to:
- Load conversation context
- Inject workflow results
- Continue agent execution flow

### 3. Error Handling
- Implement retry logic with exponential backoff
- Add circuit breakers for API failures
- Monitor and alert on task timeouts

### 4. Security
- Encrypt API keys in configuration
- Validate workflow IDs against whitelist
- Implement rate limiting
- Audit log all workflow executions

### 5. Scalability
- Use message queues (RabbitMQ, Azure Service Bus)
- Implement webhooks if Dify supports them (future)
- Consider worker pool pattern for polling

## Example Scenario

**User Request**: "Analyze the Q4 sales report and draft an email to the CEO"

**BotSharp Processing**:
1. Authenticates user and validates permissions
2. Router identifies two tasks:
   - Data extraction (local C# service)
   - Email drafting (Dify workflow)

3. Calls local service: `SalesDataService.GetLastQuarterData()`
4. Invokes Dify plugin with raw data:
   ```json
   {
     "workflow_id": "email-drafter-workflow",
     "inputs": "{\"sales_data\": {...}, \"recipient\": \"CEO\"}",
     "async": true
   }
   ```

5. Dify executes workflow:
   - Data cleaning node
   - Insight extraction node
   - Email composition node
   - Content polishing node

6. Plugin polls and receives draft
7. BotSharp sends email via Exchange API
8. Logs audit trail

## API Reference

### Function: dify-workflow-execute

**Arguments**:
- `workflow_id` (required): Dify workflow identifier
- `inputs` (optional): JSON string of input parameters
- `async` (optional): Execute asynchronously (default: false)
- `conversation_id` (optional): Context for conversation tracking

**Returns**:
- Synchronous: Workflow output data
- Asynchronous: Task ID and pending status

## Troubleshooting

### Common Issues

1. **Connection Timeout**
   - Increase `TimeoutSeconds` in settings
   - Check network connectivity to Dify API
   - Verify API endpoint URL

2. **Authentication Errors**
   - Verify API key is correct
   - Check API key has workflow execution permissions
   - Ensure proper authorization header format

3. **Task Never Completes**
   - Check Dify workflow logs
   - Verify workflow doesn't have infinite loops
   - Increase `MaxPollingAttempts` if needed

4. **Memory Leak**
   - Implement persistent storage
   - Add task cleanup for completed tasks
   - Set retention policies

## Future Enhancements

1. **Webhook Support**: Replace polling with push notifications when Dify adds webhook support
2. **Streaming Results**: Stream intermediate workflow results to user
3. **Workflow Templates**: Pre-configured workflows for common tasks
4. **Analytics Dashboard**: Monitor workflow performance and costs
5. **Caching Layer**: Cache frequent workflow results
6. **Multi-tenancy**: Isolate workflows by tenant/organization

## License

This plugin follows the BotSharp project license.

## Contributing

Contributions welcome! Please follow BotSharp contribution guidelines.
