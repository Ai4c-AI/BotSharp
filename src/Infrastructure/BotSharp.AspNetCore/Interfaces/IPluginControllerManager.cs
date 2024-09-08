using System.Reflection;

namespace BotSharp.AspNetCore.Interfaces;

public interface IPluginControllerManager
{
    void AddControllers(Assembly assembly);

    void RemoveControllers(string pluginId);
}
