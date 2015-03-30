using Insanity.Testing.Integration.Http;
using Microsoft.Owin.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Owin
{
	public class OwinApiHost
	{
		private string baseAddress;
		private TestServer host;
		private string jwtToken;

		public ApiClient Client
		{
			get
			{
				return new ApiClient(baseAddress, host.HttpClient, jwtToken);
			}
		}

		public OwinApiHost(string baseAddress, string jwtToken = null)
		{
			this.baseAddress = baseAddress;
			this.jwtToken = jwtToken;
		}

		public void Start<TStartup>()
		{
			host = Microsoft.Owin.Testing.TestServer.Create<TStartup>();
		}

		public void Stop()
		{
			host.Dispose();
		}
	}
}
