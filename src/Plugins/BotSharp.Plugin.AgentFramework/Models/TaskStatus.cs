namespace BotSharp.Plugin.AgentFramework.Models;

/// <summary>
/// Constants for A2A task status values
/// </summary>
public static class A2ATaskStatus
{
    /// <summary>
    /// Task has been queued and is waiting to be processed
    /// </summary>
    public const string Queued = "queued";

    /// <summary>
    /// Task is currently being processed
    /// </summary>
    public const string Running = "running";

    /// <summary>
    /// Task has completed successfully
    /// </summary>
    public const string Completed = "completed";

    /// <summary>
    /// Task has failed
    /// </summary>
    public const string Failed = "failed";
}
