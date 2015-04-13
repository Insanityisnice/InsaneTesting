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

				responseMessage.EnsureSuccessStatusCode();
				return responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : await Task<string>.FromResult("");
			}

			private static async Task<T> ReadAs<T>(HttpResponseMessage responseMessage)
			{
				Trace.Write(String.Format(CultureInfo.InvariantCulture, "Processing message as {0}.", typeof(T).GetType().Name), "ResponseMessage");

				responseMessage.EnsureSuccessStatusCode();
				return responseMessage.Content != null ? await responseMessage.Content.ReadAsAsync<T>() : await Task<T>.FromResult(default(T));
			}
			#endregion
		}
	}
}
