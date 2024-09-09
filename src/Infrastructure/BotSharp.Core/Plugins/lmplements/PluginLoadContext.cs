using BotSharp.Abstraction.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Core.Plugins.lmplements;

/// <summary>
/// PluginLoadContext
/// </summary>
public class PluginLoadContext : PositivePluginLoadContext, IPluginContext
{
    public PluginLoadContext(string pluginId, string pluginMainDllFilePath) 
        : base(pluginId, pluginMainDllFilePath)
    {

    }
}
