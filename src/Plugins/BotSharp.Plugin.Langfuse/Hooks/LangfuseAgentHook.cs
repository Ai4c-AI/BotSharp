namespace BotSharp.Plugin.Langfuse.Hooks;

public class LangfuseAgentHook : AgentHookBase
{
    private readonly LangfuseService _langfuseService;
    private readonly ILogger<LangfuseAgentHook> _logger;

    public LangfuseAgentHook(
        IServiceProvider services,
        AgentSettings settings,
        LangfuseService langfuseService,
        ILogger<LangfuseAgentHook> logger) : base(services, settings)
    {
        _langfuseService = langfuseService;
        _logger = logger;
    }

    public override bool OnAgentLoading(ref string id)
    {
        if (!_langfuseService.IsEnabled) return false;

        try
        {
            _logger.LogDebug("Agent loading: {AgentId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnAgentLoading hook");
        }

        return false;
    }

    public override void OnAgentLoaded(Agent agent)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            _logger.LogDebug("Agent loaded: {AgentName} ({AgentId})", agent.Name, agent.Id);
            
            // Track agent metadata
            var metadata = new Dictionary<string, object>
            {
                ["agent_name"] = agent.Name,
                ["agent_id"] = agent.Id,
                ["agent_type"] = agent.Type.ToString(),
                ["is_disabled"] = agent.Disabled,
                ["has_routing_rules"] = agent.RoutingRules?.Any() ?? false
            };

            _logger.LogInformation("Tracked agent load: {AgentName}", agent.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnAgentLoaded hook");
        }
    }

    public override bool OnInstructionLoaded(string template, Dictionary<string, object> dict)
    {
        if (!_langfuseService.IsEnabled) return false;

        try
        {
            _logger.LogDebug("Instruction loaded for agent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnInstructionLoaded hook");
        }

        return false;
    }

    public override bool OnFunctionsLoaded(List<FunctionDef> functions)
    {
        if (!_langfuseService.IsEnabled) return false;

        try
        {
            _logger.LogDebug("Functions loaded: {FunctionCount}", functions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnFunctionsLoaded hook");
        }

        return false;
    }

    public override bool OnSamplesLoaded(List<string> samples)
    {
        if (!_langfuseService.IsEnabled) return false;

        try
        {
            _logger.LogDebug("Samples loaded: {SampleCount}", samples.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnSamplesLoaded hook");
        }

        return false;
    }

    public override void OnAgentUtilityLoaded(Agent agent)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            _logger.LogDebug("Agent utility loaded for {AgentName}", agent.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnAgentUtilityLoaded hook");
        }
    }

    public override void OnAgentMcpToolLoaded(Agent agent)
    {
        if (!_langfuseService.IsEnabled) return;

        try
        {
            _logger.LogDebug("Agent MCP tool loaded for {AgentName}", agent.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnAgentMcpToolLoaded hook");
        }
    }
}
