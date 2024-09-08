using BotSharp.Abstraction.Repositories.Enums;
using BotSharp.Abstraction.Settings;
using Microsoft.Extensions.Configuration;

namespace BotSharp.Core.Repository;

public class RepositoryPlugin : IBotSharpModule
{
    public string Id => "866b4b19-b4d3-479d-8e0a-98816643b8db";
    public string Name => "Data Repository";
    public string Description => "Provides a data persistence abstraction layer to store Agent and conversation-related data.";
 
}
