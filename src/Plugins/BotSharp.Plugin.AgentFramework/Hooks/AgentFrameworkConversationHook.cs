using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Agents.Enums;
using BotSharp.Abstraction.Conversations;
using BotSharp.Plugin.AgentFramework.Services;
using BotSharp.Plugin.AgentFramework.Settings;

namespace BotSharp.Plugin.AgentFramework.Hooks;

/// <summary>
/// Conversation hook for intercepting and handling A2A remote agent invocations
/// </summary>
public class AgentFrameworkConversationHook : ConversationHookBase
{
    private readonly IA2AAgentService _a2aService;
    private readonly AgentFrameworkSettings _settings;
    private readonly ILogger<AgentFrameworkConversationHook> _logger;

    public AgentFrameworkConversationHook(
        IA2AAgentService a2aService,
        AgentFrameworkSettings settings,
        ILogger<AgentFrameworkConversationHook> logger)
    {
        _a2aService = a2aService;
        _settings = settings;
        _logger = logger;
        Priority = 100; // High priority to intercept early
    }

    /// <summary>
    /// Intercept message received to check if it should be routed to A2A agent
    /// </summary>
    public override async Task OnMessageReceived(RoleDialogModel message)
    {
        if (!_settings.Enabled || Agent == null)
        {
            return;
        }

        // Check if current agent is an A2A remote agent
        if (Agent.Type != AgentType.A2ARemote)
        {
            return;
        }

        _logger.LogInformation("Intercepting message for A2A remote agent {AgentId}", Agent.Id);

        try
        {
            // Get conversation history for context (excluding current message)
            var history = Dialogs?.Where(d => d != message).ToList();

            // Invoke remote A2A agent
            var response = await _a2aService.InvokeAgentAsync(Agent, message.Content, history);

            // Set the response content
            message.Content = response;
            message.Role = AgentRole.Assistant;
            message.StopCompletion = true; // Stop further processing

            _logger.LogInformation("Successfully handled message via A2A remote agent {AgentId}", Agent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke A2A remote agent {AgentId}", Agent.Id);
            message.Content = $"Failed to communicate with remote agent: {ex.Message}";
            message.Role = AgentRole.Assistant;
            message.StopCompletion = true;
        }
    }
}
