using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Insanity.Testing.Integration.Owin.UnitTests.WebApi;
using System.Collections.Generic;
using FluentAssertions;

namespace Insanity.Testing.Integration.Owin.UnitTests
{
    [TestClass]
    public class OwinApiHostTests
    {
        [TestMethod]
        public void OwinHost_StartWebApi_GetValueResource()
        {
            var host = new OwinApiHost("http://tests/");
            try
            {
                host.Start<Startup>();
                var result = host.Client.Get<List<string>>("api/values");

                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(new[] { "Value1", "Value2" });
            }
            finally
            {
                if (host != null)
                {
                    host.Stop();
                    host = null;
                }
            }
        }
    }
}
