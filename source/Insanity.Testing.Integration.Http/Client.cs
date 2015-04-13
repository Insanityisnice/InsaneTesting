using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public class Client
	{
		#region Private Fields
		private HttpMessageInvoker invoker;
		private Request request;
		#endregion

		#region Constructors
		public Client(string baseAddress, HttpMessageInvoker invoker)
			: this(baseAddress, invoker, null)
		{
		}
		
		public Client(string baseAddress, HttpMessageInvoker invoker, string authorizationToken)
		{
			this.invoker = invoker;
			this.request = new Request(baseAddress, authorizationToken);
		} 
		#endregion

		#region Get
		public string Get(string resource)
		{
			return GetAsync(resource).GetAwaiter().GetResult();
		}

		public T Get<T>(string resource)
		{
			return GetAsync<T>(resource).GetAwaiter().GetResult();
		}

		public async Task<string> GetAsync(string resource)
		{
			var response = await request.SendAsync(resource, HttpMethod.Get, JsonMediaTypeFormatter.DefaultMediaType.MediaType, invoker.SendAsync);
			return await Response.ProcessAsStringAsync(response);
		}

		public async Task<T> GetAsync<T>(string resource)
		{
			var result = await request.SendAsync(resource, HttpMethod.Get, JsonMediaTypeFormatter.DefaultMediaType.MediaType, invoker.SendAsync);
			return await Response.ProcessAsAsync<T>(result);
		}
		#endregion

		#region Post
		public string Post(string resource, string content)
		{
			return PostAsync(resource, content).GetAwaiter().GetResult();
		}

		public T Post<T>(string resource, T content)
		{
			return PostAsync<T>(resource, content).GetAwaiter().GetResult();
		}

		public async Task<string> PostAsync(string resource, string content)
		{
			var result = await request.SendAsync(resource, HttpMethod.Post, JsonMediaTypeFormatter.DefaultMediaType.MediaType, content, invoker.SendAsync);
			return await Response.ProcessAsStringAsync(result);
		}

		public async Task<T> PostAsync<T>(string resource, T content)
		{
			var result = await request.SendAsync(resource, HttpMethod.Post, JsonMediaTypeFormatter.DefaultMediaType.MediaType, content, new JsonMediaTypeFormatter(), invoker.SendAsync);
			return await Response.ProcessAsAsync<T>(result);
		}
		#endregion

		#region Put
		public string Put(string resource, string content)
		{
			return PutAsync(resource, content).GetAwaiter().GetResult();
		}

		public T Put<T>(string resource, T content)
		{
			return PutAsync<T>(resource, content).GetAwaiter().GetResult();
		}

		public async Task<string> PutAsync(string resource, string content)
		{
			var result = await request.SendAsync(resource, HttpMethod.Put, JsonMediaTypeFormatter.DefaultMediaType.MediaType, content, invoker.SendAsync);
			return await Response.ProcessAsStringAsync(result);
		}

		public async Task<T> PutAsync<T>(string resource, T content)
		{
			var result = await request.SendAsync(resource, HttpMethod.Put, JsonMediaTypeFormatter.DefaultMediaType.MediaType, content, new JsonMediaTypeFormatter(), invoker.SendAsync);
			return await Response.ProcessAsAsync<T>(result);
		}
		#endregion

		#region Delete
		public void Delete(string resource)
		{
			DeleteAsync(resource).GetAwaiter().GetResult();
		}

		public async Task DeleteAsync(string resource)
		{
			var result = await request.SendAsync(resource, HttpMethod.Delete, JsonMediaTypeFormatter.DefaultMediaType.MediaType, invoker.SendAsync);
			await Response.ProcessAsStringAsync(result);
		}
		#endregion
		
		#region Contained Types
		private class Request
		{
			private string baseAddress;
			private string authorizationToken;

			public Request(string baseAddress, string authorizationToken)
			{
				this.baseAddress = baseAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? baseAddress : baseAddress + "/";
				this.authorizationToken = authorizationToken;
			}
	
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
		}

		private static class Response
		{
			#region Private Fields
			private static IDictionary<HttpStatusCode, Func<string, Exception>> statusCodeExceptions = new Dictionary<HttpStatusCode, Func<string, Exception>>()
			{
				{ HttpStatusCode.Unauthorized, stringContent => new UnauthorizedAccessException(stringContent)},
				{ HttpStatusCode.Forbidden, stringContent => new UnauthorizedAccessException(stringContent)},
				{ HttpStatusCode.NotImplemented, stringContent => new NotImplementedException(stringContent)},
				{ HttpStatusCode.RequestTimeout, stringContent => new TimeoutException(stringContent)},
			}; 
			#endregion

			public static async Task<string> ProcessAsStringAsync(HttpResponseMessage responseMessage)
			{
				using (responseMessage)
				{
					return await ReadString(responseMessage);
				}
			}

			public static async Task<T> ProcessAsAsync<T>(HttpResponseMessage responseMessage)
			{
				using (responseMessage)
				{
					return await ReadAs<T>(responseMessage);
				}
			}

			#region Private Methods
			private static async Task<string> ReadString(HttpResponseMessage responseMessage)
			{
				Trace.Write("Processing message as string.", "ResponseMessage");
				
				await Validate(responseMessage);
				return responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : await Task<string>.FromResult("");
			}

			private static async Task<T> ReadAs<T>(HttpResponseMessage responseMessage)
			{
				Trace.Write(String.Format(CultureInfo.InvariantCulture, "Processing message as {0}.", typeof(T).GetType().Name), "ResponseMessage");
				
				await Validate(responseMessage);
				return responseMessage.Content != null ? await responseMessage.Content.ReadAsAsync<T>() : await Task<T>.FromResult(default(T));
			}

			private static async Task Validate(HttpResponseMessage responseMessage)
			{
				if (!responseMessage.IsSuccessStatusCode)
				{
					var error = await responseMessage.Content.ReadAsStringAsync();
					Trace.Write(error, "HttpError");
					
					throw statusCodeExceptions.ContainsKey(responseMessage.StatusCode)
						? statusCodeExceptions[responseMessage.StatusCode](error)
						: new InvalidOperationException(error);
				}
			}
			#endregion
		}
		#endregion
	}
}