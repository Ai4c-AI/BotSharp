using BotSharp.Abstraction.Plugins;
using BotSharp.Abstraction.Plugins.Interfaces;
using BotSharp.AspNetCore.Infrastructures;
using BotSharp.AspNetCore.Interfaces;
using BotSharp.AspNetCore.Middlewares;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.AspNetCore.lmplements;

public class PluginApplicationBuilderManager : PluginApplicationBuilderManager<PluginApplicationBuilder>
{
    public PluginApplicationBuilderManager(IPluginFinder pluginFinder) : base(pluginFinder)
    {
    }
}

public class PluginApplicationBuilderManager<TPluginApplicationBuilder> : IPluginApplicationBuilderManager
       where TPluginApplicationBuilder : PluginApplicationBuilder, new()
{
    private readonly IPluginFinder _pluginFinder;

    public PluginApplicationBuilderManager(IPluginFinder pluginFinder)
    {
        _pluginFinder = pluginFinder;
    }

    public static RequestDelegate RequestDelegateResult { get; set; }


    /// <summary>
    /// When plugin is enabled or disabled: Rebuild
    /// </summary>
    public void ReBuild()
    {
        TPluginApplicationBuilder applicationBuilder = new TPluginApplicationBuilder();
        applicationBuilder.ReachEndAction = PluginStartupMiddleware.ReachedEndAction;

        var plugins = this._pluginFinder.EnablePlugins<IBotSharpAppPlugin>()?.OrderBy(m => m.ConfigureOrder)?.ToList();
        foreach (var item in plugins)
        {
            item.Configure(applicationBuilder);
        }

        RequestDelegateResult = applicationBuilder.Build();
    }


    public RequestDelegate GetBuildResult()
    {
        if (RequestDelegateResult == null)
        {
            ReBuild();
        }

        return RequestDelegateResult;
    }

}
