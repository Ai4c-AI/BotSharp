namespace BotSharp.Abstraction.Plugins;

public abstract class BasePlugin : IPlugin
{
    public virtual (bool IsSuccess, string Message) AfterEnable()
    {
        return (true, "Enabled successfully");
    }

    public virtual (bool IsSuccess, string Message) BeforeDisable()
    {
        return (true, "Disable successfully");
    }


    public virtual (bool IsSuccess, string Message) Update(string currentVersion, string targetVersion)
    {
        return (true, "Update Success");
    }

    public virtual void AppStart()
    {

    }

    public virtual List<string> AppStartOrderDependPlugins
    {
        get
        {
            return new List<string>();
        }
    }
}
