using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public class ApiClient
	{
		#region Private Fields
		private HttpMessageInvoker invoker;
		private Request request;
		#endregion

		#region Constructors
		public ApiClient(string baseAddress, HttpMessageInvoker invoker, string authorizationToken = null)
		{
			this.invoker = invoker;
			this.request = new Request(baseAddress, authorizationToken);
		} 
		#endregion

		#region Get
		public string Get(string uri)
		{
			return invoker.SendAsync(request.Create(uri, HttpMethod.Get, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						 .ContinueWith<string>(Response.ProcessAsString, TaskContinuationOptions.OnlyOnRanToCompletion)
						 .Result;
		}

		public T Get<T>(string uri)
		{
			return invoker.SendAsync(request.Create(uri, HttpMethod.Get, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						 .ContinueWith<T>(Response.ProcessAs<T>, TaskContinuationOptions.OnlyOnRanToCompletion)
						 .Result;
		}
		#endregion

		#region Post
		public string Post(string uri, string content)
		{
			return invoker.SendAsync(request.Create(uri, HttpMethod.Post, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						  .ContinueWith<string>(Response.ProcessAsString, TaskContinuationOptions.OnlyOnRanToCompletion)
						  .Result;
		}

		public T Post<T>(string uri, object content)
		{
			return invoker.SendAsync(request.Create(uri, HttpMethod.Post, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						  .ContinueWith<T>(Response.ProcessAs<T>, TaskContinuationOptions.OnlyOnRanToCompletion)
						  .Result;
		}
		#endregion

		#region Put
		public string Put(string uri, string content)
		{
			return invoker.SendAsync(request.Create(uri, HttpMethod.Put, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						  .ContinueWith<string>(Response.ProcessAsString, TaskContinuationOptions.OnlyOnRanToCompletion)
						  .Result;
		}

		public T Put<T>(string uri, object content)
		{
			return invoker.SendAsync(request.Create(uri, HttpMethod.Put, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						  .ContinueWith<T>(Response.ProcessAs<T>, TaskContinuationOptions.OnlyOnRanToCompletion)
						  .Result;
		}
		#endregion

		#region Delete
		public void Delete(string uri)
		{
			invoker.SendAsync(request.Create(uri, HttpMethod.Delete, JsonMediaTypeFormatter.DefaultMediaType.MediaType), default(CancellationToken))
						  .ContinueWith<string>(Response.ProcessAsString, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
		#endregion
		
		#region Contained Types
		private class Request
		{
			private string baseAddress;
			private string authorizationToken;

			public Request(string baseAddress, string authorizationToken)
			{
				this.baseAddress = baseAddress;
				this.authorizationToken = authorizationToken;
			}

			public HttpRequestMessage Create(string uri, HttpMethod method, string mediaType)
			{
				var request = new HttpRequestMessage()
				{
					RequestUri = new Uri(baseAddress + uri),
					Method = method,
				};
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

				if (!string.IsNullOrEmpty(authorizationToken))
				{
					request.Headers.Add("Authorization", "Bearer " + authorizationToken);
				}

				return request;
			}

			public HttpRequestMessage Create(string uri, HttpMethod method, string mediaType, string content)
			{
				var request = Create(uri, method, mediaType);
				request.Content = new StringContent(content, Encoding.UTF8, mediaType);

				return request;
			}

			public HttpRequestMessage Create<T>(string uri, HttpMethod method, string mediaType, T content, MediaTypeFormatter formatter)
			{
				var request = Create(uri, method, mediaType);
				request.Content = new ObjectContent<T>(content, formatter);

				return request;
			}
		}

		private class Response
		{
			#region Private Fields
			private static IDictionary<HttpStatusCode, Func<HttpContent, Exception>> statusCodeExceptions = new Dictionary<HttpStatusCode, Func<HttpContent, Exception>>()
			{
				{ HttpStatusCode.NotFound, content => new InvalidOperationException("The resource could not be found.")}
			}; 
			#endregion

			public static string ProcessAsString(Task<HttpResponseMessage> responseMessageTask)
			{
				using (var responseMessage = responseMessageTask.Result)
				{
					return ReadString(responseMessage);
				}
			}

			public static T ProcessAs<T>(Task<HttpResponseMessage> responseMessageTask)
			{
				using (var responseMessage = responseMessageTask.Result)
				{
					return ReadAs<T>(responseMessage);
				}
			}

			#region Private Methods
			private static string ReadString(HttpResponseMessage responseMessage)
			{
				Validate(responseMessage);
				return responseMessage.Content != null ? responseMessage.Content.ReadAsStringAsync().Result : "";
			}

			private static T ReadAs<T>(HttpResponseMessage responseMessage)
			{
				Validate(responseMessage);
				return responseMessage.Content != null ? responseMessage.Content.ReadAsAsync<T>().Result : default(T);
			}

			private static void Validate(HttpResponseMessage responseMessage)
			{
				if (!responseMessage.IsSuccessStatusCode)
				{
					throw statusCodeExceptions.ContainsKey(responseMessage.StatusCode)
						? statusCodeExceptions[responseMessage.StatusCode](responseMessage.Content)
						: new InvalidOperationException("Unknown status code.");
				}
			}
			#endregion
		}
		#endregion
	}
}
