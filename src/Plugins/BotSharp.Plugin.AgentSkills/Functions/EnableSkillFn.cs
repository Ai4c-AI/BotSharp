using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Files;
using BotSharp.Abstraction.Functions;
using EntityFrameworkCore.BootKit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Plugin.AgentSkills.Functions;

public class EnableSkillFn : IFunctionCallback
{
    public string Name => "enable_skill";

    private readonly IServiceProvider _services;

    public EnableSkillFn(IServiceProvider services)
    {
        _services = services;
    }


    public async Task<bool> Execute(RoleDialogModel message)
    {
        var skillName = message.FunctionArgs;
        var conversation_state = _services.GetRequiredService<IConversationService>();
        conversation_state.SetValue("active_skills", skillName!);
        return true;
    }
}
