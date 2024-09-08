using Microsoft.AspNetCore.Http;

namespace BotSharp.AspNetCore.Interfaces
{
    public interface IPluginApplicationBuilderManager
    {
        void ReBuild();

        RequestDelegate GetBuildResult();
    }
}
