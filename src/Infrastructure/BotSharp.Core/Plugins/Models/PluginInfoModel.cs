using BotSharp.Abstraction.Plugins;
using Microsoft.VisualBasic;
using Parlot.Fluent;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace BotSharp.Core.Plugins.Models;

/// <summary>
/// Plug-in information model
/// <para>Corresponding to the plug-in directory under info.json</para>
/// <para>Convention: plugin folder name = PluginId </ para >
/// <para>Convention: Plugin folder name = plugin main assembly (Asembley) name</para>
/// <para>Eg: plugins/payment/payment.dll</para>
/// <para>Convention: Logo under the plugin folder logo.png is the plug-in icon</para>
/// <para>Convention: Plug-in folder under README.md is the plug-in documentation file</para>
/// <para>Convention: Settings in the plugins settings.json sets up the file for the plugin</para>
/// </summary>
public class PluginInfoModel
{
    public string PluginId { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string Author { get; set; }

    public string Version { get; set; }

    public IList<string> SupportedVersions { get; set; }

    /// <summary>
    /// 前置依赖插件
    /// </summary>
    /// <value></value>
    public IList<string> DependPlugins { get; set; }

    #region Ctor
    public PluginInfoModel()
    {
        this.SupportedVersions = new List<string>();
        this.DependPlugins = new List<string>();
    }
    #endregion
}
