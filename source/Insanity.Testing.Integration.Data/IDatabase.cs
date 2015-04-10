using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Insanity.Testing.Integration.Data
{
	public interface IDatabase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dacpac")]
		void Create(params string[] dacpacFiles);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dacpac")]
		void Update(params string[] dacpacFiles);

		void DetachDatabase();
		void DeleteDatabase();

		int ExecuteNonQuery(Action<DbCommand> prepare);
		
		TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> process);
		TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<DbConnection, IDataReader, TResult> process);

		IEnumerable<TResult> GetResults<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> process);
		Dictionary<TKey, TValue> GetResults<TKey, TValue>(Action<DbCommand> prepare, Func<IDataReader, TValue> process, Func<TValue, TKey> keySelector);

		TResult GetSingleResult<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> process);
	}
}
