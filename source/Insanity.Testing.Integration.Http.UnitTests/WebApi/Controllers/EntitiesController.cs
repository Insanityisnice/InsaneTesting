using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Insanity.Testing.Integration.UnitTests.Http.WebApi.Controllers
{
	[RoutePrefix("api")]
	public class EntitiesController : ApiController
	{
		[Route("entities")]
		public IHttpActionResult Get()
		{
			return Ok(new[] {
				new { Id = 1, Name = "Name1" },
				new { Id = 2, Name = "Name2" }
			});
		}

		[Route("entities/{id}")]
		public IHttpActionResult Get(int id)
		{
			return Ok(new { Id = 1, Name = "Name1" });
		}
	}
}
