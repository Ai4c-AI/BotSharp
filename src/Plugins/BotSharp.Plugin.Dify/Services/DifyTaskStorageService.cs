using BotSharp.Plugin.Dify.Models;

namespace BotSharp.Plugin.Dify.Services;

/// <summary>
/// In-memory storage for Dify workflow tasks
/// In production, this should be replaced with persistent storage
/// </summary>
public class DifyTaskStorageService
{
    private readonly Dictionary<string, DifyWorkflowTask> _tasks = new();
    private readonly object _lock = new object();
    private readonly ILogger<DifyTaskStorageService> _logger;

    public DifyTaskStorageService(ILogger<DifyTaskStorageService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Store a new task
    /// </summary>
    public void StoreTask(DifyWorkflowTask task)
    {
        lock (_lock)
        {
            _tasks[task.TaskId] = task;
            _logger.LogInformation($"Stored Dify task: {task.TaskId}, Status: {task.Status}");
        }
    }

    /// <summary>
    /// Get a task by ID
    /// </summary>
    public DifyWorkflowTask? GetTask(string taskId)
    {
        lock (_lock)
        {
            return _tasks.TryGetValue(taskId, out var task) ? task : null;
        }
    }

    /// <summary>
    /// Update task status
    /// </summary>
    public void UpdateTask(DifyWorkflowTask task)
    {
        lock (_lock)
        {
            if (_tasks.ContainsKey(task.TaskId))
            {
                task.UpdatedAt = DateTime.UtcNow;
                _tasks[task.TaskId] = task;
                _logger.LogInformation($"Updated Dify task: {task.TaskId}, Status: {task.Status}");
            }
        }
    }

    /// <summary>
    /// Get all running tasks
    /// </summary>
    public List<DifyWorkflowTask> GetRunningTasks()
    {
        lock (_lock)
        {
            return _tasks.Values
                .Where(t => t.Status == DifyWorkflowStatus.Running)
                .ToList();
        }
    }

    /// <summary>
    /// Remove a task
    /// </summary>
    public void RemoveTask(string taskId)
    {
        lock (_lock)
        {
            if (_tasks.Remove(taskId))
            {
                _logger.LogInformation($"Removed Dify task: {taskId}");
            }
        }
    }

    /// <summary>
    /// Get all tasks for a conversation
    /// </summary>
    public List<DifyWorkflowTask> GetTasksByConversation(string conversationId)
    {
        lock (_lock)
        {
            return _tasks.Values
                .Where(t => t.ConversationId == conversationId)
                .ToList();
        }
    }
}
