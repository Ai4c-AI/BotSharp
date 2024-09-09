using BotSharp.Abstraction.Evaluations;
using BotSharp.Abstraction.Evaluations.Settings;
using BotSharp.Abstraction.Functions;
using BotSharp.Abstraction.Google.Settings;
using BotSharp.Abstraction.Instructs;
using BotSharp.Abstraction.Messaging;
using BotSharp.Abstraction.Messaging.JsonConverters;
using BotSharp.Abstraction.MLTasks;
using BotSharp.Abstraction.MLTasks.Settings;
using BotSharp.Abstraction.Options;
using BotSharp.Abstraction.Repositories.Enums;
using BotSharp.Abstraction.Routing.Planning;
using BotSharp.Abstraction.Routing.Settings;
using BotSharp.Abstraction.Settings;
using BotSharp.Abstraction.Statistics.Settings;
using BotSharp.Abstraction.Tasks;
using BotSharp.Abstraction.Templating;
using BotSharp.Abstraction.Users.Settings;
using BotSharp.Core.Evaluatings;
using BotSharp.Core.Evaluations;
using BotSharp.Core.Files.Services;
using BotSharp.Core.Instructs;
using BotSharp.Core.Messaging;
using BotSharp.Core.Routing.Functions;
using BotSharp.Core.Routing.Handlers;
using BotSharp.Core.Routing.Hooks;
using BotSharp.Core.Routing.Planning;
using BotSharp.Core.Tasks.Services;
using BotSharp.Core.Templating;
using BotSharp.Core.Translation;
using BotSharp.Logger.Hooks;
using Microsoft.Extensions.Configuration;

namespace BotSharp.Core;

public static class BotSharpCoreExtensions
{
    public static IServiceCollection AddBotSharpCore(this IServiceCollection services, IConfiguration config, Action<BotSharpOptions>? configOptions = null)
    {
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<DistributedLocker>();

        ConfigureBotSharpOptions(services, configOptions);
        services.AddScoped<ILlmProviderService, LlmProviderService>();
        services.AddScoped<IAgentService, AgentService>();

        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<AgentSettings>("Agent");
        });

        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<ConversationSetting>("Conversation");
        });

        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<GoogleApiSettings>("GoogleApi");
        });

        services.AddScoped<IConversationStorage, ConversationStorage>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IConversationStateService, ConversationStateService>();
        services.AddScoped<ITranslationService, TranslationService>();

        // Evaluation
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<EvaluatorSetting>("Evaluator");
        });

        services.AddScoped<IConversationHook, EvaluationConversationHook>();
        services.AddScoped<IEvaluatingService, EvaluatingService>();
        services.AddScoped<IExecutionLogger, ExecutionLogger>();

        // Rich content messaging
        services.AddScoped<IRichContentService, RichContentService>();

        // Register template render
        services.AddSingleton<ITemplateRender, TemplateRender>();
        services.AddScoped<IResponseTemplateService, ResponseTemplateService>();

        services.AddScoped<IExecutor, InstructExecutor>();
        services.AddScoped<IInstructService, InstructService>();
        services.AddScoped<ITokenStatistics, TokenStatistics>();
        var fileCoreSettings = new FileCoreSettings();
        config.Bind("FileCore", fileCoreSettings);
        services.AddSingleton(fileCoreSettings);

        if (fileCoreSettings.Storage == FileStorageEnum.LocalFileStorage)
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }
        services.AddScoped<IFileInstructService, FileInstructService>();
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            var loger = provider.GetRequiredService<ILogger<LlmProviderPlugin>>();
            var llmProviders = settingService.Bind<List<LlmProviderSetting>>("LlmProviders");
            foreach (var llmProvider in llmProviders)
            {
                loger.LogInformation($"Loaded LlmProvider {llmProvider.Provider} settings with {llmProvider.Models.Count} models.");
            }
            return llmProviders;
        });

        var myDatabaseSettings = new BotSharpDatabaseSettings();
        config.Bind("Database", myDatabaseSettings);

        // In order to use EntityFramework.BootKit in other plugin
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<DatabaseSettings>("Database");
        });

        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<DatabaseBasicSettings>("Database");
        });

        services.AddSingleton(provider => myDatabaseSettings);

        if (myDatabaseSettings.Default == RepositoryEnum.FileRepository)
        {
            services.AddScoped<IBotSharpRepository, FileRepository>();
        }

        services.AddScoped<IRoutingContext, RoutingContext>();

        // Register router
        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<RoutingSettings>("Router");
        });

        services.AddScoped<IRoutingService, RoutingService>();
        services.AddScoped<IAgentHook, RoutingAgentHook>();

        services.AddScoped<IRoutingPlaner, NaivePlanner>();
        services.AddScoped<IRoutingPlaner, HFPlanner>();
        services.AddScoped<IRoutingPlaner, SequentialPlanner>();
        services.AddScoped<IRoutingPlaner, TwoStagePlanner>();
        services.AddScoped<IAgentTaskService, AgentTaskService>();
        services.AddScoped<IConversationHook, TranslationResponseHook>();
 
        services.AddScoped<IRoutingHandler, RouteToAgentRoutingHandler>();
        services.AddScoped<IFunctionCallback, FallbackToRouterFn>();
        services.AddScoped<IFunctionCallback, HumanInterventionNeededFn>();
        services.AddScoped<IFunctionCallback, RouteToAgentFn>();

        var accountSettings = new AccountSetting();
        config.Bind("Account", accountSettings);
        services.AddScoped(x => accountSettings);
        var options = new BotSharpOptions();

        services.AddScoped(provider =>
        {
            var settingService = provider.GetRequiredService<ISettingService>();
            return settingService.Bind<StatisticsSettings>("Statistics");
        });
        AddDefaultJsonConverters(options);
        services.AddSingleton(options);
        return services;
    }

    private static void ConfigureBotSharpOptions(IServiceCollection services, Action<BotSharpOptions>? configure)
    {
        var options = new BotSharpOptions();
        if (configure != null)
        {
            configure(options);
        }

        AddDefaultJsonConverters(options);
        services.AddSingleton(options);
    }

    private static void AddDefaultJsonConverters(BotSharpOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new RichContentJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TemplateMessageJsonConverter());
    }   
}
