using System;
using System.Globalization;
using System.Text;

namespace Insanity.Testing.Integration.Http
{
	public class UriBuilder
	{
		private string resource;

		public QueryBuilder Query { get { return new QueryBuilder(this); } }

		public UriBuilder(string resource)
		{
			this.resource = resource;
		}

		public UriBuilder WithId(int id)
		{
			resource = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", resource, id);
			return this;
		}

		public UriBuilder WithName(string name)
		{
			resource = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", resource, name);
			return this;
		}

		public UriBuilder Child(string name)
		{
			resource = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", resource, name);
			return this;
		}

		public override string ToString()
		{
			return resource;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class QueryBuilder
		{
			private UriBuilder parent;
			private StringBuilder query;

			internal QueryBuilder(UriBuilder parent)
			{
				this.parent = parent;
			}

			public QueryBuilder AddParameter(string name, string value)
			{
				return AppendFormat("{0}={1}", name, value);
			}

			private QueryBuilder AppendFormat(string format, params object[] parameters)
			{
				if (query.Length > 0)
				{
					format = "&" + format;
				}

				query = query.AppendFormat(CultureInfo.InvariantCulture, format, parameters);
				return this;
			}

			public override string ToString()
			{
				return parent.ToString() + "?" + query;
			}
		}
	}
}
