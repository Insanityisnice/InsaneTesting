using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public sealed class Request
	{
		private string baseAddress;
		private HttpClient client;
		private string authorizationToken;

		public Request(string baseAddress, HttpClient client = null, string authorizationToken = null)
		{
			this.baseAddress = baseAddress;
			this.client = client;
			this.authorizationToken = authorizationToken;
		}

		public void Get(string uri, Action<string> processor)
		{
			var request = CreateRequest(uri, HttpMethod.Get, JsonMediaTypeFormatter.DefaultMediaType.MediaType);

			using (var response = Client.SendAsync(request).Result)
			{
				processor(ProcessAsStringAsync(response).Result);
			}
		}

		public void Post(string uri, string content, Action<string> processor)
		{
			var request = CreateRequest(uri, HttpMethod.Post, JsonMediaTypeFormatter.DefaultMediaType.MediaType, content);

			using (var response = Client.SendAsync(request).Result)
			{
				processor(ProcessAsStringAsync(response).Result);
			}
		}

		public void Put(string uri, string content, Action<string> processor)
		{
			var request = CreateRequest(uri, HttpMethod.Put, JsonMediaTypeFormatter.DefaultMediaType.MediaType, content);

			using (var response = Client.SendAsync(request).Result)
			{
				processor(ProcessAsStringAsync(response).Result);
			}
		}

		public void Delete(string uri)
		{
			var request = CreateRequest(uri, HttpMethod.Delete, JsonMediaTypeFormatter.DefaultMediaType.MediaType, string.Empty);

			using (var response = Client.SendAsync(request).Result)
			{
				Verify(response).Wait();
			}
		}

		public void SetPrincipal(IEnumerable<Claim> claims)
		{
			var identity = new ClaimsIdentity();
			identity.AddClaims(claims);

			Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
		}

		private HttpClient Client { get { return client; } }

		private HttpRequestMessage CreateRequest(string uri, HttpMethod method, string mediaType)
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

		private HttpRequestMessage CreateRequest(string uri, HttpMethod method, string mediaType, string content)
		{
			var request = CreateRequest(uri, method, mediaType);
			request.Content = new StringContent(content, Encoding.UTF8, mediaType);

			return request;
		}

		private HttpRequestMessage CreateRequest<T>(string uri, HttpMethod method, string mediaType, T content, MediaTypeFormatter formatter)
		{
			var request = CreateRequest(uri, method, mediaType);
			request.Content = new ObjectContent<T>(content, formatter);

			return request;
		}

		private static async Task<string> ProcessAsStringAsync(HttpResponseMessage response)
		{
			await Verify(response);
			return response.Content != null ? await response.Content.ReadAsStringAsync() : null;
		}

		private static async Task Verify(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode == false)
			{
				StringBuilder sb = new StringBuilder();
				var error = await response.Content.ReadAsAsync<HttpError>();

				sb.AppendLine(error.ExceptionType);
				sb.AppendLine(error.ExceptionMessage);
				sb.AppendLine(error.StackTrace);

				throw new Exception(sb.ToString());
			}
		}

		private string CreateAuthorizationHeader(List<Claim> claims, string issuer, string audience, X509Certificate2 certificate)
		{
			//var credentials = new X509SigningCredentials(certificate);
			//var jwt = new JwtSecurityToken(
			//				issuer,
			//				audience,
			//				claims,
			//				DateTime.Now,
			//				DateTime.Now.AddMinutes(10),
			//				credentials);

			//var x509credential = credentials as X509SigningCredentials;
			//if (x509credential != null)
			//{
			//	jwt.Header.Add("kid", Base64UrlEncoder.Encode(x509credential.Certificate.GetCertHash()));
			//}

			//var handler = new JwtSecurityTokenHandler();
			//return handler.WriteToken(jwt);

			return null;
		}
	}
}
