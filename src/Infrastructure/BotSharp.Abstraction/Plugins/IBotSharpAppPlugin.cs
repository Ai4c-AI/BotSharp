using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotSharp.Abstraction.Plugins
{
    public interface IBotSharpAppPlugin : IPlugin
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        void ConfigureServices(IServiceCollection services, IConfiguration config);

        int ConfigureServicesOrder { get; }

        void Configure(IApplicationBuilder app);

        int ConfigureOrder { get; }
    }
}
