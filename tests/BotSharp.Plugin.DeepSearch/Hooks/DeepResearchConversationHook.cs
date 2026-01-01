using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Routing.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotSharp.Plugin.DeepSearch.Hooks;

public  class DeepResearchConversationHook : ConversationHookBase
{
    private readonly IServiceProvider _services;
    private readonly IConversationStateService _states;

    public DeepResearchConversationHook(IServiceProvider services,
        IConversationStateService states)
    {
        _services = services;
        _states = states;
    }

    // 拦截函数调用结果
    public override async Task  OnFunctionExecuted(RoleDialogModel message, InvokeFunctionOptions? options = null)
    {
        if (message.FunctionName == "WebSearch")
        {
            // 搜索完成后，自动增加计数器
            var state = _services.GetRequiredService<IConversationStateService>();
            var count = state.GetState("search_count", "0");
            int currentCount = int.Parse(count) + 1;
            state.SetState("search_count", currentCount.ToString());

            // 强制退出机制：如果搜索超过 5 次，强制进入报告生成阶段
            if (currentCount >= 5)
            {
                message.Content += "\n[系统提示]：搜索次数已达上限，请根据现有信息生成报告。";
            }
        }
        await base.OnFunctionExecuted(message);
    }
}
