using BotSharp.Abstraction.Evaluations.Settings;
using BotSharp.Abstraction.Evaluations;
using BotSharp.Abstraction.Settings;
using BotSharp.Core.Evaluatings;
using Microsoft.Extensions.Configuration;

namespace BotSharp.Core.Evaluations;

public class EvaluationPlugin : IBotSharpModule
{
    public string Id => "0ae251a6-7e34-403e-a333-b365d8986068";
    public string Name => "Agent Evaluation";
    public string Description => "Evaluate the created Agent and review whether the user dialogue meets design expectations.";

}
