using zborek.Langfuse;

namespace BotSharp.Plugin.Langfuse.Services;

public class LangfuseService : IDisposable
{
    private readonly LangfuseSettings _settings;
    private readonly ILogger<LangfuseService> _logger;
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<string, string> _conversationTraces = new();
    private readonly ConcurrentDictionary<string, string> _agentSpans = new();

    public LangfuseService(
        LangfuseSettings settings, 
        ILogger<LangfuseService> logger,
        IServiceProvider services)
    {
        _settings = settings;
        _logger = logger;
        _services = services;

        if (_settings.Enabled && !string.IsNullOrEmpty(_settings.PublicKey) && !string.IsNullOrEmpty(_settings.SecretKey))
        {
            _logger.LogInformation("Langfuse integration enabled");
        }
        else
        {
            _logger.LogWarning("Langfuse is disabled or not configured properly");
        }
    }

    public bool IsEnabled => _settings.Enabled && !string.IsNullOrEmpty(_settings.PublicKey) && !string.IsNullOrEmpty(_settings.SecretKey);

    public string? CreateTrace(string conversationId, string userId, Dictionary<string, object>? metadata = null)
    {
        if (!IsEnabled) return null;

        try
        {
            var traceId = $"trace-{conversationId}";
            // Trace will be created by LangfuseTrace service on first use
            _conversationTraces[conversationId] = traceId;
            _logger.LogDebug("Registered trace for conversation {ConversationId}", conversationId);
            return traceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register trace for conversation {ConversationId}", conversationId);
            return null;
        }
    }

    public string? GetTraceId(string conversationId)
    {
        _conversationTraces.TryGetValue(conversationId, out var traceId);
        return traceId;
    }

    public string? CreateSpan(string traceId, string name, Dictionary<string, object>? metadata = null, Dictionary<string, object>? input = null)
    {
        if (!IsEnabled) return null;

        try
        {
            // Langfuse trace tracking via scoped service would go here
            // For now, just log the span creation
            var spanId = $"span-{Guid.NewGuid()}";
            _logger.LogDebug("Created span {SpanName} for trace {TraceId}", name, traceId);
            return spanId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create span {SpanName}", name);
            return null;
        }
    }

    public void UpdateSpan(string spanId, Dictionary<string, object>? output = null, Dictionary<string, object>? metadata = null, string? level = null)
    {
        if (!IsEnabled) return;

        try
        {
            _logger.LogDebug("Span {SpanId} logged", spanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log span {SpanId}", spanId);
        }
    }

    public void EndSpan(string spanId)
    {
        if (!IsEnabled) return;

        try
        {
            _logger.LogDebug("Span {SpanId} ended", spanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end span {SpanId}", spanId);
        }
    }

    public string? CreateGeneration(string traceId, string name, string model, Dictionary<string, object>? input = null, Dictionary<string, object>? metadata = null)
    {
        if (!IsEnabled) return null;

        try
        {
            // Langfuse generation tracking via scoped service would go here
            // For now, just log the generation creation
            var generationId = $"generation-{Guid.NewGuid()}";
            _logger.LogDebug("Created generation {GenerationName} for trace {TraceId}", name, traceId);
            return generationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create generation {GenerationName}", name);
            return null;
        }
    }

    public void UpdateGeneration(string generationId, Dictionary<string, object>? output = null, Dictionary<string, object>? metadata = null, Dictionary<string, object>? usage = null)
    {
        if (!IsEnabled) return;

        try
        {
            _logger.LogDebug("Generation {GenerationId} logged", generationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log generation {GenerationId}", generationId);
        }
    }

    public void SetAgentSpan(string agentId, string spanId)
    {
        _agentSpans[agentId] = spanId;
    }

    public string? GetAgentSpan(string agentId)
    {
        _agentSpans.TryGetValue(agentId, out var spanId);
        return spanId;
    }

    public void Flush()
    {
        if (!IsEnabled) return;

        try
        {
            _logger.LogDebug("Langfuse events flushed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush Langfuse events");
        }
    }

    public void Dispose()
    {
        try
        {
            Flush();
            _logger.LogInformation("Langfuse service disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing Langfuse service");
        }
    }
}
