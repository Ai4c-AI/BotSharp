using BotSharp.Abstraction.Functions;
using BotSharp.Abstraction.Plugins.Interfaces;
using BotSharp.Abstraction.Plugins.Models;
using System.Reflection;

namespace BotSharp.Core.Plugins.lmplements;

public class PluginModuleManager : IPluginModuleManager
{
    private static readonly object _lockObject = new Object();
    /// <summary>
    /// Used to parse the services required by the plugin constructor
    /// </summary>
    private readonly IServiceCollection _services;

    private List<IBotSharpModule> _modules
    {
        get
        {
            return PluginModuleStore.Modules;
        }
    }
    private List<PluginDef> _plugins
    {
        get
        {
            return PluginModuleStore.Plugins;
        }
    }


    public PluginModuleManager(IServiceCollection services)
    {
        _services = services;

        InitBotSharpCore();
    }

    private void InitBotSharpCore()
    {
        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
            .Where(m => m.FullName.StartsWith("BotSharp.Core, Version=")).FirstOrDefault();
        AddPluginModules(assembly);
    }

    public void AddPluginModules(Assembly assembly)
    {
        try
        {
            var modules = assembly.GetTypes()
                       .Where(x => x.IsClass)
                       .Where(x => typeof(IBotSharpModule).IsAssignableFrom(x) && x != typeof(IBotSharpModule))
                       .ToArray();

            foreach (var module in modules)
            {
                var botsharpmodule = Activator.CreateInstance(module) as IBotSharpModule;

                if (_plugins.Exists(x => x.Id == botsharpmodule.Id))
                {
                    continue;
                }
                InitModule(assembly.FullName, botsharpmodule);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        //// Register routing handlers
        //var handlers = assembly.GetTypes()
        //    .Where(x => x.IsClass)
        //    .Where(x => x.GetInterface(nameof(IRoutingHandler)) != null)
        //    .ToArray();

        //foreach (var handler in handlers)
        //{
        //    _services.AddScoped(typeof(IRoutingHandler), handler);             
        //}

        //// Register function callback
        //var functions = assembly.GetTypes()
        //    .Where(x => x.IsClass)
        //    .Where(x => x.GetInterface(nameof(IFunctionCallback)) != null)
        //    .ToArray();

        //foreach (var function in functions)
        //{
        //    _services.AddScoped(typeof(IFunctionCallback), function);
        //}
    }

    private void InitModule(string assembly, IBotSharpModule module)
    {
        var name = string.IsNullOrEmpty(module.Name) ? module.GetType().Name : module.Name;
        if(!_modules.Exists(x => x.Id == module.Id))
        {
            lock (_lockObject)
            {
                _modules.Add(module);
            }
             
        }
       if(!_plugins.Exists(x => x.Id == module.Id))
        {
            lock (_lockObject)
            {
                _plugins.Add(new PluginDef
                {
                    Id = module.Id,
                    Name = name,
                    Module = module,
                    Description = module.Description,
                    Assembly = assembly,
                    IconUrl = module.IconUrl,
                    AgentIds = module.AgentIds
                });
            }
        }
        
        Console.Write($"Loaded plugin ");
        Console.Write(name);
        Console.WriteLine($" from {assembly}.");
        if (!string.IsNullOrEmpty(module.Description))
        {
            Console.WriteLine(module.Description);
        }
    }

    public void RemovePluginModules(string pluginId)
    {
         
    }

    public List<PluginDef> GetPlugins(IServiceProvider services)
    {
        var db = services.GetRequiredService<IBotSharpRepository>();
        var config = db.GetPluginConfig();
        foreach (var plugin in _plugins)
        {
            plugin.Enabled = plugin.IsCore || config.EnabledPlugins.Contains(plugin.Id);
        }
        return _plugins;
    }

    public PagedItems<PluginDef> GetPagedPlugins(IServiceProvider services, PluginFilter filter)
    {
        var plugins = GetPlugins(services);
        var pager = filter?.Pager ?? new Pagination();

        return new PagedItems<PluginDef>
        {
            Items = plugins.Skip(pager.Offset).Take(pager.Size),
            Count = plugins.Count()
        };
    }

    public PluginDef UpdatePluginStatus(IServiceProvider services, string id, bool enable)
    {
        var plugin = _plugins.First(x => x.Id == id);
        plugin.Enabled = enable;

        var db = services.GetRequiredService<IBotSharpRepository>();
        var config = db.GetPluginConfig();
        if (enable)
        {
            var dependentPlugins = new HashSet<string>();
            var dependentAgentIds = new HashSet<string>();
            FindPluginDependency(id, enable, ref dependentPlugins, ref dependentAgentIds);
            var missingPlugins = dependentPlugins.Where(x => !config.EnabledPlugins.Contains(x)).ToList();
            if (!missingPlugins.IsNullOrEmpty())
            {
                config.EnabledPlugins.AddRange(missingPlugins);
                db.SavePluginConfig(config);
            }

            // enable agents
            var agentService = services.GetRequiredService<IAgentService>();
            foreach (var agentId in dependentAgentIds)
            {
                var agent = agentService.LoadAgent(agentId).Result;
                agent.Disabled = false;
                agentService.UpdateAgent(agent, AgentField.Disabled);

                if (agent.InheritAgentId != null)
                {
                    agent = agentService.LoadAgent(agent.InheritAgentId).Result;
                    agent.Disabled = false;
                    agentService.UpdateAgent(agent, AgentField.Disabled);
                }
            }
        }
        else
        {
            if (config.EnabledPlugins.Exists(x => x == id))
            {
                config.EnabledPlugins.Remove(id);
                db.SavePluginConfig(config);
            }
            var agentService = services.GetRequiredService<IAgentService>();
            foreach (var agentId in plugin.AgentIds)
            {
                var agent = agentService.LoadAgent(agentId).Result;
                agent.Disabled = true;
                agentService.UpdateAgent(agent, AgentField.Disabled);
            }
        }
        return plugin;
    }

    private void FindPluginDependency(string pluginId, bool enabled, ref HashSet<string> dependentPlugins, ref HashSet<string> dependentAgentIds)
    {
        var pluginDef = _plugins.FirstOrDefault(x => x.Id == pluginId);
        if (pluginDef == null) return;

        if (!pluginDef.IsCore)
        {
            pluginDef.Enabled = enabled;
            dependentPlugins.Add(pluginId);
            if (!pluginDef.AgentIds.IsNullOrEmpty())
            {
                foreach (var agentId in pluginDef.AgentIds)
                {
                    dependentAgentIds.Add(agentId);
                }
            }
        }

        var foundPlugin = _modules.FirstOrDefault(x => x.Id == pluginId);
        if (foundPlugin == null) return;

        var attr = foundPlugin.GetType().GetCustomAttribute<PluginDependencyAttribute>();
        if (attr != null && !attr.PluginNames.IsNullOrEmpty())
        {
            foreach (var name in attr.PluginNames)
            {
                var plugins = _plugins.Where(x => x.Assembly == name).ToList();
                if (plugins.IsNullOrEmpty()) return;

                foreach (var plugin in plugins)
                {
                    FindPluginDependency(plugin.Id, enabled, ref dependentPlugins, ref dependentAgentIds);
                }
            }
        }
    }

    public List<PluginMenuDef> GetPluginMenuByRoles(List<PluginMenuDef> plugins, string userRole)
    {
        if (plugins.IsNullOrEmpty()) 
            return plugins;

        var filtered = new List<PluginMenuDef>();
        foreach (var plugin in plugins)
        {
            if (plugin.Roles.IsNullOrEmpty() || plugin.Roles.Contains(userRole))
            {
                plugin.SubMenu = GetPluginMenuByRoles(plugin.SubMenu, userRole);
                filtered.Add(plugin);
            }
        }
        return filtered;
    }

}

internal class PluginModuleStore
{
    public static List<IBotSharpModule> Modules = new List<IBotSharpModule>();
    public static List<PluginDef> Plugins = new List<PluginDef>();

}
