using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public sealed partial class Client
	{
		private class Request
		{
			#region Private Fields
			private string baseAddress;
			private string authorizationToken; 
			#endregion

			#region Constructors
			public Request(string baseAddress, string authorizationToken)
			{
				this.baseAddress = baseAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? baseAddress : baseAddress + "/";
				this.authorizationToken = authorizationToken;
			} 
			#endregion

			#region Public Methods
			public async Task<HttpResponseMessage> SendAsync(string resource, HttpMethod method, string mediaType, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> process)
			{
				using (var request = new HttpRequestMessage(method, new Uri(baseAddress + resource)))
				{
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

					if (!string.IsNullOrEmpty(authorizationToken))
					{
						Trace.Write("Authorized Request", "Information");
						request.Headers.Add("Authorization", "Bearer " + authorizationToken);
					}

					return await process(request, default(CancellationToken));
				}
			}

			public async Task<HttpResponseMessage> SendAsync(string resource, HttpMethod method, string mediaType, string content, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> process)
			{
				return await SendAsync(resource, method, mediaType, async (request, cancellationToken) =>
				{
					request.Content = new StringContent(content, Encoding.UTF8, mediaType);
					return await process(request, cancellationToken);
				});
			}

			public async Task<HttpResponseMessage> SendAsync<T>(string resource, HttpMethod method, string mediaType, T content, MediaTypeFormatter formatter, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> process)
			{
				return await SendAsync(resource, method, mediaType, async (request, cancellationToken) =>
				{
					request.Content = new ObjectContent<T>(content, formatter);
					return await process(request, cancellationToken);
				});
			} 
			#endregion
		}
	}
}
