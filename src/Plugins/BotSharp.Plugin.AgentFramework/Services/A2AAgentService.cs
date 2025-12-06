using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Agents.Enums;
using BotSharp.Plugin.AgentFramework.Settings;

namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Implementation of A2A agent service for invoking remote MAF agents
/// </summary>
public class A2AAgentService : IA2AAgentService
{
    private readonly IA2AClientFactory _clientFactory;
    private readonly AgentFrameworkSettings _settings;
    private readonly ILogger<A2AAgentService> _logger;

    public A2AAgentService(
        IA2AClientFactory clientFactory,
        AgentFrameworkSettings settings,
        ILogger<A2AAgentService> logger)
    {
        _clientFactory = clientFactory;
        _settings = settings;
        _logger = logger;
    }

    public async Task<string> InvokeAgentAsync(Agent agent, string input, List<RoleDialogModel>? conversationHistory = null)
    {
        if (agent.Type != AgentType.A2ARemote)
        {
            throw new InvalidOperationException($"Agent {agent.Id} is not an A2A remote agent");
        }

        // Get endpoint from agent configuration
        if (!agent.TemplateDict.TryGetValue("a2a_endpoint", out var endpointObj) || endpointObj == null)
        {
            throw new InvalidOperationException($"Agent {agent.Id} has no a2a_endpoint configured");
        }

        var endpoint = endpointObj.ToString();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException($"Agent {agent.Id} has empty a2a_endpoint");
        }

        _logger.LogInformation("Invoking remote A2A agent {AgentId} at {Endpoint}", agent.Id, endpoint);

        // Create client for this endpoint
        var client = _clientFactory.CreateClient(endpoint);

        // Prepare context from conversation history if provided
        object? context = null;
        if (conversationHistory != null && conversationHistory.Any())
        {
            // Convert conversation history to a format suitable for A2A
            context = new
            {
                history = conversationHistory.Select(d => new
                {
                    role = d.Role,
                    content = d.Content,
                    created_at = d.CreatedAt
                }).ToList()
            };
        }

        // Send task to remote agent
        var taskResult = await client.SendTaskAsync(input, context);

        _logger.LogInformation("Task submitted to remote agent. TaskId: {TaskId}, Status: {Status}", 
            taskResult.TaskId, taskResult.Status);

        // If task is not immediately completed, poll for completion
        if (taskResult.Status != "completed")
        {
            var pollingInterval = _settings.PollingIntervalMs;
            var maxAttempts = _settings.MaxPollingAttempts;

            _logger.LogInformation("Polling for task completion. Interval: {IntervalMs}ms, Max attempts: {MaxAttempts}", 
                pollingInterval, maxAttempts);

            taskResult = await client.PollTaskCompletionAsync(taskResult.TaskId, pollingInterval, maxAttempts);
        }

        // Return the output from the completed task
        if (string.IsNullOrWhiteSpace(taskResult.Output))
        {
            _logger.LogWarning("Task {TaskId} completed but has no output", taskResult.TaskId);
            return "The remote agent completed the task but provided no response.";
        }

        _logger.LogInformation("Successfully received response from remote A2A agent {AgentId}", agent.Id);
        return taskResult.Output;
    }
}
