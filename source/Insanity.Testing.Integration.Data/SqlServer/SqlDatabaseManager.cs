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
				var connectionStringBuilder = new SqlConnectionStringBuilder();
				connectionStringBuilder.ConnectionString = ConnectionString;

				var dacServices = new DacServices(GetDacFriendlyConnectionString(ConnectionString));
				dacServices.Message += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, args.Message);
				dacServices.ProgressChanged += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, String.Format("[{0}] {1} - {2}", args.OperationId, args.Status, args.Message));

				var created = false;
				foreach (var dacpacFile in dacpacFiles)
				{
					var options = new DacDeployOptions();
					options.DropObjectsNotInSource = false;

					if (created == false) options.CreateNewDatabase = true;

					ApplyDacPackage(dacpacFile, connectionStringBuilder.InitialCatalog, dacServices, options);
					created = true;
				}
			}
			
			public void Update(params string[] dacpacFiles)
			{
				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					var dacServices = new DacServices(GetDacFriendlyConnectionString(ConnectionString));
					dacServices.Message += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, args.Message);
					dacServices.ProgressChanged += (sender, args) => Debug.WriteLineIf(Debugger.IsAttached, String.Format("[{0}] {1} - {2}", args.OperationId, args.Status, args.Message));

					foreach (var dacpacFile in dacpacFiles)
					{
						var options = new DacDeployOptions();
						options.DropObjectsNotInSource = false;

						ApplyDacPackage(dacpacFile, connection.Database, dacServices, options);
					}
					connection.Close();
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

			public void KillDatabase()
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
					server.KillDatabase(databaseName);
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

			public TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> function)
			{
				var result = default(TResult);

				ProcessCommand(command =>
				{
					prepare(command);
					using (var reader = command.ExecuteReader())
					{
						result = function(reader);
					}
				});

				return result;
			}

			public TResult ExecuteReader<TResult>(Action<DbCommand> prepare, Func<DbConnection, IDataReader, TResult> function)
			{
				var result = default(TResult);

				ProcessCommand((connection, command) =>
				{
					prepare(command);
					using (var reader = command.ExecuteReader())
					{
						result = function(connection, reader);
					}
				});

				return result;
			}

			public IEnumerable<TResult> GetResults<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> function)
			{
				return ExecuteReader(prepare, reader => reader.ToList(function));
			}

			public Dictionary<TKey, TValue> GetResults<TKey, TValue>(Action<DbCommand> prepare, Func<IDataReader, TValue> function, Func<TValue, TKey> keySelector)
			{
				return ExecuteReader(prepare, reader => reader.ToDictionary(function, keySelector));
			}

			public TResult GetSingleResult<TResult>(Action<DbCommand> prepare, Func<IDataReader, TResult> function)
			{
				return ExecuteReader(prepare, reader => reader.Read() ? function(reader) : default(TResult));
			}
			#endregion

			#region Private Methods
			private void ProcessCommand(Action<DbCommand> function)
			{
				ProcessCommand((connection, command) => function(command));
			}

			private void ProcessCommand(Action<DbConnection, DbCommand> function)
			{
				if (function == null)
				{
					throw new ArgumentNullException("function");
				}

				ProcessConnection(connection =>
				{
					using (var command = connection.CreateCommand())
					{
						function(connection, command);
					}
				});
			}

			private void ProcessConnection(Action<SqlConnection> function)
			{
				if (function == null)
				{
					throw new ArgumentNullException("function");
				}

				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					function(connection);
				}
			}

			private T ProcessConnection<T>(Func<SqlConnection, T> function)
			{
				if (function == null)
				{
					throw new ArgumentNullException("function");
				}

				using (var connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					return function(connection);
				}
			}

			private static string GetDatabaseFilename(string connectionString)
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
				string filename = String.Empty;
				if (connectionStringBuilder.AttachDBFilename.Contains("|DataDirectory|"))
				{
					filename = connectionStringBuilder.AttachDBFilename.Replace("|DataDirectory|", AppDomain.CurrentDomain.BaseDirectory);
				}
				else
				{
					filename = connectionStringBuilder.AttachDBFilename;
				}

				return filename;
			}

			private void ApplyDacPackage(string dacpacFile, string initialCatalog, DacServices dacServices, DacDeployOptions options)
			{
				var package = DacPackage.Load(dacpacFile);
				CancellationToken? cancellationToken = new CancellationToken();

				dacServices.Deploy(package, initialCatalog, true, options, cancellationToken);
			}

			private void GrantFileAccessForAttach()
			{
				var databaseFilename = GetDatabaseFilename(ConnectionString);

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
