using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Net.Http;
using ApprovalTests;
using ApprovalTests.Reporters;
using System.Threading;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http.UnitTests
{
	[TestClass]
	public class ApiClientTests
	{
		[Ignore]
		[TestMethod]
		[UseReporter(typeof(DiffReporter))]
		public void Get_EmptyResource_EmptyReturned()
		{
			var invoker = new TestInvoker(new Task<HttpResponseMessage>(() => new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("[]") }));

			ApiClient target = new ApiClient("http://test_address", invoker);

			var result  = target.Get("/api/resources");
			Approvals.VerifyJson(result);
		}
	}

	public class TestInvoker : HttpMessageInvoker
	{
		private Task<HttpResponseMessage> response;

		//TODO: unable to pass null in..  Need to find a good way to mock up HttpMessageInvoker or Handler in order to test.
		public TestInvoker(Task<HttpResponseMessage> response) : base(null)
		{
			this.response = response;
		}

		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return response;
		}
	}
}
