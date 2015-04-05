using Insanity.Testing.Integration.Owin.UnitTests.WebApi.App_Start;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Owin.UnitTests.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWebApi(WebApiConfig.Configure());            
        }
    }
}
