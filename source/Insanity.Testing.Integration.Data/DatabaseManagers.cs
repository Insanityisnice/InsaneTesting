using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Data
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class DatabaseManagers
	{
		public static DatabaseManagers Instance { get; private set; }

		private IDictionary<string, IDatabaseManager> managers = new Dictionary<string, IDatabaseManager>();
		private ReaderWriterLockSlim managersLock = new ReaderWriterLockSlim();

		static DatabaseManagers()
		{
			Instance = new DatabaseManagers();
		}

		private DatabaseManagers()
		{
			
		}

		~DatabaseManagers()
		{
			if(managersLock != null)
			{
				managersLock.Dispose();
			}
		}

		public static void SetDataDirectory()
		{
			SetDataDirectory(String.Empty);
		}

		public static void SetDataDirectory(string dataDirectory)
		{
			if (String.IsNullOrWhiteSpace(dataDirectory))
			{
				dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
			}
			AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
		}

		public static void SetRandomDatabaseName(string connectionStringName)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder();
			connectionStringBuilder.ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

			var guid = Guid.NewGuid();
			var newDatabaseName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", connectionStringBuilder.InitialCatalog, guid);
			connectionStringBuilder.InitialCatalog = newDatabaseName;

			if(!String.IsNullOrWhiteSpace(connectionStringBuilder.AttachDBFilename))
			{
				connectionStringBuilder.AttachDBFilename = connectionStringBuilder.AttachDBFilename.Replace(".mdf", String.Format(CultureInfo.InvariantCulture, "_{0}.mdf", newDatabaseName));
			}

			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.ConnectionStrings.ConnectionStrings[connectionStringName].ConnectionString = connectionStringBuilder.ConnectionString;
			configuration.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection("connectionStrings");
		}

		public void Add(string name, IDatabaseManager manager)
		{
			managersLock.EnterWriteLock();
			try
			{
				managers.Add(name, manager);
			}
			finally
			{
				managersLock.ExitWriteLock();
			}
		}

		public void Remove(string name)
		{
			managersLock.EnterWriteLock();
			try
			{
				if (managers.ContainsKey(name))
				{
					managers.Remove(name);
				}
			}
			finally
			{
				managersLock.ExitWriteLock();
			}
		}

		public IDatabaseManager this[string name]
		{
			get
			{
				managersLock.EnterReadLock();
				try
				{
					return managers[name];
				}
				finally
				{
					managersLock.ExitReadLock();
				}
			}
		}

		public void DetachDatabase(string name)
		{
			managersLock.EnterReadLock();
			try
			{
				if (managers.ContainsKey(name))
				{
					managers[name].Database.DetachDatabase();
				}

				managers.Remove(name);
			}
			finally
			{
				managersLock.ExitReadLock();
			}
		}
	}
}
