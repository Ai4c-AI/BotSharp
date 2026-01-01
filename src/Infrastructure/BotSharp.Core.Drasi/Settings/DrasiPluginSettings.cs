using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Core.Drasi.Settings;

public class DrasiPluginSettings
{
    public Dictionary<string, string> QueryAgentMapping { get; set; } = new();
    public string DefaultBotId { get; set; } = string.Empty;
    public string SignalREndpoint { get; set; } = string.Empty;
}
