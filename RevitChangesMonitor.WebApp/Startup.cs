using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RevitChangesMonitor.WebApp.Startup))]
namespace RevitChangesMonitor.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
