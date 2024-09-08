using BotSharp.Abstraction.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Core.Plugins.lmplements;

/// <summary>
/// A collectible assembly loading context
/// In the design of the entire plugin loading context, each plugin is loaded using a separate CollectableAssemblyLoadContext, and the CollectableAssemblyLoadContext of all plugins is placed in a PluginsLoadContext object
/// </summary>
public class CollectableAssemblyLoadContext : AssemblyLoadContext, IPluginContext, ICollectableAssemblyLoadContext
{
    public string PluginId
    {
        get
        {
            return this.Name ?? "";
        }
    }

    public CollectableAssemblyLoadContext(string? name)
         : base(isCollectible: true, name: name)
    {
    }

    protected override Assembly Load(AssemblyName name)
    {
        return null;
    }
}
