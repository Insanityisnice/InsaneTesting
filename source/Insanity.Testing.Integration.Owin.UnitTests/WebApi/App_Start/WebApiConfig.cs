using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Insanity.Testing.Integration.Owin.UnitTests.WebApi.App_Start
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Configure()
        {
            HttpConfiguration config = new HttpConfiguration();

            // Web API routes
            config.MapHttpAttributeRoutes();
            
            return config;
        }
    }
}
