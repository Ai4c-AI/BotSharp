namespace BotSharp.Plugin.Langfuse.Hooks;

public class LangfuseContentGeneratingHook : IContentGeneratingHook
{
    private readonly LangfuseService _langfuseService;
    private readonly ILogger<LangfuseContentGeneratingHook> _logger;
    private string? _currentGenerationId;

    public LangfuseContentGeneratingHook(
        LangfuseService langfuseService,
        ILogger<LangfuseContentGeneratingHook> logger)
    {
        _langfuseService = langfuseService;
        _logger = logger;
    }

    public async Task BeforeGenerating(Agent agent, List<RoleDialogModel> conversations)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            // TODO: Get conversation ID from the context
            // For now, skip trace ID resolution in content hook as we don't have access to conversation context
            _logger.LogDebug("Skipping generation tracking for agent {AgentName} - no conversation context", agent.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BeforeGenerating hook");
        }

        await Task.CompletedTask;
    }

    public async Task AfterGenerated(RoleDialogModel message, TokenStatsModel tokenStats)
    {
        if (!_langfuseService.IsEnabled || string.IsNullOrEmpty(_currentGenerationId)) return;

        try
        {
            var output = new Dictionary<string, object>
            {
                ["content"] = message.Content ?? "",
                ["role"] = message.Role,
                ["message_id"] = message.MessageId ?? ""
            };

            if (!string.IsNullOrEmpty(message.FunctionName))
            {
                output["function_name"] = message.FunctionName;
                output["function_args"] = message.FunctionArgs ?? "";
            }

            var usage = new Dictionary<string, object>
            {
                ["prompt_tokens"] = tokenStats.TotalInputTokens,
                ["completion_tokens"] = tokenStats.TotalOutputTokens,
                ["total_tokens"] = tokenStats.TotalInputTokens + tokenStats.TotalOutputTokens
            };

            var metadata = new Dictionary<string, object>
            {
                ["has_function_call"] = !string.IsNullOrEmpty(message.FunctionName)
            };

            _langfuseService.UpdateGeneration(
                generationId: _currentGenerationId,
                output: output,
                usage: usage,
                metadata: metadata
            );

            _logger.LogDebug("Updated generation with response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AfterGenerated hook");
        }
        finally
        {
            _currentGenerationId = null;
        }

        await Task.CompletedTask;
    }

    public async Task BeforeFunctionInvoked(RoleDialogModel message, TokenStatsModel tokenStats)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            // TODO: Track function invocation - skipped for now as we don't have conversation context
            _logger.LogDebug("Skipping function tracking for {FunctionName}", message.FunctionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BeforeFunctionInvoked hook");
        }

        await Task.CompletedTask;
    }

    public async Task AfterFunctionInvoked(RoleDialogModel message, TokenStatsModel tokenStats)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            _logger.LogDebug("Function {FunctionName} execution completed", message.FunctionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AfterFunctionInvoked hook");
        }

        await Task.CompletedTask;
    }
}
