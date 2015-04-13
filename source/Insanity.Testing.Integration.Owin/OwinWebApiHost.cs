using Insanity.Testing.Integration.Http;
using Microsoft.Owin.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Owin
{
	public class OwinWebApiHost
	{
		private TestServer host;
		private string baseAddress;
		
		public Client Client()
		{
			return Client(null);
		}

		public Client Client(string jwtToken)
		{
			return new Client(baseAddress, host.HttpClient, jwtToken);
		}

		public OwinWebApiHost(string baseAddress)
		{
			this.baseAddress = baseAddress;
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
