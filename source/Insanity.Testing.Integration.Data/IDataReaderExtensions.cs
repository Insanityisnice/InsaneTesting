using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
	//TODO: This needs some serious cleanup.
	public static class IDataReaderExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public static List<TResult> ToList<TResult>(this IDataReader reader, Func<IDataReader, TResult> process)
		{
			if (process == null) throw new ArgumentNullException("process");
			if (reader == null) throw new ArgumentNullException("reader");

			var list = new List<TResult>();

			while (reader.Read())
			{
				list.Add(process(reader));
			}

			return list;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDataReader reader, Func<IDataReader, TValue> process, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> equalityComparer = null)
		{
			if (process == null) throw new ArgumentNullException("process");
			if (reader == null) throw new ArgumentNullException("reader");

			if (keySelector == null)
			{
				throw new ArgumentNullException("keySelector");
			}

			var dictionary = new Dictionary<TKey, TValue>(equalityComparer ?? EqualityComparer<TKey>.Default);

			while (reader.Read())
			{
				TValue value = process(reader);
				dictionary.Add(keySelector(value), value);
			}

			return dictionary;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDataReader reader, Func<IDataReader, TValue> process, Func<IDataReader, TKey> keySelector, IEqualityComparer<TKey> equalityComparer = null)
		{
			if (process == null) throw new ArgumentNullException("process");
			if (reader == null) throw new ArgumentNullException("reader");
			if (keySelector == null) throw new ArgumentNullException("keySelector");

			var dictionary = new Dictionary<TKey, TValue>(equalityComparer ?? EqualityComparer<TKey>.Default);

			while (reader.Read())
			{
				TValue value = process(reader);
				dictionary.Add(keySelector(reader), value);
			}

			return dictionary;
		}

		public static T Get<T>(this IDataRecord record, string name)
		{
			T value;

			TryGet<T>(record, name, out value);

			return value;
		}

		public static T Get<T>(this IDataRecord record, int ordinal)
		{
			T value;

			TryGet<T>(record, ordinal, out value);

			return value;
		}

		public static T Get<T>(this IDataRecord record, string name, T defaultValue)
		{
			T value;

			if (!TryGet<T>(record, name, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public static T Get<T>(this IDataRecord record, int ordinal, T defaultValue)
		{
			T value;

			if (!TryGet<T>(record, ordinal, out value))
			{
				value = defaultValue;
			}

			return value;
		}

		public static bool TryGet<T>(this IDataRecord record, string name, out T value)
		{
			if (record == null)
			{
				throw new ArgumentNullException("record");
			}

			return TryGet<T>(record, record.GetOrdinal(name), out value);
		}

		public static bool TryGet<T>(this IDataRecord record, int ordinal, out T value)
		{
			if (record == null)
			{
				throw new ArgumentNullException("record");
			}

			if (record.IsDBNull(ordinal))
			{
				value = default(T);
				return false;
			}

			if (typeof(T) == typeof(string))
			{
				value = (T)(object)record.GetString(ordinal);
				return true;
			}

			Type t = typeof(T);

			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				t = t.GetGenericArguments()[0];
			}

			if (!t.IsValueType)
			{
				throw new InvalidCastException("Cannot cast SQL value to " + typeof(T).Name);
			}

			object v = record.GetValue(ordinal);

			if (t.IsAssignableFrom(v.GetType()))
			{
				value = (T)(object)v;
			}
			else if (t.IsEnum)
			{
				if (v is string)
				{
					value = (T)Enum.Parse(typeof(T), v.ToString());
				}
				else if (typeof(T) == t)
				{
					if (v is int)
						value = (T)(object)v;
					else
						value = (T)Convert.ChangeType(v, typeof(int), CultureInfo.InvariantCulture);
				}
				else
				{
					value = (T)Enum.Parse(t, v.ToString());
				}
			}
			else
			{
				value = (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
			}

			return true;
		}
	}
}
