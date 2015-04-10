using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;
using System.Globalization;

namespace Insanity.Testing.Integration.Data.SqlServer
{
	public class SqlDatabaseManager : IDatabaseManager
	{
		#region Properties
		public string ConnectionString { get { return ((DatabaseImpl)Database).ConnectionString; } }
		public IDatabase Database { get; private set; } 
		#endregion

		#region Constructor
		public SqlDatabaseManager(string connectionString)
		{
			Database = new DatabaseImpl(connectionString);
		} 
		#endregion

		#region Private Types
		private class DatabaseImpl : IDatabase
		{
			//TODO: This is really getting to be a mess need to see if it can be cleaned up.
			#region Public Properties
			public string ConnectionString { get; private set; }
			#endregion

			#region Constructors
			public DatabaseImpl(string connectionString)
			{
				var connectionStringBuilder = new DbConnectionStringBuilder();
				connectionStringBuilder.ConnectionString = connectionString;

				this.ConnectionString = connectionString;
			} 
			#endregion

			#region IDatabase Implementation
			public void Create(params string[] dacpacFiles)
			{
				if (dacpacFiles == null) throw new ArgumentNullException("dacpacFiles");

				DeleteDatabase();

				var connectionStringBuilder = new SqlConnectionStringBuilder();
				connectionStringBuilder.ConnectionString = ConnectionString;

				if(!String.IsNullOrWhiteSpace(connectionStringBuilder.AttachDBFilename))
				{
					CreateNewAttachedDbFileDatabase(dacpacFiles, connectionStringBuilder);
				}
				else
				{
					CreateNewDatabase(dacpacFiles, connectionStringBuilder);	
				}
			}
			
			public void Update(params string[] dacpacFiles)
			{
				if (dacpacFiles == null) throw new ArgumentNullException("dacpacFiles");

				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					var dacServices = new DacServices(GetDacFriendlyConnectionString(ConnectionString));
					dacServices.Message += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, args.Message);
					dacServices.ProgressChanged += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, String.Format(CultureInfo.InvariantCulture, "[{0}] {1} - {2}", args.OperationId, args.Status, args.Message));

					foreach (var dacpacFile in dacpacFiles)
					{
						var options = new DacDeployOptions();
						options.DropObjectsNotInSource = false;

						ApplyDacPackage(dacpacFile, connection.Database, dacServices, options);
					}
				}
			}

			public void DetachDatabase()
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);

				var serverName = connectionStringBuilder.DataSource;
				var databaseName = connectionStringBuilder.InitialCatalog;

				var server = new Server(serverName);
				server.KillAllProcesses(databaseName);
				var database = server.Databases[databaseName];
				if (database != null)
				{
					database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
					database.Alter(TerminationClause.RollbackTransactionsImmediately);
					server.DetachDatabase(databaseName, true);
				}
			}

			public void DeleteDatabase()
			{
				//TODO: Make sure the database is attached.
				var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);

				var serverName = connectionStringBuilder.DataSource;
				var databaseName = connectionStringBuilder.InitialCatalog;

				var server = new Server(serverName);

				if (server.Databases.Contains(databaseName))
				{
					server.KillAllProcesses(databaseName);
					var database = server.Databases[databaseName];
					if (database != null)
					{
						database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
						database.Alter(TerminationClause.RollbackTransactionsImmediately);
						server.KillDatabase(databaseName);
					}
				}
				else
				{
					File.Delete(GetDatabaseFileName(connectionStringBuilder.AttachDBFilename));
					File.Delete(GetDatabaseFileName(connectionStringBuilder.AttachDBFilename.Replace(".mdf", "_log.ldf")));
				}
			}

			public int ExecuteNonQuery(Action<DbCommand> prepare)
			{
				var rows = 0;
				ProcessCommand(
					command =>
					{
						prepare(command);
						rows = command.ExecuteNonQuery();
					});

				return rows;
			}

			public TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> process)
			{
				var result = default(TResult);

				ProcessCommand(command =>
				{
					prepare(command);
					using (var reader = command.ExecuteReader())
					{
						result = process(reader);
					}
				});

				return result;
			}

			public TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<DbConnection, IDataReader, TResult> process)
			{
				var result = default(TResult);

				ProcessCommand((connection, command) =>
				{
					prepare(command);
					using (var reader = command.ExecuteReader())
					{
						result = process(connection, reader);
					}
				});

				return result;
			}

			public IEnumerable<TResult> GetResults<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> process)
			{
				return ExecuteReader(prepare, reader => reader.ToList(process));
			}

			public Dictionary<TKey, TValue> GetResults<TKey, TValue>(Action<DbCommand> prepare, Func<IDataReader, TValue> process, Func<TValue, TKey> keySelector)
			{
				return ExecuteReader(prepare, reader => reader.ToDictionary(process, keySelector));
			}

			public TResult GetSingleResult<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> process)
			{
				return ExecuteReader(prepare, reader => reader.Read() ? process(reader) : default(TResult));
			}
			#endregion

			#region Private Methods
			private void ProcessCommand(Action<DbCommand> process)
			{
				ProcessCommand((connection, command) => process(command));
			}

			private void ProcessCommand(Action<DbConnection, DbCommand> process)
			{
				if (process == null)
				{
					throw new ArgumentNullException("process");
				}

				ProcessConnection(connection =>
				{
					using (var command = connection.CreateCommand())
					{
						process(connection, command);
					}
				});
			}

			private void ProcessConnection(Action<SqlConnection> process)
			{
				if (process == null)
				{
					throw new ArgumentNullException("process");
				}

				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					process(connection);
				}
			}

			private static string GetDatabaseFileName(string filename)
			{
				if (filename.Contains("|DataDirectory|"))
				{
					filename = filename.Replace("|DataDirectory|", AppDomain.CurrentDomain.BaseDirectory);
				}

				return filename;
			}

			private void CreateNewDatabase(string[] dacpacFiles, SqlConnectionStringBuilder connectionStringBuilder)
			{
				var created = false;
				ApplyDacPackageFiles(dacpacFiles, connectionStringBuilder, () =>
				{
					var options = new DacDeployOptions();
					options.DropObjectsNotInSource = false;

					if (created == false)
					{
						options.CreateNewDatabase = true;
						created = false;
					}

					return options;
				});
			}

			private void CreateNewAttachedDbFileDatabase(string[] dacpacFiles, SqlConnectionStringBuilder connectionStringBuilder)
			{
				try
				{
					CreateDatabaseFileForAttachDbFile(connectionStringBuilder);
					ApplyDacPackageFiles(dacpacFiles, connectionStringBuilder, () =>
					{
						var options = new DacDeployOptions();
						options.DropObjectsNotInSource = false;
						return options;
					});
				}
				finally
				{
					DetachDatabase();
				}
			}

			private void ApplyDacPackageFiles(string[] dacpacFiles, SqlConnectionStringBuilder connectionStringBuilder, Func<DacDeployOptions> options)
			{
				var dacServices = new DacServices(GetDacFriendlyConnectionString(ConnectionString));
				dacServices.Message += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, args.Message);
				dacServices.ProgressChanged += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, String.Format(CultureInfo.InvariantCulture, "[{0}] {1} - {2}", args.OperationId, args.Status, args.Message));

				foreach (var dacpacFile in dacpacFiles)
				{
					ApplyDacPackage(dacpacFile, connectionStringBuilder.InitialCatalog, dacServices, options());
				}
			}

			private static void ApplyDacPackage(string dacpacFile, string initialCatalog, DacServices dacServices, DacDeployOptions options)
			{
				var package = DacPackage.Load(dacpacFile);
				CancellationToken? cancellationToken = new CancellationToken();

				dacServices.Deploy(package, initialCatalog, true, options, cancellationToken);
			}

			private void CreateDatabaseFileForAttachDbFile(SqlConnectionStringBuilder connectionStringBuilder)
			{
				var serverName = connectionStringBuilder.DataSource;
				var databaseName = connectionStringBuilder.InitialCatalog;
				var fileName = connectionStringBuilder.AttachDBFilename;

				var server = new Server(serverName);
				Database database = new Database(server, databaseName);

				database.FileGroups.Add(new FileGroup(database, "PRIMARY"));

				DataFile dataFile = new DataFile(database.FileGroups["PRIMARY"], databaseName, GetDatabaseFileName(fileName));
				LogFile logFile = new LogFile(database, String.Format(CultureInfo.InvariantCulture, "{0}_log", databaseName), GetDatabaseFileName(fileName.Replace(".mdf", "_log.ldf")));

				database.FileGroups["PRIMARY"].Files.Add(dataFile);
				database.LogFiles.Add(logFile);

				database.Create();

				GrantFileAccessForAttach();
			}

			private void GrantFileAccessForAttach()
			{
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
				builder.ConnectionString = ConnectionString;

				var databaseFilename = GetDatabaseFileName(builder.AttachDBFilename);

				GrantFileAccess(databaseFilename);
				GrantFileAccess(databaseFilename.Replace(".mdf", "_log.ldf"));
			}

			private static void GrantFileAccess(string filename)
			{
				var fs = new FileSecurity();
				fs.AddAccessRule(new FileSystemAccessRule(Thread.CurrentPrincipal.Identity.Name, FileSystemRights.FullControl, AccessControlType.Allow));

				File.SetAccessControl(filename, fs);
			}

			private static string GetDacFriendlyConnectionString(string connectionString)
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString) { AttachDBFilename = String.Empty, Pooling = false };
				return connectionStringBuilder.ConnectionString;
			}
			#endregion
		}
		#endregion
	}
}
