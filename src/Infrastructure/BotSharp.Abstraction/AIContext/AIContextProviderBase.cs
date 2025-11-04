namespace BotSharp.Abstraction.AIContext;

/// <summary>
/// Base implementation of IAIContextProvider with default behavior.
/// </summary>
public abstract class AIContextProviderBase : IAIContextProvider
{
    /// <summary>
    /// Priority for execution order. Lower values execute first.
    /// Default is 0 (medium priority).
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// Invoked before the AI model is called to provide additional context.
    /// Override this method to provide custom context.
    /// </summary>
    public virtual Task<AIContext?> InvokingAsync(InvokingContext context)
    {
        return Task.FromResult<AIContext?>(null);
    }

    /// <summary>
    /// Invoked after the AI model has been called to process the response and update memory.
    /// Override this method to save memory or process the response.
    /// </summary>
    public virtual Task InvokedAsync(InvokedContext context)
    {
        return Task.CompletedTask;
    }
}
