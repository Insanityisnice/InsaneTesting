using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Insanity.Testing.Integration.Data
{
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

		public void SetDataDirectory(string dataDirectory = "")
		{
			if (String.IsNullOrWhiteSpace(dataDirectory))
			{
				dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
			}
			AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
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
