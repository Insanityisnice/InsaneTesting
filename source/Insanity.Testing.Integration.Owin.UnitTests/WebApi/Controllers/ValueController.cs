using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Insanity.Testing.Integration.Owin.UnitTests.WebApi.Controllers
{
    [RoutePrefix("api")]
    public class ValueController : ApiController
    {
        [Route("values")]
        public IHttpActionResult Get()
        {
            var values = new[] { "Value1", "Value2" };
            return Ok(values);
        }
    }
}
