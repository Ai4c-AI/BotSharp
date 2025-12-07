using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Functions.Models;

namespace BotSharp.Plugin.AgentProtocol.Hooks;

/// <summary>
/// Hook for A2A agent integration
/// </summary>
public class A2AAgentHook : AgentHookBase
{
    private readonly IA2AClient _a2aClient;
    private readonly IAgentRegistry _registry;
    private readonly AgentProtocolSettings _a2aSettings;

    public override string SelfId => string.Empty;

    public A2AAgentHook(
        IServiceProvider services,
        AgentSettings agentSettings,
        IA2AClient a2aClient,
        IAgentRegistry registry,
        AgentProtocolSettings settings)
        : base(services, agentSettings)
    {
        _a2aClient = a2aClient;
        _registry = registry;
        _a2aSettings = settings;
    }

    public override bool OnInstructionLoaded(string template, IDictionary<string, object> dict)
    {
        // Add A2A agents to routing agents if this is a routing agent
        if (_agent.Type == AgentType.Routing && _a2aSettings.EnableA2AClient)
        {
            var routing = _services.GetRequiredService<IRoutingService>();
            var agents = routing.GetRoutableAgents(_agent.Profiles);

            // Get A2A agents from registry
            // Note: OnInstructionLoaded is synchronous by design in AgentHookBase
            // Using Task.Run to prevent blocking the synchronization context
            var a2aAgents = Task.Run(async () => await _registry.GetAllAgentsAsync()).GetAwaiter().GetResult();
            
            if (a2aAgents.Any())
            {
                // Convert A2A agent cards to routable agents
                var routableA2AAgents = a2aAgents.Select(card => new RoutableAgent
                {
                    AgentId = card.Id,
                    Name = card.Name,
                    Description = card.Description,
                    Type = AgentType.Task,
                    RequiredFields = card.RequiredParameters.Select(p => new ParameterPropertyDef(p.Name, p.Description, type: p.Type)
                    {
                        Required = p.Required
                    }).ToList(),
                    OptionalFields = card.OptionalParameters.Select(p => new ParameterPropertyDef(p.Name, p.Description, type: p.Type)
                    {
                        Required = p.Required
                    }).ToList()
                }).ToArray();

                // Merge with existing agents
                var allAgents = agents.Concat(routableA2AAgents).ToArray();
                dict["routing_agents"] = allAgents;
            }
        }

        return base.OnInstructionLoaded(template, dict);
    }
}
