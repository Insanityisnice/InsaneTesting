using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Insanity.Testing.Integration.Data.SqlServer;
using System.Configuration;

namespace Insanity.Testing.Integration.Data.UnitTests.SqlServer
{
	[TestClass]
	public class SqlDatabaseManagerTests
	{
		[TestMethod]
		public void SingleDacpack_DeploingToLocalDb_ValidDabaseDeployed()
		{
			const string managerName = "Test";

			// Setup
			string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["TestDatabase"]].ConnectionString;
			string dacpacFile = @"..\..\..\Insanity.Testing.Integration.Database\bin\Debug\Insanity.Testing.Integration.Database.dacpac";
			SqlDatabase.Setup(managerName, connectionString, null, dacpacFile);
			
			//Detach
			SqlDatabase.Detach(managerName);
		}
	}
}
