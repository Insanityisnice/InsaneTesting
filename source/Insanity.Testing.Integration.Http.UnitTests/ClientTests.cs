using Insanity.Testing.Integration.Http.UnitTests.WebApi;
using Insanity.Testing.Integration.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using FluentAssertions;
using ApprovalTests;
using ApprovalTests.Reporters;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace Insanity.Testing.Integration.Http.UnitTests
{
	[TestClass]
	[UseReporter(typeof(DiffReporter))]
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

		#region GetAsync
		[TestMethod]
		public async Task Client_GetAsync_Raw()
		{
			var uri = new UriBuilder("values");

			var result = await host.Client().GetAsync(uri.ToString());

			result.Should().NotBeNull();
			result.Should().Be("[\"Value1\",\"Value2\"]");
		}
		#endregion

		#region GetAsync<T>
		[TestMethod]
		public async Task Client_GetAsync_ListString()
		{
			var uri = new UriBuilder("values");

			var result = await host.Client().GetAsync<List<string>>(uri.ToString());

			result.Should().NotBeNull();
			result.Should().BeEquivalentTo(new[] { "Value1", "Value2" });
		}

		[TestMethod]
		public async Task Client_GetAsync_String()
		{
			var uri = new UriBuilder("values").WithId(1);

			var result = await host.Client().GetAsync<string>(uri.ToString());

			result.Should().NotBeNull();
			result.Should().Be("Value1");
		}

		[TestMethod]
		public async Task Client_GetAsync_ListEntity()
		{
			var uri = new UriBuilder("entities");

			var result = await host.Client().GetAsync<List<Entity>>(uri.ToString());

			result.Should().NotBeNull();
			result.ShouldBeEquivalentTo(new List<Entity>() 
			{ 
				new Entity() { Id = 1, Name = "Name1" }, 
				new Entity() { Id = 2, Name = "Name2" }
			});
		}

		[TestMethod]
		public async Task Client_GetAsync_ListWrongType()
		{
			var uri = new UriBuilder("entities");

			var result = await host.Client().GetAsync<List<EntityFalse>>(uri.ToString());

			result.Should().NotBeNull();
			result.ShouldBeEquivalentTo(new List<EntityFalse>() 
			{ 
				new EntityFalse(), 
				new EntityFalse()
			});
		}

		[TestMethod]
		public async Task Client_GetAsync_WrongType()
		{
			var uri = new UriBuilder("entities").WithId(1);

			var result = await host.Client().GetAsync<EntityFalse>(uri.ToString());

			result.Should().NotBeNull();
			result.ShouldBeEquivalentTo(new EntityFalse());
		}

		[TestMethod]
		public async Task Client_GetAsync_MissingResource()
		{
			var uri = new UriBuilder("notfound").WithId(1);

			new Func<Task>(async () => await host.Client().GetAsync<string>(uri.ToString()))
					.ShouldThrow<HttpRequestException>();
		}
		#endregion

		#region PostAsync
		[TestMethod]
		public async Task Client_PostAsyncRaw_String()
		{
			var uri = new UriBuilder("values");

			var result = await host.Client().PostAsync(uri.ToString(), "Value1");

			result.Should().NotBeNull();
			result.Should().Be("\"Value1\"");
		}

		[TestMethod]
		public async Task Client_PostAsync_String()
		{
			var uri = new UriBuilder("values");

			var result = await host.Client().PostAsync<string>(uri.ToString(), "Value1");

			result.Should().NotBeNull();
			result.Should().Be("Value1");
		}
		#endregion

		#region PutAsync
		[TestMethod]
		public async Task Client_PutAsyncRaw_String()
		{
			var uri = new UriBuilder("values").WithId(1);

			var result = await host.Client().PutAsync(uri.ToString(), "Value1");

			result.Should().NotBeNull();
			result.Should().Be("\"Value1\"");
		}

		[TestMethod]
		public async Task Client_PutAsync_String()
		{
			var uri = new UriBuilder("values").WithId(1);

			var result = await host.Client().PutAsync<string>(uri.ToString(), "Value1");

			result.Should().NotBeNull();
			result.Should().Be("Value1");
		}
		#endregion

		#region DeleteAsync
		[TestMethod]
		public async Task Client_DeleteAsync()
		{
			var uri = new UriBuilder("values").WithId(1);

			await host.Client().DeleteAsync(uri.ToString());
		}
		#endregion
	}

	public class Entity
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class EntityFalse
	{
		public int DoesntExist { get; set; }
	}
}
