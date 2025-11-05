using BotSharp.Abstraction.Memory;

namespace BotSharp.Core.Memory
{
    public class AIContextOrchestrator : IAIContextOrchestrator
    {
        private readonly IEnumerable<IAIContextProvider> _providers;
        private readonly ILogger _logger;

        public AIContextOrchestrator(IEnumerable<IAIContextProvider> providers, ILogger<AIContextOrchestrator> logger)
        {
            _providers = providers.OrderBy(p => p.Priority).ToArray();
            _logger = logger;
        }

        public async ValueTask<AIContext> OnInvokingAsync(InvokingContext invokingContext, CancellationToken ct= default)
        {
            var aggregated = new AIContext() {  ContextMessages = invokingContext.Dialogs};
            foreach (var provider in _providers)
            {
                try
                {
                    var ctx = await provider.InvokingAsync(invokingContext, ct);
                    if (ctx == null)
                        continue;

                    if (ctx.ContextMessages?.Count > 0)
                    {
                        aggregated.ContextMessages.AddRange(ctx.ContextMessages);
                    }

                    if (ctx.Metadata?.Count > 0)
                    {
                        foreach (var kv in ctx.Metadata)
                            aggregated.Metadata[kv.Key] = kv.Value;
                    }

                    if(!string.IsNullOrEmpty(ctx.SystemInstruction))
                    {
                        aggregated.SystemInstruction += ctx.SystemInstruction + "\n";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AIContextProvider {Name} InvokingAsync failed.", provider.Name);
                }
            }
            return aggregated;
        }

        public async ValueTask OnInvokedAsync(InvokedContext invokedContext, CancellationToken ct = default)
        {
            foreach (var p in _providers.Reverse())
            {
                try
                {
                    await p.InvokedAsync(invokedContext, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AIContextProvider {Name} InvokedAsync failed.", p.Name);
                }
            }
        }
    }
}
