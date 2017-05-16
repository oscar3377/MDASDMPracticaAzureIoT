using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppSensores.Startup))]
namespace WebAppSensores
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
