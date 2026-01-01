using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Messaging;
using BotSharp.Abstraction.Messaging.Models.RichContent;
using BotSharp.Abstraction.Messaging.Models.RichContent.Template;
using BotSharp.Abstraction.Routing.Models;
using BotSharp.Plugin.PythonInterpreter.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Plugin.AgentSkills.Hooks;

public  class AgentSkillConversationHook : ConversationHookBase
{
    
    private readonly IServiceProvider _services;
    private readonly SkillLoaderService _loader;
    private readonly IConversationStateService _state;
    private readonly PythonScriptExecutor _pythonExecutor;

    public AgentSkillConversationHook(IServiceProvider services,
        IConversationStateService states)
    {
        _services = services;
        _state = services.GetRequiredService<IConversationStateService>();
        _pythonExecutor = services.GetRequiredService<PythonScriptExecutor>();
    }

 
    public override async Task OnFunctionExecuting(RoleDialogModel message, InvokeFunctionOptions? options = null)
    {
        var activeSkills = _state.GetState("active_skills")?.Split(',') ?? new string[0];
        foreach (var skillName in activeSkills)
        {
            var skill = _loader.GetSkill(skillName);
            var script = skill.ScriptFiles.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == message.FunctionName);
            if (!string.IsNullOrEmpty(script))
            {
                var result = await _pythonExecutor.ExecuteScript(script, message.FunctionArgs);
                message.Content = result;
                message.RichContent = new RichContent<IRichMessage>
                {
                    Recipient = new Recipient { Id = message.MessageId },
                    Message = new ProgramCodeTemplateMessage
                    {
                        Text = result,
                        Language = "python"
                    }
                };
                message.StopCompletion = true;
                return;
            }
        }         
    }
}
