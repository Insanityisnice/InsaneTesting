using Insanity.Testing.Integration.Http.UnitTests.WebApi;
using Insanity.Testing.Integration.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using FluentAssertions;

namespace Insanity.Testing.Integration.Http.UnitTests
{
	[TestClass]
	public class ClientTests
	{
		#region Test Setup
		private static OwinWebApiHost host;

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			host = new OwinWebApiHost("http://tests/api");
			host.Start<Startup>();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (host != null)
			{
				host.Dispose();
				host = null;
			}
		} 
		#endregion

		[TestMethod]
		public void Client_Get_ListString()
		{
			var uri = new UriBuilder("values");

			var result = host.Client().Get<List<string>>(uri.ToString());

			result.Should().NotBeNull();
			result.Should().BeEquivalentTo(new[] { "Value1", "Value2" });
		}
	}
}
