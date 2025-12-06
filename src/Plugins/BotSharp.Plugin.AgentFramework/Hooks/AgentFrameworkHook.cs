using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Agents.Enums;
using BotSharp.Abstraction.Agents.Settings;
using BotSharp.Plugin.AgentFramework.Services;
using BotSharp.Plugin.AgentFramework.Settings;

namespace BotSharp.Plugin.AgentFramework.Hooks;

/// <summary>
/// Agent hook for integrating with Microsoft Agent Framework (MAF) via A2A protocol
/// </summary>
public class AgentFrameworkHook : AgentHookBase
{
    private readonly A2ACardResolver _cardResolver;
    private readonly AgentFrameworkSettings _frameworkSettings;
    private readonly ILogger<AgentFrameworkHook> _logger;

    public override string SelfId => string.Empty;

    public AgentFrameworkHook(
        IServiceProvider services,
        AgentSettings agentSettings,
        A2ACardResolver cardResolver,
        AgentFrameworkSettings settings,
        ILogger<AgentFrameworkHook> logger) : base(services, agentSettings)
    {
        _cardResolver = cardResolver;
        _frameworkSettings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Called when agent is loaded. If agent type is "a2a-remote", fetch and update agent metadata
    /// </summary>
    public override async void OnAgentLoaded(Agent agent)
    {
        if (!_frameworkSettings.Enabled)
        {
            return;
        }

        // Check if this is an A2A remote agent
        if (agent.Type != AgentType.A2ARemote)
        {
            return;
        }

        try
        {
            // Get the endpoint from TemplateDict
            if (!agent.TemplateDict.TryGetValue("a2a_endpoint", out var endpointObj) || endpointObj == null)
            {
                _logger.LogWarning("Agent {AgentId} is marked as a2a-remote but has no a2a_endpoint in TemplateDict", agent.Id);
                return;
            }

            var endpoint = endpointObj.ToString();
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogWarning("Agent {AgentId} has empty a2a_endpoint", agent.Id);
                return;
            }

            _logger.LogInformation("Loading agent card for remote A2A agent {AgentId} from {Endpoint}", agent.Id, endpoint);

            // Fetch agent card from remote service
            var card = await _cardResolver.GetCardAsync(endpoint);

            // Update agent description and instruction based on agent card
            if (!string.IsNullOrWhiteSpace(card.Description))
            {
                agent.Description = card.Description;
                _logger.LogInformation("Updated agent {AgentId} description from agent card: {Description}", 
                    agent.Id, card.Description);
            }

            // Set instruction to forward requests to remote service
            agent.Instruction = "Forward user request to remote A2A service.";

            // Store agent card data in TemplateDict for later use
            agent.TemplateDict["a2a_agent_card"] = card;
            agent.TemplateDict["a2a_agent_name"] = card.Name;
            agent.TemplateDict["a2a_agent_version"] = card.Version;

            _logger.LogInformation("Successfully loaded agent card for {AgentId}: Name={Name}, Version={Version}, Capabilities={Capabilities}",
                agent.Id, card.Name, card.Version, string.Join(", ", card.Capabilities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load agent card for A2A remote agent {AgentId}", agent.Id);
        }
    }
}
