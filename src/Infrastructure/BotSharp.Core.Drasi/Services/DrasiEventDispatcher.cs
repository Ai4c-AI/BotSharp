using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Conversations;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.Models;
using BotSharp.Abstraction.Repositories.Filters;
using BotSharp.Core.Drasi.Models;
using BotSharp.Core.Drasi.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BotSharp.Core.Drasi.Services;

public interface IDrasiEventDispatcher
{
    Task DispatchAsync(DrasiChangeMessage message);
}

public class DrasiEventDispatcher : IDrasiEventDispatcher
{
    private readonly IConversationService _conversationService;
    private readonly IAgentService _agentService;
    private readonly DrasiPluginSettings _settings;
    private readonly IServiceProvider _services;

    public DrasiEventDispatcher(
        IConversationService conversationService,
        IAgentService agentService,
        IOptions<DrasiPluginSettings> settings,
        IServiceProvider services)
    {
        _conversationService = conversationService;
        _agentService = agentService;
        _settings = settings.Value;
        _services = services;
    }

    public async Task DispatchAsync(DrasiChangeMessage message)
    {
        var queryId = message.Payload.Source.QueryId;

        // 1. 获取目标 Agent ID (从配置或数据库查找)
        var targetAgentId = _settings.QueryAgentMapping.ContainsKey(queryId)
           ? _settings.QueryAgentMapping[queryId]
            : _settings.DefaultBotId;

        if (string.IsNullOrEmpty(targetAgentId))
            return;

        // 2. 查找或创建会话 (Conversation)
        // 在实际业务中，我们通常会将报警推送到一个特定的长期会话，或者创建一个新的会话
        var conversationId = await GetOrCreateConversationAsync(targetAgentId, queryId);

        // 3. 构建消息
        // 这里的关键是将结构化数据转换为 LLM 可读的自然语言描述
        string promptContent = GeneratePromptFromChange(message);

        // 4. 注入消息
        // 必须设置 ConversationId，BotSharp 才能加载历史上下文
        _conversationService.SetConversationId(conversationId, new List<MessageState>());

        var msg = new RoleDialogModel("function", promptContent) // 使用 function 角色通常能更好地触发 agent 反应
        {
            FunctionName = "DrasiMonitor", // 伪装成一个工具调用的返回
            Indication = $"System Alert: {queryId}"
        };

        // 5. 触发 LLM 响应 (reply: true)
        // 这是关键点：我们要让 Agent 不仅收到消息，还要"思考"并"回复"
        // 例如，Agent 可能会回复："检测到库存不足，正在调用 ERP 接口下单..."
        await _conversationService.SendMessage(targetAgentId, msg, replyMessage: null, async _ => { });
    }

    private string GeneratePromptFromChange(DrasiChangeMessage msg)
    {
        var action = msg.Operation switch
        {
            "i" => "New Record Created",
            "u" => "Record Updated",
            "d" => "Record Deleted",
            _ => "Unknown Change"
        };

        var data = msg.Operation == "d" ? msg.Payload.Before : msg.Payload.After;
        var json = JsonSerializer.Serialize(data);

        return $@"

Source: Drasi Continuous Query '{msg.Payload.Source.QueryId}'
Event Type: {action}
Data Payload: {json}

Instruction: Analyze this data change. If it represents a risk or requires action, execute the necessary tools immediately. If it is informational, summarize it.";
    }

    private async Task<string> GetOrCreateConversationAsync(string targetAgentId, string queryId)
    {
        // 简化逻辑：假设每个 Agent 有一个固定的会话用于 Drasi 报警
        var conversationName = $"DrasiAlerts_{queryId}";
        var conversations = await _conversationService.GetConversations(new ConversationFilter
        {
            AgentId = targetAgentId
        });
        var conversation = conversations.Items.FirstOrDefault(c => c.Title == conversationName);
        if (conversation != null)
        {
            return conversation.Id;
        }
        else
        {
            var newConversation = new Conversation
            {
                AgentId = targetAgentId,
                Title = conversationName,
                CreatedTime = DateTime.UtcNow
            };
            var created = await _conversationService.NewConversation(newConversation);
            return created.Id;
        }
    }
}

