using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IT_Pre.Startup))]
namespace IT_Pre
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
