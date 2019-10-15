using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebMP4.Startup))]
namespace WebMP4
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
