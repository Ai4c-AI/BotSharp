using Microsoft.Extensions.Configuration;

namespace BotSharp.Plugin.KnowledgeBase.MemVecDb;

public class MemVecDbPlugin : IBotSharpModule
{
    public string Id => "5ae38f52-4fa4-4f7c-8582-0a00d1f9a412";
    public string Name => "Memory Vector DB";
    public string Description => "Store text embedding, search similar text from memory.";
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IVectorDb, MemoryVectorDb>();
    }
}
