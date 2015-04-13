using Insanity.Testing.Integration.Http.UnitTests.WebApi.App_Start;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http.UnitTests.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWebApi(WebApiConfig.Configure());            
        }
    }
}
