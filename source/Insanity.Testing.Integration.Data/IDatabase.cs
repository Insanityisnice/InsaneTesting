using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Insanity.Testing.Integration.Data
{
	public interface IDatabase
	{
		void Create(params string[] dacpacFiles);
		void Update(params string[] dacpacFiles);

		void DetachDatabase();
		void DeleteDatabase();

		int ExecuteNonQuery(Action<DbCommand> prepare);

		TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> function);
		TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<DbConnection, IDataReader, TResult> function);

		IEnumerable<TResult> GetResults<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> function);
		Dictionary<TKey, TValue> GetResults<TKey, TValue>(Action<DbCommand> prepare, Func<IDataReader, TValue> function, Func<TValue, TKey> keySelector);

		TResult GetSingleResult<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> function);
	}
}
