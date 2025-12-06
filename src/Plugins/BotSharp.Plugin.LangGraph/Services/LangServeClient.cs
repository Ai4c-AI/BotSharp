using BotSharp.Plugin.LangGraph.Models;
using BotSharp.Plugin.LangGraph.Settings;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BotSharp.Plugin.LangGraph.Services;

/// <summary>
/// Service for communicating with LangServe REST API
/// </summary>
public class LangServeClient
{
    private readonly HttpClient _httpClient;
    private readonly LangGraphSettings _settings;
    private readonly ILogger<LangServeClient> _logger;

    public LangServeClient(
        HttpClient httpClient,
        LangGraphSettings settings,
        ILogger<LangServeClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.Timeout);

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }
    }

    /// <summary>
    /// Invoke LangGraph agent synchronously
    /// </summary>
    public async Task<LangServeResponse> InvokeAsync(
        string agentEndpoint,
        string conversationId,
        string userMessage,
        string? traceparent = null,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(conversationId, userMessage);
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/{agentEndpoint}/invoke")
        {
            Content = JsonContent.Create(request)
        };

        AddTracingHeaders(httpRequest, traceparent);

        try
        {
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LangServeResponse>(cancellationToken);
            return result ?? new LangServeResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking LangServe agent at {Endpoint}", agentEndpoint);
            throw;
        }
    }

    /// <summary>
    /// Stream responses from LangGraph agent
    /// </summary>
    public async IAsyncEnumerable<string> StreamAsync(
        string agentEndpoint,
        string conversationId,
        string userMessage,
        string? traceparent = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(conversationId, userMessage);
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/{agentEndpoint}/stream")
        {
            Content = JsonContent.Create(request)
        };

        AddTracingHeaders(httpRequest, traceparent);

        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                
                // Check for null (end of stream) or empty lines
                if (line == null || string.IsNullOrWhiteSpace(line))
                {
                    if (line == null)
                        break;
                    continue;
                }

                // Parse SSE format: "data: {...}"
                if (line.StartsWith("data: ") && line.Length > 6)
                {
                    var jsonData = line.Substring(6);
                    if (jsonData == "[DONE]")
                        break;

                    LangServeStreamChunk? chunk = null;
                    try
                    {
                        chunk = JsonSerializer.Deserialize<LangServeStreamChunk>(jsonData);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse stream chunk: {Data}", jsonData);
                        continue;
                    }

                    if (chunk?.Content != null)
                    {
                        yield return chunk.Content;
                    }
                }
            }
        }
        finally
        {
            response?.Dispose();
        }
    }

    /// <summary>
    /// Check if response indicates an interrupt (human-in-the-loop)
    /// </summary>
    public bool IsInterrupted(LangServeResponse response)
    {
        return response.Status == "interrupted" || response.Interrupt != null;
    }

    /// <summary>
    /// Resume execution after interrupt
    /// Note: LangGraph automatically resumes from the interrupted state when the same thread_id is used.
    /// The state is maintained by LangGraph's checkpointer, so we simply invoke with the user's response.
    /// </summary>
    public async Task<LangServeResponse> ResumeAsync(
        string agentEndpoint,
        string conversationId,
        string userResponse,
        string? traceparent = null,
        CancellationToken cancellationToken = default)
    {
        // Resume is handled by LangGraph's state management - just invoke with the same thread_id
        return await InvokeAsync(agentEndpoint, conversationId, userResponse, traceparent, cancellationToken);
    }

    private LangServeRequest BuildRequest(string conversationId, string userMessage)
    {
        return new LangServeRequest
        {
            Input = new LangServeInput
            {
                Messages = new List<LangServeMessage>
                {
                    new LangServeMessage
                    {
                        Type = "human",
                        Content = userMessage
                    }
                }
            },
            Config = new LangServeConfig
            {
                Configurable = new Dictionary<string, string>
                {
                    ["thread_id"] = $"botsharp_conversation_{conversationId}"
                }
            }
        };
    }

    private void AddTracingHeaders(HttpRequestMessage request, string? traceparent)
    {
        if (_settings.EnableTracing && !string.IsNullOrEmpty(traceparent))
        {
            request.Headers.Add("traceparent", traceparent);
        }
    }
}
