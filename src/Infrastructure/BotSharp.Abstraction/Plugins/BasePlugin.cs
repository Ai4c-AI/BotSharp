using BotSharp.Abstraction.Files;
using BotSharp.Abstraction.Repositories.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Abstraction.Plugins;

public abstract class BasePlugin : IPlugin, IBotSharpAppPlugin
{
    private List<string> _dependencies = new List<string>();

    public virtual (bool IsSuccess, string Message) AfterEnable()
    {
        return (true, "Enabled successfully");
    }

    public virtual (bool IsSuccess, string Message) BeforeDisable()
    {
        return (true, "Disable successfully");
    }


    public virtual (bool IsSuccess, string Message) Update(string currentVersion, string targetVersion)
    {
        return (true, "Update Success");
    }

    public virtual void AppStart()
    {

    }

    public virtual List<string> AppStartOrderDependPlugins
    {
        get
        {
            return _dependencies;
        }
    }

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration config)
    {

    }

    public virtual int ConfigureServicesOrder => 2;

    public virtual  int ConfigureOrder => 2;

    public virtual void Configure(IApplicationBuilder app)
    {

    }
}
