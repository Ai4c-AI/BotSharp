using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Plugins.Interfaces
{
    public interface IPluginContextPack
    {
        /// <summary>
        /// Package this plugin into a <see cref="IPluginContext"/>
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        IPluginContext Pack(string pluginId);
    }
}
