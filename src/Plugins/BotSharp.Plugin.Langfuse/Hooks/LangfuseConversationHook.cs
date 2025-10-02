namespace BotSharp.Plugin.Langfuse.Hooks;

public class LangfuseConversationHook : ConversationHookBase
{
    private readonly LangfuseService _langfuseService;
    private readonly ILogger<LangfuseConversationHook> _logger;

    public LangfuseConversationHook(
        LangfuseService langfuseService,
        ILogger<LangfuseConversationHook> logger)
    {
        _langfuseService = langfuseService;
        _logger = logger;
        Priority = 10;
    }

    public override async Task OnConversationInitialized(Conversation conversation)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["agent_id"] = conversation.AgentId ?? "",
                ["channel"] = conversation.Channel ?? "",
                ["created_time"] = conversation.CreatedTime
            };

            var traceId = _langfuseService.CreateTrace(
                conversationId: conversation.Id,
                userId: conversation.UserId ?? "anonymous",
                metadata: metadata
            );

            _logger.LogInformation("Initialized Langfuse trace for conversation {ConversationId}", conversation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Langfuse trace for conversation {ConversationId}", conversation.Id);
        }

        await base.OnConversationInitialized(conversation);
    }

    public override async Task OnMessageReceived(RoleDialogModel message)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            var conversationId = Conversation?.Id;
            if (string.IsNullOrEmpty(conversationId)) return;

            var traceId = _langfuseService.GetTraceId(conversationId);
            if (string.IsNullOrEmpty(traceId)) return;

            var metadata = new Dictionary<string, object>
            {
                ["role"] = message.Role,
                ["message_id"] = message.MessageId ?? "",
                ["created_at"] = message.CreatedAt
            };

            var input = new Dictionary<string, object>
            {
                ["content"] = message.Content ?? ""
            };

            var spanId = _langfuseService.CreateSpan(
                traceId: traceId,
                name: $"Message-{message.Role}",
                metadata: metadata,
                input: input
            );

            if (!string.IsNullOrEmpty(spanId))
            {
                _langfuseService.EndSpan(spanId);
            }

            _logger.LogDebug("Tracked message received for conversation {ConversationId}", conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking message received");
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

            var metadata = new Dictionary<string, object>
            {
                ["function_name"] = message.FunctionName ?? "",
                ["message_id"] = message.MessageId ?? "",
                ["from"] = options?.From ?? "Unknown"
            };

            var input = new Dictionary<string, object>
            {
                ["args"] = message.FunctionArgs ?? ""
            };

            _langfuseService.CreateSpan(
                traceId: traceId,
                name: $"Function-Execute-{message.FunctionName}",
                metadata: metadata,
                input: input
            );

            _logger.LogDebug("Tracking function execution: {FunctionName}", message.FunctionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking function execution");
        }

        await base.OnFunctionExecuting(message, options);
    }

    public override async Task OnFunctionExecuted(RoleDialogModel message, InvokeFunctionOptions? options = null)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            _logger.LogDebug("Function executed: {FunctionName}", message.FunctionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnFunctionExecuted hook");
        }

        await base.OnFunctionExecuted(message, options);
    }

    public override async Task OnResponseGenerated(RoleDialogModel message)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            var conversationId = Conversation?.Id;
            if (string.IsNullOrEmpty(conversationId)) return;

            var traceId = _langfuseService.GetTraceId(conversationId);
            if (string.IsNullOrEmpty(traceId)) return;

            var metadata = new Dictionary<string, object>
            {
                ["role"] = message.Role,
                ["message_id"] = message.MessageId ?? "",
                ["has_rich_content"] = message.RichContent != null
            };

            var output = new Dictionary<string, object>
            {
                ["content"] = message.Content ?? ""
            };

            var spanId = _langfuseService.CreateSpan(
                traceId: traceId,
                name: "Response-Generated",
                metadata: metadata,
                input: output
            );

            if (!string.IsNullOrEmpty(spanId))
            {
                _langfuseService.EndSpan(spanId);
            }

            _logger.LogDebug("Tracked response generated for conversation {ConversationId}", conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking response generated");
        }

        await base.OnResponseGenerated(message);
    }

    public override async Task OnUserAgentConnectedInitially(Conversation conversation)
    {
        await base.OnUserAgentConnectedInitially(conversation);
    }

    public override async Task OnDialogsLoaded(List<RoleDialogModel> dialogs)
    {
        await base.OnDialogsLoaded(dialogs);
    }

    public override async Task OnDialogRecordLoaded(RoleDialogModel dialog)
    {
        await base.OnDialogRecordLoaded(dialog);
    }

    public override async Task OnStateLoaded(ConversationState state)
    {
        await base.OnStateLoaded(state);
    }

    public override async Task OnStateChanged(StateChangeModel stateChange)
    {
        await base.OnStateChanged(stateChange);
    }

    public override async Task OnPostbackMessageReceived(RoleDialogModel message, PostbackMessageModel replyMsg)
    {
        await base.OnPostbackMessageReceived(message, replyMsg);
    }

    public override async Task OnConversationEnding(RoleDialogModel message)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            _langfuseService.Flush();
            _logger.LogInformation("Flushed Langfuse events for ending conversation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing Langfuse events");
        }

        await base.OnConversationEnding(message);
    }

    public override async Task OnHumanInterventionNeeded(RoleDialogModel message)
    {
        await base.OnHumanInterventionNeeded(message);
    }

    public override async Task OnMessageDeleted(string conversationId, string messageId)
    {
        await base.OnMessageDeleted(conversationId, messageId);
    }

    public override async Task OnBreakpointUpdated(string conversationId, bool resetStates)
    {
        await base.OnBreakpointUpdated(conversationId, resetStates);
    }

    public override async Task OnNotificationGenerated(RoleDialogModel message)
    {
        await base.OnNotificationGenerated(message);
    }
}
