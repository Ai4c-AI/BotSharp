using BotSharp.Abstraction.Functions;
using BotSharp.Abstraction.Plugins.Interfaces;
using System.Reflection;

namespace BotSharp.Core.Plugins.lmplements;

public class PluginFinder : IPluginFinder
{
    /// <summary>
    /// Used to parse the services required by the plugin constructor
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IPluginContextManager PluginContextManager { get; set; }

    public PluginFinder(IPluginContextManager pluginContextManager, IServiceScopeFactory serviceScopeFactory)
    {
        this.PluginContextManager = pluginContextManager;
        _serviceScopeFactory = serviceScopeFactory;
    } 

    public IEnumerable<(TPlugin PluginInstance, string PluginId)> EnablePluginsFull<TPlugin>()
        where TPlugin : IPlugin  
    {
        var pluginConfigModel = PluginConfigModelFactory.Read();
        IList<string> enablePluginIds = pluginConfigModel.EnabledPlugins;
        foreach (var pluginId in enablePluginIds)
        {
            if (this.PluginContextManager.Any(pluginId))
            {
                 var context = this.PluginContextManager.Get(pluginId);
                // Assembly.FullName: HelloWorld, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
                Assembly pluginMainAssembly = context.Assemblies.Where(m => m.FullName.StartsWith($"{pluginId}, Version=")).FirstOrDefault();
                if (pluginMainAssembly == null)
                {
                    continue;
                }

                Type pluginType = pluginMainAssembly.ExportedTypes.Where(m =>
                    (m.BaseType == typeof(TPlugin) || m.GetInterfaces().Contains(typeof(TPlugin)))
                    &&
                    !m.IsInterface
                    &&
                    !m.IsAbstract
                ).FirstOrDefault();

                if (pluginType == null)
                {
                    continue;
                }
                object instance = ResolveUnregistered(pluginType);
                //try to get typed instance
                TPlugin typedInstance = (TPlugin)instance;
                if (typedInstance == null)
                {
                    continue;
                }

                yield return (PluginInstance: typedInstance, PluginId: pluginId);
            }
        }

    }

    public IEnumerable<TPlugin> EnablePlugins<TPlugin>()
        where TPlugin : IPlugin  
    {
        return EnablePluginsFull<TPlugin>().Select(m => m.PluginInstance);
    }

    public IEnumerable<IPlugin> EnablePlugins()
    {
        return EnablePluginsFull<IPlugin>().Select(m => m.PluginInstance);
    }

    public IEnumerable<(IPlugin PluginInstance, string PluginId)> EnablePluginsFull()
    {
        return EnablePluginsFull<IPlugin>();
    }

    public IEnumerable<string> EnablePluginIds<TPlugin>()
        where TPlugin : IPlugin 
    {
       
        var pluginConfigModel = PluginConfigModelFactory.Read();
        IList<string> enablePluginIds = pluginConfigModel.EnabledPlugins;
        foreach (var pluginId in enablePluginIds)
        {
            if (this.PluginContextManager.Any(pluginId))
            {
                yield return pluginId;
            }
        }
    }

    public IEnumerable<string> EnablePluginIds()
    {
        return EnablePluginIds<IPlugin>();
    }
   
    public IPlugin Plugin(string pluginId)
    {
        var pluginConfigModel = PluginConfigModelFactory.Read();
        IList<string> enablePluginIds = pluginConfigModel.EnabledPlugins;

        if (!enablePluginIds.Contains(pluginId))
        {
            return null;
        }

        if (!this.PluginContextManager.Any(pluginId))
        {
            return null;
        }

        var context = this.PluginContextManager.Get(pluginId);
        // Assembly.FullName: HelloWorld, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
        Assembly pluginMainAssembly = context.Assemblies.Where(m => m.FullName.StartsWith($"{pluginId}, Version=")).FirstOrDefault();
        if (pluginMainAssembly == null)
        {
            return null;
        }
        Type pluginType = pluginMainAssembly.ExportedTypes.Where(m =>
            (m.BaseType == typeof(IPlugin) || m.GetInterfaces().Contains(typeof(IPlugin)))
            &&
            !m.IsInterface
            &&
            !m.IsAbstract
        ).FirstOrDefault();
        if (pluginType == null)
        {
            return null;
        }
        object instance = ResolveUnregistered(pluginType);
        IPlugin typedInstance = (IPlugin)instance;
     
        if (typedInstance == null)
        {
            return null;
        }

        return typedInstance;
    }
   
    protected virtual object ResolveUnregistered(Type type)
    {

        Exception innerException = null;
        foreach (var constructor in type.GetConstructors())
        {
            try
            {
                //try to resolve constructor parameters
                var parameters = constructor.GetParameters().Select(parameter =>
                {
                    //var service = Resolve(parameter.ParameterType);
                    // var service = _repository.GetService(parameter.ParameterType);
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetService(parameter.ParameterType);
                        if (service == null)
                            throw new Exception("Unknown dependency");
                        return service;
                    }
                });

                //all is ok, so create instance
                return Activator.CreateInstance(type, parameters.ToArray());
            }
            catch (Exception ex)
            {
                innerException = ex;
            }
        }
        throw new Exception("No constructor was found that had all the dependencies satisfied.", innerException);
    }
}