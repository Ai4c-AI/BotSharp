namespace BotSharp.Plugin.AgentFramework.Services;

/// <summary>
/// Factory interface for creating A2A clients for different endpoints
/// </summary>
public interface IA2AClientFactory
{
    /// <summary>
    /// Creates an A2A client for the specified endpoint
    /// </summary>
    /// <param name="baseUrl">Base URL of the MAF service</param>
    /// <returns>Configured A2A client</returns>
    IA2AClient CreateClient(string baseUrl);
}
