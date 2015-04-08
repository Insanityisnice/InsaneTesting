# InsaneTesting
This is a library for integration testing from service layer to database.

# Supported Databases
Currently SQL Server 2014 or older is the only supported database.

# Supported Hosts
Currently Owin is the only supported API host.

# Quick Start
To setup a database use the ```SqlDatabase``` classes ```Setup``` method.

```C#
const string managerName = "Test";
string connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["TestDatabase"]].ConnectionString;
string dacpacFile = @"..\..\..\Insanity.Testing.Integration.Database\bin\Debug\Insanity.Testing.Integration.Database.dacpac";

SqlDatabase.Setup(managerName, connectionString, null, dacpacFile);

```
