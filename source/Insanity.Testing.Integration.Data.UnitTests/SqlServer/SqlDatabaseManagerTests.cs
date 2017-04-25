using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Insanity.Testing.Integration.Data.SqlServer;
using System.Configuration;
using System.Reflection;

namespace Insanity.Testing.Integration.Data.UnitTests.SqlServer
{
    [TestClass]
    public class SqlDatabaseManagerTests
    {
        [TestMethod]
        public void SingleDacpack_DeploingToLocalDb_DatabaseCreatedAndDeployed()
        {
            const string managerName = "Test";
            const string connectionStringName = "LocalDb_TestDatabase";

            DatabaseManagers.SetDataDirectory();

            string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings[connectionStringName]].ConnectionString;
            string dacpacFile = @"..\..\..\Insanity.Testing.Integration.Database\bin\Debug\Insanity.Testing.Integration.Database.dacpac";


            try
            {
                SqlDatabase.Create(managerName, connectionString, null, dacpacFile);

                //TODO: Verify the contents of the database.
            }
            finally
            {
                SqlDatabase.Delete(managerName);
            }
        }

        [TestMethod]
        public void SingleDacpack_DeploingToLocalDb_DatabaseCreatedDeployedAndSeeded()
        {
            const string managerName = "Test";
            const string connectionStringName = "LocalDb_TestDatabase";

            DatabaseManagers.SetDataDirectory();

            string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings[connectionStringName]].ConnectionString;
            string dacpacFile = @"..\..\..\Insanity.Testing.Integration.Database\bin\Debug\Insanity.Testing.Integration.Database.dacpac";

            try
            {
                SqlDatabase.Create(managerName, connectionString, database =>
                {
                    database.ExecuteNonQuery(command =>
                    {
                        command.CommandText = GetResourceString("Insanity.Testing.Integration.Data.UnitTests.SqlServer.Seed.sql");
                        command.CommandType = System.Data.CommandType.Text;
                    });
                }, dacpacFile);

                //TODO: Verify the contents of the database.

            }
            finally
            {
                SqlDatabase.Delete(managerName);
            }
        }

        [TestMethod]
        public void SingleDacpack_DeploingToLocalDb_AttachDb_DatabaseCreatedAndDeployed()
        {
            const string managerName = "Test";
            const string connectionStringName = "LocalDb_Attach_TestDatabase";

            DatabaseManagers.SetDataDirectory();

            string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings[connectionStringName]].ConnectionString;
            string dacpacFile = @"..\..\..\Insanity.Testing.Integration.Database\bin\Debug\Insanity.Testing.Integration.Database.dacpac";


            try
            {
                SqlDatabase.Create(managerName, connectionString, null, dacpacFile);

                //TODO: Verify the contents of the database.
            }
            finally
            {
                SqlDatabase.Delete(managerName);
            }
        }

        //TODO: Figure out why attachdb is not working correctly.
        [TestMethod]
        [Ignore]
        public void SingleDacpack_DeploingToLocalDb_AttachDb_DatabaseCreatedDeployedAndSeeded()
        {
            const string managerName = "Test";
            const string connectionStringName = "LocalDb_Attach_TestDatabase";

            DatabaseManagers.SetDataDirectory();

            string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings[connectionStringName]].ConnectionString;
            string dacpacFile = @"..\..\..\Insanity.Testing.Integration.Database\bin\Debug\Insanity.Testing.Integration.Database.dacpac";

            try
            {
                SqlDatabase.Create(managerName, connectionString, database =>
                {
                    database.ExecuteNonQuery(command =>
                    {
                        command.CommandText = GetResourceString("Insanity.Testing.Integration.Data.UnitTests.SqlServer.Seed.sql");
                        command.CommandType = System.Data.CommandType.Text;
                    });
                }, dacpacFile);

                //TODO: Verify the contents of the database.

            }
            finally
            {
                SqlDatabase.Delete(managerName);
            }
        }

        static string GetResourceString(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                using (var streamReader = new System.IO.StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}