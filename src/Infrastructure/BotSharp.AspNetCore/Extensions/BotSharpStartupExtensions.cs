using BotSharp.Abstraction.Plugins;
using BotSharp.Abstraction.Plugins.Interfaces;
using BotSharp.Abstraction.Utilities;
using BotSharp.AspNetCore.Interfaces;
using BotSharp.AspNetCore.lmplements;
using BotSharp.AspNetCore.Middlewares;
using BotSharp.Core;
using BotSharp.Core.Plugins;
using BotSharp.Core.Plugins.lmplements;
using BotSharp.Core.Plugins.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace BotSharp.AspNetCore.Extensions;

public static class BotSharpStartupExtensions
{
    private static IWebHostEnvironment _webHostEnvironment;

    private static IServiceCollection _services;

    private static IServiceProvider _serviceProvider;

    /// <summary>
    /// If you need to replace the default implementation, please put it before <see cref="AddBotSharpCore(IServiceCollection)"/>. If you put it after, it will not affect the default behavior when the main program starts.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddBotSharpAspNetCore(this IServiceCollection services, IConfiguration config)
    {
        services.AddBotSharpCore(config);
        services.AddBotSharpCoreServices();

        services.AddBotSharpCoreLog();

        services.AddBotSharpCorePlugins();

        services.AddBotSharpCoreStartupPlugin(config);

        _services = services;
        _serviceProvider = services.BuildServiceProvider();
        return _services;
    }

    public static IApplicationBuilder UseBotSharpCore(this IApplicationBuilder app)
    { 
        app.UseBotSharpCoreStaticFiles();

        // If the UseAuthentication authentication middleware is added repeatedly, authentication will only be performed once.
        // But if UseAuthorization is added twice, authorization will be performed twice
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseBotSharpCoreStartupPlugin();

        app.UseBotSharpCoreAppStart();

        return app;
    }



    public static void AddBotSharpCoreServices(this IServiceCollection services)
    {
        #region Applies only to ASP.NET Core
        // Used to notify Controller.Action changes when adding a plugin Controller
        services.AddSingleton<IActionDescriptorChangeProvider>(PluginActionDescriptorChangeProvider.Instance);
        services.AddSingleton(PluginActionDescriptorChangeProvider.Instance);

        services.TryAddTransient<PluginControllerManager>();
        services.TryAddTransient<IPluginControllerManager, PluginControllerManager>();

        services.TryAddTransient<PluginApplicationBuilderManager>();
        services.TryAddTransient<IPluginApplicationBuilderManager, PluginApplicationBuilderManager>();
        #endregion

        services.AddSingleton<IPluginModuleManager, PluginModuleManager>(x => new PluginModuleManager(services));
        services.TryAddTransient<PluginContextPack>();
        services.TryAddTransient<IPluginContextPack, PluginContextPack>();

        services.TryAddTransient<AspNetCorePluginManager>();
        services.TryAddTransient<IPluginManager, AspNetCorePluginManager>();

        services.TryAddTransient<PluginFinder>();
        services.TryAddTransient<IPluginFinder, PluginFinder>();

        services.AddSingleton<PluginContextManager>();
        services.AddSingleton<IPluginContextManager>(c => c.GetRequiredService<PluginContextManager>());

        _serviceProvider = services.BuildServiceProvider();
    }

    public static void AddBotSharpCorePlugins(this IServiceCollection services)
    {
        #region ASP.NET Core
        IPluginManager pluginManager = _serviceProvider.GetRequiredService<IPluginManager>();

        // Initialize PluginCore related directories
        PluginPathProvider.PluginsRootPath();

        // Load all installed and enabled plugins at program startup
        PluginConfigModel pluginConfigModel = PluginConfigModelFactory.Read();

        IList<string> enabledPluginIds = pluginConfigModel.EnabledPlugins;
        foreach (var pluginId in enabledPluginIds)
        {
            pluginManager.LoadPlugin(pluginId);
        }

        // Add the Controller in this Assembly
        var ass = Assembly.GetExecutingAssembly();
        //IPluginControllerManager pluginControllerManager = services.BuildServiceProvider().GetService<IPluginControllerManager>();
        IPluginControllerManager pluginControllerManager = _serviceProvider.GetService<IPluginControllerManager>();
        pluginControllerManager.AddControllers(ass);

        // IWebHostEnvironment
        _webHostEnvironment = services.BuildServiceProvider().GetService<IWebHostEnvironment>();
        #endregion
    }

    public static void AddPluginCoreAuthentication(this IServiceCollection services)
    {
        // fixed: https://github.com/yiyungent/PluginCore/issues/4
        // System.InvalidOperationException: No authenticationScheme was specified, and there was no DefaultChallengeScheme found. The default schemes can be set using either AddAuthentication(string defaultScheme) or AddAuthentication(Action<AuthenticationOptions> configureOptions).
        #region 添加认证 Authentication
        // 没通过 Authentication: 401 Unauthorized
        // services.AddAuthentication("PluginCore.Authentication")
        //     .AddScheme<Authentication.PluginCoreAuthenticationSchemeOptions,
        //         Authentication.PluginCoreAuthenticationHandler>("PluginCore.Authentication", "PluginCore.Authentication",
        //         options => { });
        // 注意: 不要设置 默认 认证名: Constants.AspNetCoreAuthenticationScheme
        // services.AddAuthentication(Constants.AspNetCoreAuthenticationScheme)
        // 默认认证名: 默认
        //services.AddAuthentication()
        //    // 添加一个新的认证方案
        //    .AddScheme<Authentication.PluginCoreAuthenticationSchemeOptions, Authentication.PluginCoreAuthenticationHandler>(
        //        authenticationScheme: Constants.AspNetCoreAuthenticationScheme,
        //        displayName: Constants.AspNetCoreAuthenticationScheme,
        //        options => { });
        #endregion
    }

    public static void AddPluginCoreAuthorization(this IServiceCollection services)
    {
        //#region 添加授权策略-所有标记 [PluginCoreAdminAuthorize] 都需要权限检查
        //// Only Once, Not recommend
        ////services.AddSingleton<IAuthorizationHandler, PluginCoreAdminAuthorizationHandler>();

        //services.AddAuthorization(options =>
        //{
        //    // options.AddPolicy("PluginCore.Admin", policy =>
        //    options.AddPolicy(name: Constants.AspNetCoreAuthorizationPolicyName, policy =>
        //    {
        //        // 无法满足 下方任何一项：HTTP 403 错误
        //        // 3.需要 检查是否拥有当前请求资源的权限
        //        //policy.Requirements.Add(new PluginCoreAdminRequirement());
        //        policy.AuthenticationSchemes = new string[] {
        //                Constants.AspNetCoreAuthenticationScheme
        //            };
        //        policy.RequireAuthenticatedUser();
        //        policy.RequireClaim(claimType: Constants.AspNetCoreAuthenticationClaimType);
        //        // 必须重启才能使得更改密码生效
        //        string token = AccountManager.CreateToken();
        //        policy.RequireClaim(claimType: Constants.AspNetCoreAuthenticationClaimType, allowedValues: new string[] {
        //                token
        //            });
        //        //policy.RequireAssertion(context =>
        //        //{
        //        //    return true;
        //        //});
        //    });
        //});
        //#endregion

        //// AccountManager
        //services.AddTransient<AccountManager>();
        //// HttpContext.Current
        //services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        ////services.AddHttpContextAccessor(); 
    }

    public static void AddBotSharpCoreStartupPlugin(this IServiceCollection services, IConfiguration config)
    {
        IPluginFinder pluginFinder = _serviceProvider.GetService<IPluginFinder>();

        #region IStartupPlugin

        var plugins = pluginFinder.EnablePlugins<IBotSharpAppPlugin>()?.OrderBy(m => m.ConfigureServicesOrder)?.ToList();

        foreach (var item in plugins)
        {
            item?.ConfigureServices(services, config);
        }

        #endregion
    }

    public static void AddBotSharpCoreLog(this IServiceCollection services)
    {
        #region Logger

        IServiceScopeFactory serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

        #endregion
    }

    public static IApplicationBuilder UseBotSharpCoreStaticFiles(this IApplicationBuilder app)
    {
        // TODO: 其实由于目前已实现运行时动态新增/删除 HTTP Middleware, 其实可以不用再像下方这么复制插件 wwwroot 目录到 Plugins_wwwroot/{PluginId}
        // 而是在运行时配置, 直接指向 `Plugins/{PluginId}/wwwroot`, 而无需复制/删除

        // 注意：`UseDefaultFiles`必须在`UseStaticFiles`之前进行调用。因为`DefaultFilesMiddleware`仅仅负责重写Url，实际上默认页文件，仍然是通过`StaticFilesMiddleware`来提供的。

        string pluginwwwrootDir = PluginPathProvider.PluginsWwwRootDir();

        #region 插件 wwwroot 默认页
        // 设置目录的默认页
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Clear();
        // 指定默认页名称
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        // 指定请求路径
        defaultFilesOptions.RequestPath = "/Plugins";
        // 指定默认页所在的目录
        defaultFilesOptions.FileProvider = new PhysicalFileProvider(pluginwwwrootDir);
        app.UseDefaultFiles(defaultFilesOptions);
        #endregion

        #region 插件 wwwroot
        // 由于没办法在运行时, 动态 UseStaticFiles(), 因此不再为每一个插件都 UseStaticFiles(),
        // 而是统一在一个文件夹下, 插件启用时, 将插件的wwwroot复制到 Plugins_wwwroot/{PluginId}, 禁用时, 再删除
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                pluginwwwrootDir),
            RequestPath = "/Plugins"
        });
        #endregion

        return app;
    }

    public static IApplicationBuilder UseBotSharpCoreAppStart(this IApplicationBuilder app)
    {
        IPluginFinder pluginFinder = _serviceProvider.GetRequiredService<IPluginFinder>();

        #region AppStart

        var plugins = pluginFinder.EnablePluginsFull()?.ToList();
        var dependencySorter = new DependencySorter<IPlugin>();
        dependencySorter.AddObjects(plugins.Select(m => m.PluginInstance).ToArray());
        foreach (var item in plugins)
        {
            var dependPlugins = plugins.Where(m => item.PluginInstance.AppStartOrderDependPlugins.Contains(m.PluginId)).Select(m => m.PluginInstance).ToArray();
            dependencySorter.SetDependencies(obj: item.PluginInstance, dependsOnObjects: dependPlugins);
        }
        var sortedPlugins = dependencySorter.Sort();
        foreach (var item in sortedPlugins)
        {
            item?.AppStart();
        }

        #endregion

        return app;
    }

    public static IApplicationBuilder UseBotSharpCoreStartupPlugin(this IApplicationBuilder app)
    {
        IPluginFinder pluginFinder = _serviceProvider.GetRequiredService<IPluginFinder>();

        #region IStartupPlugin

        var startupPlugins = pluginFinder.EnablePlugins<IBotSharpAppPlugin>()?.OrderBy(m => m.ConfigureOrder)?.ToList();

        foreach (var item in startupPlugins)
        {
            item?.Configure(app);
        }
        #endregion

        app.UseMiddleware<PluginStartupMiddleware>();

        return app;
    }
}