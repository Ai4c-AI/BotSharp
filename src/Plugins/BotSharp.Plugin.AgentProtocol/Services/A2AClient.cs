namespace BotSharp.Plugin.AgentProtocol.Services;

/// <summary>
/// Implementation of A2A client for agent-to-agent communication
/// </summary>
public class A2AClient : IA2AClient
{
    private readonly ILogger<A2AClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly AgentProtocolSettings _settings;
    private readonly IA2ACardResolver _cardResolver;
    private readonly Dictionary<string, Action<A2AInvokeResponse>> _callbacks = new();
    private readonly Dictionary<string, Action<A2AProgressNotification>> _progressCallbacks = new();
    private readonly Dictionary<string, A2AInvokeResponse> _pendingRequests = new();

    public A2AClient(
        ILogger<A2AClient> logger,
        IHttpClientFactory httpClientFactory,
        AgentProtocolSettings settings,
        IA2ACardResolver cardResolver)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _settings = settings;
        _cardResolver = cardResolver;
        
        if (_settings.TimeoutSeconds > 0)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }
    }

    public async Task<A2AInvokeResponse> InvokeAgentAsync(A2AInvokeRequest request)
    {
        try
        {
            _logger.LogInformation($"Invoking A2A agent: {request.AgentId}");

            // Get agent card to determine endpoint
            var endpoint = GetAgentEndpoint(request.AgentId);
            if (string.IsNullOrEmpty(endpoint))
            {
                _logger.LogError($"No endpoint found for agent: {request.AgentId}");
                return new A2AInvokeResponse
                {
                    Status = A2AResponseStatus.Failed,
                    ErrorMessage = $"No endpoint found for agent: {request.AgentId}"
                };
            }

            // Set callback URL if enabled
            if (_settings.EnableAsyncCallbacks && !string.IsNullOrEmpty(_settings.CallbackEndpoint))
            {
                request.CallbackUrl = _settings.CallbackEndpoint;
            }

            // Send request to remote agent
            var invokeEndpoint = endpoint.TrimEnd('/') + "/a2a/invoke";
            var requestJson = JsonSerializer.Serialize(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(invokeEndpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to invoke agent {request.AgentId}: {response.StatusCode}");
                return new A2AInvokeResponse
                {
                    Status = A2AResponseStatus.Failed,
                    ErrorMessage = $"HTTP {response.StatusCode}"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var invokeResponse = JsonSerializer.Deserialize<A2AInvokeResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (invokeResponse == null)
            {
                return new A2AInvokeResponse
                {
                    Status = A2AResponseStatus.Failed,
                    ErrorMessage = "Failed to deserialize response"
                };
            }

            // Store pending request for async tracking
            if (invokeResponse.IsAsync)
            {
                _pendingRequests[invokeResponse.RequestId] = invokeResponse;
            }

            _logger.LogInformation($"Agent {request.AgentId} invoked successfully. Status: {invokeResponse.Status}");
            return invokeResponse;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, $"Timeout invoking agent {request.AgentId}");
            return new A2AInvokeResponse
            {
                Status = A2AResponseStatus.Timeout,
                ErrorMessage = "Request timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error invoking agent {request.AgentId}");
            return new A2AInvokeResponse
            {
                Status = A2AResponseStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<A2AAgentCard> GetAgentCardAsync(string endpoint)
    {
        return await _cardResolver.ResolveCardAsync(endpoint);
    }

    public void RegisterCallback(string requestId, Action<A2AInvokeResponse> callback)
    {
        _callbacks[requestId] = callback;
        _logger.LogDebug($"Registered callback for request: {requestId}");
    }

    public void RegisterProgressCallback(string requestId, Action<A2AProgressNotification> progressCallback)
    {
        _progressCallbacks[requestId] = progressCallback;
        _logger.LogDebug($"Registered progress callback for request: {requestId}");
    }

    public async Task<bool> IsRequestCompletedAsync(string requestId)
    {
        if (!_pendingRequests.ContainsKey(requestId))
        {
            return true;
        }

        var pendingRequest = _pendingRequests[requestId];
        return pendingRequest.Status == A2AResponseStatus.Success || 
               pendingRequest.Status == A2AResponseStatus.Failed || 
               pendingRequest.Status == A2AResponseStatus.Timeout;
    }

    public async Task<A2AInvokeResponse?> GetRequestResultAsync(string requestId)
    {
        if (_pendingRequests.TryGetValue(requestId, out var result))
        {
            return result;
        }
        return null;
    }

    /// <summary>
    /// Handle progress notification from remote agent
    /// </summary>
    public void HandleProgressNotification(A2AProgressNotification notification)
    {
        _logger.LogInformation($"Received progress notification for request {notification.RequestId}: {notification.Progress}%");

        if (_progressCallbacks.TryGetValue(notification.RequestId, out var callback))
        {
            callback(notification);
        }

        if (notification.IsCompleted && _pendingRequests.ContainsKey(notification.RequestId))
        {
            var response = _pendingRequests[notification.RequestId];
            response.Status = A2AResponseStatus.Success;
            response.Response = notification.Result;
            response.Progress = 100;

            if (_callbacks.TryGetValue(notification.RequestId, out var responseCallback))
            {
                responseCallback(response);
            }

            // Cleanup
            _pendingRequests.Remove(notification.RequestId);
            _callbacks.Remove(notification.RequestId);
            _progressCallbacks.Remove(notification.RequestId);
        }
    }

    /// <summary>
    /// Handle async callback from remote agent
    /// </summary>
    public void HandleAsyncCallback(A2AInvokeResponse response)
    {
        _logger.LogInformation($"Received async callback for request: {response.RequestId}");

        if (_pendingRequests.ContainsKey(response.RequestId))
        {
            _pendingRequests[response.RequestId] = response;
        }

        if (_callbacks.TryGetValue(response.RequestId, out var callback))
        {
            callback(response);
            
            // Cleanup if completed
            if (response.Status != A2AResponseStatus.Pending)
            {
                _callbacks.Remove(response.RequestId);
                _progressCallbacks.Remove(response.RequestId);
                _pendingRequests.Remove(response.RequestId);
            }
        }
    }

    private string? GetAgentEndpoint(string agentId)
    {
        // Check configured remote agents first
        if (_settings.RemoteAgents.TryGetValue(agentId, out var endpoint))
        {
            return endpoint;
        }

        // Future enhancement: Query registry for endpoint if not found in configuration
        // This would enable fully dynamic agent discovery without pre-configuration
        return null;
    }
}
