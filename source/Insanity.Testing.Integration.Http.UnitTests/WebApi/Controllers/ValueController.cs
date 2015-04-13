using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Insanity.Testing.Integration.Http.UnitTests.WebApi.Controllers
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

		[Route("values/{id}")]
		public IHttpActionResult Get(int id)
		{
			return Ok("Value1");
		}

		[Route("values")]
		public IHttpActionResult Post()
		{
			return Ok("Value1");
		}

		[Route("values/{id}")]
		public IHttpActionResult Put(int id)
		{
			return Ok("Value1");
		}

		[Route("values/{id}")]
		public IHttpActionResult Delete(int id)
		{
			return Ok();
		}
	}
}
