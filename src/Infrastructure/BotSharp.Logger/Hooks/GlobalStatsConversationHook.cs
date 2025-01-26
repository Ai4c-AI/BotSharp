using BotSharp.Abstraction.Statistics.Enums;
using BotSharp.Abstraction.Statistics.Models;
using BotSharp.Abstraction.Statistics.Services;

namespace BotSharp.Logger.Hooks;

public class GlobalStatsConversationHook : ConversationHookBase
{
    private readonly IServiceProvider _services;

    public GlobalStatsConversationHook(
        IServiceProvider services)
    {
        _services = services;
    }

    public override async Task OnMessageReceived(RoleDialogModel message)
    {
        UpdateAgentCall(message);
    }

    public override async Task OnPostbackMessageReceived(RoleDialogModel message, PostbackMessageModel replyMsg)
    {
        UpdateAgentCall(message);
    }

    private void UpdateAgentCall(RoleDialogModel message)
    {
        // record agent call
        var globalStats = _services.GetRequiredService<IBotSharpStatService>();

        var body = new BotSharpStats
        {
            Category = StatCategory.AgentCall,
            Group = $"Agent: {message.CurrentAgentId}",
            Data = new Dictionary<string, object>
            {
                { "agent_id", message.CurrentAgentId },
                { "agent_call_count", 1 }
            },
            RecordTime = DateTime.UtcNow
        };
        globalStats.UpdateAgentCall(body);
    }
}
