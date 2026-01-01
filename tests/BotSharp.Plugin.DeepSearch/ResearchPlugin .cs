using BotSharp.Abstraction.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Plugin.DeepSearch;

public class ResearchPlugin : IBotSharpPlugin
{
    public string Id => "9a67e3f6-7d03-9956-7812-f9184f47058c";
    public string Name => "Deep Research Assistant";
    public string Description => "负责规划和执行深度研究任务，协调搜索和写作.";

    public void RegisterDI(IServiceCollection services, IConfiguration config)
    {
         
    }
}
