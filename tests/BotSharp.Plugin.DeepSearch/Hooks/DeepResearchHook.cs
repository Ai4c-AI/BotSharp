using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Agents.Models;
using BotSharp.Abstraction.Agents.Settings;
using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Plugin.DeepSearch.Hooks;

public class DeepResearchHook : AgentHookBase
{
    private readonly IServiceProvider _services;

    public DeepResearchHook(IServiceProvider services, AgentSettings settings)
        : base(services, settings)
    {
        _services = services;
    }

    // 在 Agent 执行完毕后触发
    public override void OnAgentLoaded(Agent agent)
    {
 
    }

  
}