using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public sealed partial class Client
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
	}
}