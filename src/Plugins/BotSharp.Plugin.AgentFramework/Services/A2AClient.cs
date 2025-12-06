using BotSharp.Plugin.AgentFramework.Models;
using BotSharp.Plugin.AgentFramework.Settings;
using System.Net.Http.Json;
using System.Text.Json;

namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Implementation of A2A (Agent-to-Agent) client for MAF integration
/// </summary>
public class A2AClient : IA2AClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly AgentFrameworkSettings _settings;
    private readonly ILogger<A2AClient> _logger;

    public A2AClient(HttpClient httpClient, string baseUrl, AgentFrameworkSettings settings, ILogger<A2AClient> logger)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _settings = settings;
        _logger = logger;
    }

    public async Task<AgentCard> GetAgentCardAsync()
    {
        try
        {
            var url = $"{_baseUrl}/.well-known/agent-card.json";
            _logger.LogInformation("Fetching agent card from {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var agentCard = await response.Content.ReadFromJsonAsync<AgentCard>();
            if (agentCard == null)
            {
                throw new InvalidOperationException("Failed to deserialize agent card");
            }
            
            _logger.LogInformation("Successfully retrieved agent card for {AgentName}", agentCard.Name);
            return agentCard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve agent card from {BaseUrl}", _baseUrl);
            throw;
        }
    }

    public async Task<TaskResult> SendTaskAsync(string input, object? context = null)
    {
        try
        {
            var request = new A2ARequest
            {
                Method = "sendTask",
                Params = new SendTaskParams
                {
                    Input = input,
                    Context = context
                }
            };

            var url = $"{_baseUrl}/a2a/tasks";
            _logger.LogInformation("Sending task to {Url}", url);
            
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<A2AResponse<TaskResult>>();
            if (result?.Result == null)
            {
                throw new InvalidOperationException("Failed to deserialize task result");
            }

            if (result.Error != null)
            {
                throw new InvalidOperationException($"A2A error: {result.Error.Message}");
            }
            
            _logger.LogInformation("Task submitted successfully. TaskId: {TaskId}, Status: {Status}", 
                result.Result.TaskId, result.Result.Status);
            
            return result.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task to {BaseUrl}", _baseUrl);
            throw;
        }
    }

    public async Task<TaskResult> GetTaskAsync(string taskId)
    {
        try
        {
            var request = new A2ARequest
            {
                Method = "getTask",
                Params = new GetTaskParams
                {
                    TaskId = taskId
                }
            };

            var url = $"{_baseUrl}/a2a/tasks";
            _logger.LogDebug("Getting task status for {TaskId}", taskId);
            
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<A2AResponse<TaskResult>>();
            if (result?.Result == null)
            {
                throw new InvalidOperationException("Failed to deserialize task result");
            }

            if (result.Error != null)
            {
                throw new InvalidOperationException($"A2A error: {result.Error.Message}");
            }
            
            return result.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task status for {TaskId}", taskId);
            throw;
        }
    }

    public async Task<TaskResult> PollTaskCompletionAsync(string taskId, int pollingIntervalMs = 2000, int maxAttempts = 60)
    {
        _logger.LogInformation("Starting to poll task {TaskId} with interval {IntervalMs}ms, max attempts: {MaxAttempts}", 
            taskId, pollingIntervalMs, maxAttempts);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var result = await GetTaskAsync(taskId);
            
            _logger.LogDebug("Poll attempt {Attempt}/{MaxAttempts} - Status: {Status}, Progress: {Progress}", 
                attempt + 1, maxAttempts, result.Status, result.Progress);

            if (result.Status == A2ATaskStatus.Completed)
            {
                _logger.LogInformation("Task {TaskId} completed successfully", taskId);
                return result;
            }

            if (result.Status == A2ATaskStatus.Failed)
            {
                _logger.LogError("Task {TaskId} failed: {Error}", taskId, result.Error);
                throw new InvalidOperationException($"Task failed: {result.Error}");
            }

            if (attempt < maxAttempts - 1)
            {
                await Task.Delay(pollingIntervalMs);
            }
        }

        throw new TimeoutException($"Task {taskId} did not complete within the expected time");
    }
}
