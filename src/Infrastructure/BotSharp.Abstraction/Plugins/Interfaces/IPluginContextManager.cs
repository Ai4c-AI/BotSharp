using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Plugins.Interfaces;

public interface IPluginContextManager
{
    List<IPluginContext> All();

    bool Any(string pluginId);

    void Remove(string pluginId);

    IPluginContext Get(string pluginId);

    void Add(string pluginId, IPluginContext context);
}
