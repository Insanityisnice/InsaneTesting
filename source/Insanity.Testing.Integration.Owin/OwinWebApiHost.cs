using Insanity.Testing.Integration.Http;
using Microsoft.Owin.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Owin
{
	public class OwinWebApiHost : IDisposable
	{
		#region Private Fields
		private TestServer host;
		private string baseAddress; 
		#endregion

		#region Constructors
		public OwinWebApiHost(string baseAddress)
		{
			this.baseAddress = baseAddress;
		} 

		~OwinWebApiHost()
		{
			Dispose(false);
		}
		#endregion

		#region Public Methods
		public Client Client()
		{
			return Client(null);
		}

		public Client Client(string jwtToken)
		{
			return new Client(baseAddress, host.HttpClient, jwtToken);
		}

		public void Start<TStartup>()
		{
			host = Microsoft.Owin.Testing.TestServer.Create<TStartup>();
		}
		#endregion

		#region IDisposable Implementation
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (host != null)
				{
					host.Dispose();
					host = null;
				}
			}
		}
		#endregion
	}
}
