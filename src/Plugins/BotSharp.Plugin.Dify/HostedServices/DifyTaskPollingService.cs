using BotSharp.Plugin.Dify.Models;
using BotSharp.Plugin.Dify.Services;
using BotSharp.Plugin.Dify.Settings;

namespace BotSharp.Plugin.Dify.HostedServices;

/// <summary>
/// Background service for polling Dify workflow task status
/// </summary>
public class DifyTaskPollingService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DifyTaskPollingService> _logger;

    public DifyTaskPollingService(
        IServiceProvider services,
        ILogger<DifyTaskPollingService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dify Task Polling Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dify task polling service");
            }

            // Use scoped settings to determine polling interval
            using var scope = _services.CreateScope();
            var settings = scope.ServiceProvider.GetRequiredService<DifySettings>();
            await Task.Delay(TimeSpan.FromSeconds(settings.PollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Dify Task Polling Service stopped");
    }

    private async Task PollTasksAsync()
    {
        using var scope = _services.CreateScope();
        var taskStorage = scope.ServiceProvider.GetRequiredService<DifyTaskStorageService>();
        var difyService = scope.ServiceProvider.GetRequiredService<DifyWorkflowService>();
        var settings = scope.ServiceProvider.GetRequiredService<DifySettings>();

        var runningTasks = taskStorage.GetRunningTasks();

        if (runningTasks.Count == 0)
        {
            return;
        }

        _logger.LogInformation($"Polling {runningTasks.Count} running Dify tasks");

        foreach (var task in runningTasks)
        {
            try
            {
                await PollSingleTaskAsync(task, difyService, taskStorage, settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error polling task {task.TaskId}");
            }
        }
    }

    private async Task PollSingleTaskAsync(
        DifyWorkflowTask task,
        DifyWorkflowService difyService,
        DifyTaskStorageService taskStorage,
        DifySettings settings)
    {
        task.PollingAttempts++;

        // Check if max attempts reached
        if (task.PollingAttempts > settings.MaxPollingAttempts)
        {
            _logger.LogWarning($"Task {task.TaskId} exceeded max polling attempts");
            task.Status = DifyWorkflowStatus.Failed;
            task.ErrorMessage = "Polling timeout exceeded";
            taskStorage.UpdateTask(task);
            await OnTaskCompletedAsync(task);
            return;
        }

        // Query Dify API for status
        var response = await difyService.GetWorkflowStatusAsync(task.WorkflowRunId);

        if (response == null)
        {
            _logger.LogWarning($"Failed to get status for task {task.TaskId}");
            return;
        }

        // Update task based on response
        if (response.Status == DifyWorkflowStatus.Succeeded)
        {
            task.Status = DifyWorkflowStatus.Succeeded;
            task.ResultData = JsonSerializer.Serialize(response.Data);
            taskStorage.UpdateTask(task);
            _logger.LogInformation($"Task {task.TaskId} completed successfully");
            await OnTaskCompletedAsync(task);
        }
        else if (response.Status == DifyWorkflowStatus.Failed)
        {
            task.Status = DifyWorkflowStatus.Failed;
            task.ErrorMessage = response.Error;
            taskStorage.UpdateTask(task);
            _logger.LogWarning($"Task {task.TaskId} failed: {response.Error}");
            await OnTaskCompletedAsync(task);
        }
        else if (response.Status == DifyWorkflowStatus.Running)
        {
            _logger.LogDebug($"Task {task.TaskId} still running (attempt {task.PollingAttempts})");
            taskStorage.UpdateTask(task);
        }
    }

    private async Task OnTaskCompletedAsync(DifyWorkflowTask task)
    {
        // Here you would resume the suspended conversation/session
        // This is a placeholder for the actual implementation
        _logger.LogInformation($"Task {task.TaskId} completed, would resume conversation {task.ConversationId}");

        // In a real implementation, you would:
        // 1. Load the conversation context
        // 2. Resume the conversation with the workflow result
        // 3. Continue the agent's execution flow

        await Task.CompletedTask;
    }
}
