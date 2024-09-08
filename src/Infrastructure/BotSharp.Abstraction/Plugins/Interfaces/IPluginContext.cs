using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Plugins.Interfaces
{
    /// <summary>
    /// All assemblies of each plugin are packaged into this
    /// </summary>
    public interface IPluginContext
    {
        string PluginId { get; }

        IEnumerable<Assembly> Assemblies { get; }

        Assembly LoadFromAssemblyName(AssemblyName assemblyName);

        void Unload(); 
    }
}
