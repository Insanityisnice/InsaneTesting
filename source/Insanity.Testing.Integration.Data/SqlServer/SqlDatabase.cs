using Insanity.Testing.Integration.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Data
{
	public static class SqlDatabase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dacpac")]
		public static void Create(string managerName, string connectionString, Action<IDatabase> seed, params string[] dacpacFiles)
		{
			DatabaseManagers.Instance.Add(managerName, new SqlDatabaseManager(connectionString));

			var database = DatabaseManagers.Instance[managerName].Database;
			database.Create(dacpacFiles);

			if (seed != null)
			{
				seed(database);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dacpac")]
		public static void Update(string managerName, string connectionString, Action<IDatabase> seed, params string[] dacpacFiles)
		{
			DatabaseManagers.Instance.Add(managerName, new SqlDatabaseManager(connectionString));

			var database = DatabaseManagers.Instance[managerName].Database;
			database.Update(dacpacFiles);

			if (seed != null)
			{
				seed(database);
			}
		}

		public static void Detach(string managerName)
		{
			DatabaseManagers.Instance[managerName].Database.DetachDatabase();
		}

		public static void Delete(string managerName)
		{
			DatabaseManagers.Instance[managerName].Database.DeleteDatabase();
			DatabaseManagers.Instance.Remove(managerName);
		}
	}
}
