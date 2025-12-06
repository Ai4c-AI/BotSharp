using BotSharp.Plugin.Dify.Models;
using BotSharp.Plugin.Dify.Settings;

namespace BotSharp.Plugin.Dify.Services;

/// <summary>
/// Service for interacting with Dify API
/// </summary>
public class DifyWorkflowService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DifySettings _settings;
    private readonly ILogger<DifyWorkflowService> _logger;

    public DifyWorkflowService(
        IHttpClientFactory httpClientFactory,
        DifySettings settings,
        ILogger<DifyWorkflowService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Execute a Dify workflow
    /// </summary>
    public async Task<DifyWorkflowResponse?> ExecuteWorkflowAsync(
        string workflowId,
        Dictionary<string, object> inputs,
        string userId = "default-user",
        bool async = false)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            var request = new DifyWorkflowRequest
            {
                Inputs = inputs,
                ResponseMode = async ? "streaming" : "blocking",
                User = userId
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var url = $"{_settings.BaseUrl}/v1/workflows/{workflowId}/run";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = requestContent
            };

            httpRequest.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");

            _logger.LogInformation($"Calling Dify workflow: {workflowId}");

            var response = await client.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Dify API error: {response.StatusCode} - {responseContent}");
                return new DifyWorkflowResponse
                {
                    Status = DifyWorkflowStatus.Failed,
                    Error = $"API error: {response.StatusCode}"
                };
            }

            var result = JsonSerializer.Deserialize<DifyWorkflowResponse>(responseContent);
            _logger.LogInformation($"Dify workflow execution result: {result?.Status}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing Dify workflow: {workflowId}");
            return new DifyWorkflowResponse
            {
                Status = DifyWorkflowStatus.Failed,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Check workflow execution status
    /// </summary>
    public async Task<DifyWorkflowResponse?> GetWorkflowStatusAsync(string workflowRunId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            var url = $"{_settings.BaseUrl}/v1/workflows/run/{workflowRunId}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");

            var response = await client.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Dify API error when checking status: {response.StatusCode}");
                return null;
            }

            return JsonSerializer.Deserialize<DifyWorkflowResponse>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking Dify workflow status: {workflowRunId}");
            return null;
        }
    }
}
