using BotSharp.Abstraction.Functions;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Plugin.MicrosoftAgentFramework.Models;
using BotSharp.Plugin.MicrosoftAgentFramework.Settings;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace BotSharp.Plugin.MicrosoftAgentFramework.Functions;

/// <summary>
/// Generic MAF Workflow Executor Function
/// Provides a flexible interface for executing various MAF workflows from BotSharp
/// </summary>
public class MafWorkflowExecutorFunction : IFunctionCallback
{
    public string Name => "execute_maf_workflow";
    
    public string Indication => "正在执行业务工作流... (Executing business workflow...)";

    private readonly ILogger<MafWorkflowExecutorFunction> _logger;
    private readonly MicrosoftAgentFrameworkSettings _settings;
    private readonly IServiceProvider _services;

    public MafWorkflowExecutorFunction(
        ILogger<MafWorkflowExecutorFunction> logger,
        MicrosoftAgentFrameworkSettings settings,
        IServiceProvider services)
    {
        _logger = logger;
        _settings = settings;
        _services = services;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (_settings.EnableDetailedLogging)
            {
                _logger.LogInformation($"[MAF] Starting generic workflow execution with args: {message.FunctionArgs}");
            }

            // Parse workflow execution request
            var request = ParseWorkflowRequest(message.FunctionArgs);

            if (request == null)
            {
                message.Content = "Invalid workflow request format.";
                return false;
            }

            // Apply timeout from settings if not specified
            var timeout = request.TimeoutSeconds ?? _settings.WorkflowTimeoutSeconds;

            // Execute workflow with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            var result = await ExecuteWorkflow(request, cts.Token);

            // Set response
            message.Content = result.Output;

            // Store state if needed
            if (!string.IsNullOrEmpty(result.SerializedState) && _settings.EnableStateSerialization)
            {
                message.Payload = result.SerializedState;
            }

            stopwatch.Stop();
            _logger.LogInformation($"[MAF] Workflow '{request.WorkflowType}' completed in {stopwatch.ElapsedMilliseconds}ms. Success: {result.Success}");

            return result.Success;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[MAF] Workflow execution timed out");
            message.Content = "Workflow execution timed out. Please try again or contact support.";
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MAF] Error executing workflow");
            message.Content = $"Error executing workflow: {ex.Message}";
            return false;
        }
    }

    private WorkflowExecutionRequest? ParseWorkflowRequest(string jsonInput)
    {
        try
        {
            return JsonSerializer.Deserialize<WorkflowExecutionRequest>(jsonInput);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MAF] Failed to parse workflow request");
            return null;
        }
    }

    /// <summary>
    /// Execute MAF workflow based on workflow type
    /// This demonstrates the plugin wrapping pattern from the problem statement
    /// </summary>
    private async Task<WorkflowResult> ExecuteWorkflow(WorkflowExecutionRequest request, CancellationToken cancellationToken)
    {
        var workflowId = Guid.NewGuid().ToString();
        var startTime = Stopwatch.StartNew();

        var result = new WorkflowResult
        {
            WorkflowId = workflowId,
            Metadata = new Dictionary<string, object>
            {
                ["workflow_type"] = request.WorkflowType,
                ["started_at"] = DateTime.UtcNow
            }
        };

        try
        {
            // In a real implementation, this would:
            // 1. Load workflow definition based on workflow_type
            // 2. Initialize MAF agents with configuration from _settings
            // 3. Build workflow graph
            // 4. Execute with proper error handling and state management
            
            /*
            // Real MAF implementation would look like:
            var agentBuilder = new ChatClientAgentBuilder()
                .WithEndpoint(_settings.OpenAIEndpoint)
                .WithApiKey(_settings.OpenAIApiKey)
                .WithModel(_settings.DefaultModel);

            var workflowDef = LoadWorkflowDefinition(request.WorkflowType);
            var agents = workflowDef.Steps.Select(step => 
                agentBuilder.WithName(step.AgentName)
                           .WithInstructions(step.Instructions)
                           .Build()
            ).ToList();

            var workflow = new WorkflowBuilder(agents[0]);
            for (int i = 0; i < agents.Count - 1; i++)
            {
                workflow.AddEdge(agents[i], agents[i + 1]);
            }

            var mafWorkflow = workflow.Build();
            var mafResult = await mafWorkflow.RunAsync(
                JsonSerializer.Serialize(request.InputData), 
                cancellationToken
            );

            result.Output = mafResult.FinalOutput;
            result.SerializedState = await mafWorkflow.SerializeStateAsync();
            */

            // Demonstration implementation
            string workflowOutput = request.WorkflowType.ToLowerInvariant() switch
            {
                "it_support" => await ExecuteITSupportWorkflow(request, cancellationToken),
                "hr_service" => await ExecuteHRServiceWorkflow(request, cancellationToken),
                "customer_service" => await ExecuteCustomerServiceWorkflow(request, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown workflow type: {request.WorkflowType}")
            };

            result.Success = true;
            result.Output = workflowOutput;
            result.ExecutionTimeMs = startTime.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[MAF] Workflow {workflowId} execution failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Output = $"Workflow execution failed: {ex.Message}";
            result.ExecutionTimeMs = startTime.ElapsedMilliseconds;
        }

        return result;
    }

    private async Task<string> ExecuteITSupportWorkflow(WorkflowExecutionRequest request, CancellationToken cancellationToken)
    {
        // Simulate IT Support workflow with MAF
        var issue = request.InputData.TryGetValue("issue_type", out var issueType) 
            ? issueType.ToString() 
            : "General IT Issue";
        
        var description = request.InputData.TryGetValue("description", out var desc) 
            ? desc.ToString() 
            : "";

        await Task.Delay(100, cancellationToken); // Simulate processing

        return $@"IT Support Workflow Completed

Issue Type: {issue}
Description: {description}
Status: Resolved
Resolution: The issue has been analyzed and a solution has been applied.
Ticket ID: IT-{DateTime.Now:yyyyMMddHHmmss}

Next Steps:
- System has been configured appropriately
- Documentation has been updated
- Follow-up email sent to requester";
    }

    private async Task<string> ExecuteHRServiceWorkflow(WorkflowExecutionRequest request, CancellationToken cancellationToken)
    {
        // Simulate HR Service workflow with MAF
        var requestType = request.InputData.TryGetValue("request_type", out var reqType) 
            ? reqType.ToString() 
            : "General HR Request";

        await Task.Delay(100, cancellationToken); // Simulate processing

        return $@"HR Service Workflow Completed

Request Type: {requestType}
Status: Processed
Request ID: HR-{DateTime.Now:yyyyMMddHHmmss}

Actions Taken:
- Request has been reviewed by HR team
- Required approvals obtained
- Documents have been processed
- Notification sent to employee

Timeline: 2-3 business days for completion";
    }

    private async Task<string> ExecuteCustomerServiceWorkflow(WorkflowExecutionRequest request, CancellationToken cancellationToken)
    {
        // Simulate Customer Service workflow with MAF
        var serviceType = request.InputData.TryGetValue("service_type", out var svcType) 
            ? svcType.ToString() 
            : "General Inquiry";

        await Task.Delay(100, cancellationToken); // Simulate processing

        return $@"Customer Service Workflow Completed

Service Type: {serviceType}
Status: Resolved
Case ID: CS-{DateTime.Now:yyyyMMddHHmmss}

Resolution:
- Customer inquiry has been processed
- Solution has been provided
- Customer satisfaction confirmed
- Case documentation completed

Thank you for your patience!";
    }
}
