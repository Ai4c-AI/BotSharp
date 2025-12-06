using BotSharp.Abstraction.Agents;
using BotSharp.Plugin.AgentFramework.Models;

namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Service for invoking remote MAF agents via A2A protocol
/// </summary>
public interface IA2AAgentService
{
    /// <summary>
    /// Invokes a remote A2A agent with the given input
    /// </summary>
    /// <param name="agent">The agent configuration with endpoint information</param>
    /// <param name="input">User input/request</param>
    /// <param name="conversationHistory">Optional conversation history for context</param>
    /// <returns>Agent response</returns>
    Task<string> InvokeAgentAsync(Agent agent, string input, List<RoleDialogModel>? conversationHistory = null);
}
