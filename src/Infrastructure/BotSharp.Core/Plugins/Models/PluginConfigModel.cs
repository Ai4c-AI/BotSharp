using BotSharp.Abstraction.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Core.Plugins.Models;

/// <summary>
///Configuration information model for all plug-ins
///<para>Corresponding to Webapi/Apu_Hit Him/Plugin.Config Ethan</para>
///<para>Plukins = Enabled + Disabled </ para >
///<para> Once uploaded to Pulkins, it is disabled by default</para>
///</summary>
public class PluginConfigModel
{
    /// <summary>
    /// List of enabled plugins: PluginID
    /// <para>It belongs to the plugin installed</para>
    /// </summary>
    public List<string> EnabledPlugins { get; set; }

    #region ctor
    public PluginConfigModel()
    {
        this.EnabledPlugins = new List<string>();
    }
    #endregion
}
