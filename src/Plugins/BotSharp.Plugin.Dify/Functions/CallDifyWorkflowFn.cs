using BotSharp.Abstraction.Agents.Models;
using BotSharp.Plugin.Dify.Models;
using BotSharp.Plugin.Dify.Services;
using BotSharp.Plugin.Dify.Settings;

namespace BotSharp.Plugin.Dify.Functions;

/// <summary>
/// Function to execute Dify workflows
/// </summary>
public class CallDifyWorkflowFn : IFunctionCallback
{
    public string Name => "dify-workflow-execute";
    public string Indication => "Executing Dify workflow...";

    private readonly IServiceProvider _services;
    private readonly DifyWorkflowService _difyService;
    private readonly DifyTaskStorageService _taskStorage;
    private readonly DifySettings _settings;
    private readonly ILogger<CallDifyWorkflowFn> _logger;

    public CallDifyWorkflowFn(
        IServiceProvider services,
        DifyWorkflowService difyService,
        DifyTaskStorageService taskStorage,
        DifySettings settings,
        ILogger<CallDifyWorkflowFn> logger)
    {
        _services = services;
        _difyService = difyService;
        _taskStorage = taskStorage;
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        try
        {
            var args = JsonSerializer.Deserialize<DifyWorkflowArgs>(message.FunctionArgs ?? "{}");
            if (args == null || string.IsNullOrEmpty(args.WorkflowId))
            {
                message.Content = "Error: Workflow ID is required";
                return false;
            }

            // Parse inputs
            var inputs = ParseInputs(args.Inputs);

            if (args.Async)
            {
                // Execute asynchronously
                return await ExecuteAsync(message, args, inputs);
            }
            else
            {
                // Execute synchronously (blocking)
                return await ExecuteSync(message, args, inputs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Dify workflow function");
            message.Content = $"Error: {ex.Message}";
            return false;
        }
    }

    private async Task<bool> ExecuteSync(
        RoleDialogModel message,
        DifyWorkflowArgs args,
        Dictionary<string, object> inputs)
    {
        var result = await _difyService.ExecuteWorkflowAsync(
            args.WorkflowId,
            inputs,
            message.SenderId ?? "default-user",
            async: false);

        if (result == null || result.Status == DifyWorkflowStatus.Failed)
        {
            message.Content = $"Workflow execution failed: {result?.Error ?? "Unknown error"}";
            return false;
        }

        // Extract output data
        var outputData = ExtractOutputData(result);
        message.Content = outputData;
        return true;
    }

    private async Task<bool> ExecuteAsync(
        RoleDialogModel message,
        DifyWorkflowArgs args,
        Dictionary<string, object> inputs)
    {
        var result = await _difyService.ExecuteWorkflowAsync(
            args.WorkflowId,
            inputs,
            message.SenderId ?? "default-user",
            async: true);

        if (result == null)
        {
            message.Content = "Failed to start workflow execution";
            return false;
        }

        // Create task for tracking
        var taskId = result.TaskId ?? Guid.NewGuid().ToString();
        if (result.TaskId == null)
        {
            _logger.LogWarning($"Dify API did not provide TaskId, generating fallback ID: {taskId}");
        }

        var task = new DifyWorkflowTask
        {
            TaskId = taskId,
            WorkflowRunId = result.WorkflowRunId,
            WorkflowId = args.WorkflowId,
            ConversationId = args.ConversationId ?? message.MessageId,
            AgentId = message.CurrentAgentId ?? string.Empty,
            MessageId = message.MessageId,
            Status = DifyWorkflowStatus.Running
        };

        _taskStorage.StoreTask(task);

        // Set pending message
        message.Content = $"Workflow started with task ID: {task.TaskId}. Processing in background...";
        message.Data = new { TaskId = task.TaskId, Status = "pending" };

        return true;
    }

    private Dictionary<string, object> ParseInputs(string? inputsJson)
    {
        if (string.IsNullOrEmpty(inputsJson))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            var inputs = JsonSerializer.Deserialize<Dictionary<string, object>>(inputsJson);
            return inputs ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to parse inputs JSON: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    private string ExtractOutputData(DifyWorkflowResponse response)
    {
        if (response.Data?.Outputs != null)
        {
            return JsonSerializer.Serialize(response.Data.Outputs);
        }

        if (response.Data?.ExtensionData != null)
        {
            return JsonSerializer.Serialize(response.Data.ExtensionData);
        }

        return "Workflow completed successfully";
    }
}
