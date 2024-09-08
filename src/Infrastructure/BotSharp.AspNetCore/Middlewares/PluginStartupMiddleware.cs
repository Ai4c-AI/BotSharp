using BotSharp.AspNetCore.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BotSharp.AspNetCore.Middlewares;

internal class PluginStartupMiddleware
{
    private readonly RequestDelegate _next;

    public PluginStartupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public static Action ReachedEndAction { get; set; } = () => { _isReachedEnd = true; };

    private static bool _isReachedEnd;

    public async Task InvokeAsync(HttpContext httpContext, IPluginApplicationBuilderManager pluginApplicationBuilderManager)
    {
        //bool isReachedEnd = false;
        _isReachedEnd = false;

        try
        {
            RequestDelegate requestDelegate = pluginApplicationBuilderManager.GetBuildResult();

            await requestDelegate(httpContext);
        }
        catch (Exception ex)
        {

        }

        if (_isReachedEnd)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(httpContext);
        }
        else
        {
            
        }

    }
}
