# Dify Plugin Usage Examples

## Scenario 1: CEO Email Report Generator (Synchronous)

This example shows a simple synchronous workflow execution for quick tasks.

### Agent Configuration

Add the Dify function to your agent's function list:

```json
{
  "name": "dify-workflow-execute",
  "description": "Execute a Dify workflow to generate content or perform complex transformations"
}
```

### Usage in Conversation

**User**: "Generate a quarterly sales report summary"

**Agent thinks**: I need to call the Dify workflow to generate the report

**Function Call**:
```json
{
  "function": "dify-workflow-execute",
  "arguments": {
    "workflow_id": "sales-report-generator",
    "inputs": "{\"period\": \"Q4-2024\", \"format\": \"executive-summary\"}",
    "async": false
  }
}
```

**Response**: The workflow completes immediately and returns the generated report.

---

## Scenario 2: Complex Data Analysis (Asynchronous)

This example demonstrates async execution with background polling for long-running workflows.

### Workflow Description

1. Extract data from multiple sources
2. Clean and normalize data
3. Perform statistical analysis
4. Generate visualizations
5. Create presentation deck

### Implementation

**Step 1: BotSharp receives request**

User: "Analyze last quarter's sales data and create a presentation for the board meeting"

**Step 2: BotSharp validates permissions**

```csharp
// BotSharp middleware checks if user has access to sales data
if (!user.HasPermission("access:sales_data")) {
    return "Access denied";
}
```

**Step 3: BotSharp extracts raw data**

```csharp
// Call local C# service for data extraction
var salesData = await SalesDataService.GetLastQuarterData();
```

**Step 4: BotSharp calls Dify workflow**

```json
{
  "function": "dify-workflow-execute",
  "arguments": {
    "workflow_id": "data-analysis-presentation",
    "inputs": "{\"sales_data\": {...}, \"target_audience\": \"board\", \"format\": \"pptx\"}",
    "async": true,
    "conversation_id": "conv-12345"
  }
}
```

**Step 5: Immediate response**

```json
{
  "status": "pending",
  "task_id": "task-abc-123",
  "message": "Analysis started. This will take a few minutes. I'll notify you when ready."
}
```

**Step 6: Background polling**

The `DifyTaskPollingService` automatically:
- Polls every 10 seconds
- Updates task status
- Waits for completion

**Step 7: Task completion**

When Dify finishes:
- Result is retrieved
- Conversation can be resumed
- User is notified

---

## Scenario 3: Multi-Step Workflow with Branching

### Use Case: Customer Support Email Handler

**Workflow Steps**:
1. Classify customer email (complaint/question/feedback)
2. Branch based on classification:
   - Complaint → Escalation workflow
   - Question → FAQ lookup + personalized response
   - Feedback → Sentiment analysis + routing

### Implementation

**Function Call**:
```json
{
  "function": "dify-workflow-execute",
  "arguments": {
    "workflow_id": "customer-email-handler",
    "inputs": "{\"email_content\": \"...\", \"customer_id\": \"12345\", \"priority\": \"high\"}",
    "async": true
  }
}
```

**Dify Workflow** (visual representation):
```
[Email Input]
    ↓
[Classification Node]
    ↓
[Branch Node]
    ├─ Complaint → [Escalate to Manager] → [Generate Response]
    ├─ Question → [FAQ Search] → [LLM Generation] → [Personalize]
    └─ Feedback → [Sentiment Analysis] → [Route to Product Team]
```

---

## Scenario 4: Document Translation Pipeline

### Use Case: Multi-language Report Generation

**Requirements**:
- Generate report in English
- Translate to 5 languages
- Apply regional formatting
- Generate PDFs

### Function Call:
```json
{
  "function": "dify-workflow-execute",
  "arguments": {
    "workflow_id": "multilingual-report-generator",
    "inputs": "{\"source_data\": {...}, \"languages\": [\"es\", \"fr\", \"de\", \"ja\", \"zh\"], \"format\": \"pdf\"}",
    "async": true
  }
}
```

---

## Scenario 5: Real-time vs Async Decision Logic

### In BotSharp Router

```csharp
public async Task<RoleDialogModel> RouteRequest(RoleDialogModel request)
{
    // Analyze task complexity
    var complexity = AnalyzeComplexity(request);
    
    if (complexity.EstimatedTime < 30) // seconds
    {
        // Use synchronous execution
        return await ExecuteDifyWorkflow(
            workflowId: complexity.WorkflowId,
            async: false
        );
    }
    else
    {
        // Use asynchronous execution
        return await ExecuteDifyWorkflow(
            workflowId: complexity.WorkflowId,
            async: true
        );
    }
}
```

---

## Configuration Tips

### Development Environment
```json
{
  "Dify": {
    "BaseUrl": "http://localhost:8080",
    "ApiKey": "dev-api-key",
    "PollingIntervalSeconds": 5,
    "MaxPollingAttempts": 60
  }
}
```

### Production Environment
```json
{
  "Dify": {
    "BaseUrl": "https://api.dify.ai",
    "ApiKey": "${DIFY_API_KEY}", // Use environment variable
    "PollingIntervalSeconds": 15,
    "MaxPollingAttempts": 240,
    "TimeoutSeconds": 600
  }
}
```

---

## Best Practices

### 1. Input Validation
Always validate inputs before calling Dify:
```csharp
if (string.IsNullOrEmpty(workflowId)) 
{
    throw new ArgumentException("Workflow ID is required");
}
```

### 2. Error Handling
```csharp
try 
{
    var result = await ExecuteDifyWorkflow(...);
} 
catch (DifyApiException ex) 
{
    _logger.LogError($"Dify API error: {ex.Message}");
    // Fallback to alternative approach
}
```

### 3. Monitoring
- Log all workflow executions
- Track execution times
- Monitor success/failure rates
- Set up alerts for long-running tasks

### 4. Cost Optimization
- Cache frequent results
- Use batch processing when possible
- Implement rate limiting
- Monitor API usage

---

## Troubleshooting

### Issue: Task never completes

**Solution**:
1. Check Dify workflow logs
2. Verify workflow doesn't have infinite loops
3. Increase `MaxPollingAttempts`
4. Check network connectivity

### Issue: Authentication errors

**Solution**:
1. Verify API key is correct
2. Check API key permissions
3. Ensure proper authorization header format

### Issue: Slow polling

**Solution**:
1. Decrease `PollingIntervalSeconds` (be mindful of rate limits)
2. Consider implementing webhooks if available
3. Use multiple worker threads for polling

---

## Integration with BotSharp Router

### Example Router Configuration

```json
{
  "routes": [
    {
      "name": "content_generation",
      "condition": "task_type == 'generate_content'",
      "handler": "dify-workflow-execute",
      "config": {
        "workflow_id": "content-generator",
        "async": true
      }
    },
    {
      "name": "data_analysis",
      "condition": "task_type == 'analyze_data'",
      "handler": "dify-workflow-execute",
      "config": {
        "workflow_id": "data-analyzer",
        "async": true
      }
    }
  ]
}
```

---

## Advanced: Custom Task Completion Handler

For production use, implement custom completion logic:

```csharp
public class CustomDifyTaskPollingService : DifyTaskPollingService
{
    protected override async Task OnTaskCompletedAsync(DifyWorkflowTask task)
    {
        // Load conversation context
        var conversation = await _conversationService.GetConversation(task.ConversationId);
        
        // Parse result
        var result = JsonSerializer.Deserialize<WorkflowResult>(task.ResultData);
        
        // Resume conversation
        await _conversationService.ResumeConversation(
            conversation, 
            result
        );
        
        // Send notification
        await _notificationService.Notify(
            task.UserId,
            $"Task {task.TaskId} completed successfully"
        );
        
        // Clean up
        _taskStorage.RemoveTask(task.TaskId);
    }
}
```
