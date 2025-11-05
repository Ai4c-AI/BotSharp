using System.Threading;

namespace BotSharp.Abstraction.Memory;

/// <summary>
/// AI Context Provider interface for managing context before and after AI model invocation. 
/// </summary>
public interface IAIContextProvider
{
    /// <summary>
    /// Priority for execution order. Lower values execute first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Name of the AI Context Provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Invoked before the AI model is called to provide additional context.
    /// </summary>
    /// <param name="context">The invoking context containing agent and dialog information</param>
    /// <returns>AI context to be injected into the model request</returns>
    ValueTask<AIContext?> InvokingAsync(InvokingContext context, CancellationToken ct=default);

    /// <summary>
    /// Invoked after the AI model has been called to process the response and update memory.
    /// </summary>
    /// <param name="context">The invoked context containing request and response information</param>
    /// <returns>Completion task</returns>
    ValueTask InvokedAsync(InvokedContext context, CancellationToken ct=default);
}
