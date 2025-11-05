using System.Threading;

namespace BotSharp.Abstraction.Memory;

/// <summary>
/// Base implementation of IAIContextProvider with default behavior.
/// </summary>
public abstract class AIContextProviderBase : IAIContextProvider
{
    public virtual string Name => GetType().Name;

    /// <summary>
    /// Priority for execution order. Lower values execute first.
    /// Default is 0 (medium priority).
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// Invoked before the AI model is called to provide additional context.
    /// Override this method to provide custom context.
    /// </summary>
    public virtual ValueTask<AIContext?> InvokingAsync(InvokingContext context, CancellationToken ct)
    {
        return default;
    }

    /// <summary>
    /// Invoked after the AI model has been called to process the response and update memory.
    /// Override this method to save memory or process the response.
    /// </summary>
    public virtual ValueTask InvokedAsync(InvokedContext context, CancellationToken ct)
    {
        return default;
    }
}
