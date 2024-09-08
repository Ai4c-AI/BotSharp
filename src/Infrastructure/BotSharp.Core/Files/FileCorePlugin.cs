using BotSharp.Abstraction.Repositories.Enums;
using BotSharp.Core.Files.Services;
using Microsoft.Extensions.Configuration;

namespace BotSharp.Core.Files;

public class FileCorePlugin : IBotSharpModule
{
    public string Id => "6a8473c0-04eb-4346-be32-24755ce5973d";

    public string Name => "File Core";

    public string Description => "Provides file storage and analysis.";

}