namespace BotSharp.Plugin.Langfuse.Hooks;

/// <summary>
/// Hook to track agent routing in Langfuse
/// This captures the multi-agent orchestration flow
/// </summary>
public class LangfuseRoutingHook : ConversationHookBase
{
    private readonly LangfuseService _langfuseService;
    private readonly ILogger<LangfuseRoutingHook> _logger;
    private readonly IRoutingService _routingService;

    public LangfuseRoutingHook(
        LangfuseService langfuseService,
        ILogger<LangfuseRoutingHook> logger,
        IRoutingService routingService)
    {
        _langfuseService = langfuseService;
        _logger = logger;
        _routingService = routingService;
        Priority = 5;
    }

    public override async Task OnMessageReceived(RoleDialogModel message)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            // Track routing context
            var context = _routingService.Context;
            if (context != null)
            {
                var conversationId = Conversation?.Id;
                if (string.IsNullOrEmpty(conversationId)) return;

                var traceId = _langfuseService.GetTraceId(conversationId);
                if (string.IsNullOrEmpty(traceId)) return;

                var currentAgentId = context.GetCurrentAgentId();
                var metadata = new Dictionary<string, object>
                {
                    ["current_agent_id"] = currentAgentId ?? "unknown",
                    ["router_agent"] = _routingService.Router?.Name ?? "unknown",
                    ["message_role"] = message.Role
                };

                var spanId = _langfuseService.CreateSpan(
                    traceId: traceId,
                    name: $"Routing-MessageReceived",
                    metadata: metadata,
                    input: new Dictionary<string, object>
                    {
                        ["content"] = message.Content ?? ""
                    }
                );

                if (!string.IsNullOrEmpty(spanId))
                {
                    _langfuseService.EndSpan(spanId);
                }

                _logger.LogDebug("Tracked routing for message in conversation {ConversationId}", conversationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking routing in OnMessageReceived");
        }

        await base.OnMessageReceived(message);
    }

    public override async Task OnFunctionExecuting(RoleDialogModel message, InvokeFunctionOptions? options = null)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            var conversationId = Conversation?.Id;
            if (string.IsNullOrEmpty(conversationId)) return;

            var traceId = _langfuseService.GetTraceId(conversationId);
            if (string.IsNullOrEmpty(traceId)) return;

            var currentAgentId = _routingService.Context?.GetCurrentAgentId();
            var metadata = new Dictionary<string, object>
            {
                ["function_name"] = message.FunctionName ?? "",
                ["current_agent_id"] = currentAgentId ?? "unknown",
                ["from_source"] = options?.From ?? "Unknown"
            };

            _langfuseService.CreateSpan(
                traceId: traceId,
                name: $"Routing-FunctionExecution-{message.FunctionName}",
                metadata: metadata,
                input: new Dictionary<string, object>
                {
                    ["args"] = message.FunctionArgs ?? ""
                }
            );

            _logger.LogDebug("Tracked routing function execution: {FunctionName}", message.FunctionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking routing in OnFunctionExecuting");
        }

        await base.OnFunctionExecuting(message, options);
    }
}
