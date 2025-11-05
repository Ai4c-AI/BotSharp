using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Memory;

public interface IAIContextOrchestrator
{
    ValueTask<AIContext> OnInvokingAsync(InvokingContext invokingContext,CancellationToken ct = default);

    ValueTask OnInvokedAsync(InvokedContext invokedContext, CancellationToken ct = default);
}
