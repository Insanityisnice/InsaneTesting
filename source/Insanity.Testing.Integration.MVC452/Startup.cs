using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Insanity.Testing.Integration.MVC452.Startup))]
namespace Insanity.Testing.Integration.MVC452
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
