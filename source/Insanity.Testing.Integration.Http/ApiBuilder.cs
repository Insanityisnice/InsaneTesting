using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Http
{
	public class ApiUriBuilder
	{
		private string _uri;

		public QueryBuilder Query { get { return new QueryBuilder(this); } }

		public ApiUriBuilder(string uri)
		{
			_uri = uri;
		}

		public ApiUriBuilder WithId(int id)
		{
			_uri = String.Format("{0}/{1}", _uri, id);
			return this;
		}

		public ApiUriBuilder WithName(string name)
		{
			_uri = String.Format("{0}/{1}", _uri, name);
			return this;
		}

		public ApiUriBuilder Child(string child)
		{
			_uri = String.Format("{0}/{1}", _uri, child);
			return this;
		}

		public override string ToString()
		{
			return _uri;
		}

		public class QueryBuilder
		{
			private ApiUriBuilder parent;
			private StringBuilder query;

			internal QueryBuilder(ApiUriBuilder parent)
			{
				this.parent = parent;
			}

			public QueryBuilder AddParamter(string name, string value)
			{
				return AppendFormat("{0}={1}", name, value);
			}

			private QueryBuilder AppendFormat(string format, params object[] parameters)
			{
				if (query.Length > 0)
				{
					format = "&" + format;
				}

				query = query.AppendFormat(format, parameters);
				return this;
			}

			public override string ToString()
			{
				return parent.ToString() + "?" + query;
			}
		}
	}
}
