using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public sealed partial class Client
	{
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

			#region Public Methods
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
			#endregion

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
	}
}
