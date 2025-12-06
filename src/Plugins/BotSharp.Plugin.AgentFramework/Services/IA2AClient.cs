using BotSharp.Plugin.AgentFramework.Models;

namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Interface for A2A (Agent-to-Agent) client operations
/// </summary>
public interface IA2AClient
{
    /// <summary>
    /// Gets the agent card from the remote service
    /// </summary>
    /// <returns>Agent card metadata</returns>
    Task<AgentCard> GetAgentCardAsync();

    /// <summary>
    /// Sends a task to the remote agent
    /// </summary>
    /// <param name="input">User input/request</param>
    /// <param name="context">Optional conversation context</param>
    /// <returns>Task result with ID and status</returns>
    Task<TaskResult> SendTaskAsync(string input, object? context = null);

    /// <summary>
    /// Gets the status and result of a task
    /// </summary>
    /// <param name="taskId">Task ID to query</param>
    /// <returns>Task result with current status</returns>
    Task<TaskResult> GetTaskAsync(string taskId);

    /// <summary>
    /// Polls for task completion
    /// </summary>
    /// <param name="taskId">Task ID to poll</param>
    /// <param name="pollingIntervalMs">Interval between polls in milliseconds</param>
    /// <param name="maxAttempts">Maximum number of polling attempts</param>
    /// <returns>Completed task result</returns>
    Task<TaskResult> PollTaskCompletionAsync(string taskId, int pollingIntervalMs = 2000, int maxAttempts = 60);
}
