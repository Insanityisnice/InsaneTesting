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
		public static void SetupNew(string managerName, string connectionString, Action<IDatabase> setup, params string[] dacpacFiles)
		{
			DatabaseManagers.Instance.Add(managerName, new SqlDatabaseManager(connectionString));

			var database = DatabaseManagers.Instance[managerName].Database;
			database.Create(dacpacFiles);

			if (setup != null)
			{
				setup(database);
			}
		}

		public static void Setup(string managerName, string connectionString, Action<IDatabase> setup, params string[] dacpacFiles)
		{
			DatabaseManagers.Instance.Add(managerName, new SqlDatabaseManager(connectionString));

			var database = DatabaseManagers.Instance[managerName].Database;
			database.Update(dacpacFiles);

			if (setup != null)
			{
				setup(database);
			}
		}

		public static void Detach(string managerName)
		{
			DatabaseManagers.Instance[managerName].Database.DetachDatabase();
		}

		public static void Delete(string managerName)
		{
			DatabaseManagers.Instance[managerName].Database.DeleteDatabase();
		}
	}
}
