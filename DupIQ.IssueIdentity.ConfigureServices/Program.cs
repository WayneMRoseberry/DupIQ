// See https://aka.ms/new-console-template for more information
using DupIQ.IssueIdentityProviders.Sql;
using Microsoft.Extensions.Configuration;

Console.WriteLine($"{System.AppDomain.CurrentDomain.FriendlyName}");

var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
var thing = builder.Build();
var SqlConfig = thing.GetSection("SqlIOHelperConfig").Get<SqlIOHelperConfig>();

SqlIssueDbIOHelper sqlIOHelper = new SqlIssueDbIOHelper(SqlConfig);
SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
sqlTenantDatabaseHelper.ConfigureTenantDatabase();
sqlIOHelper.ConfigureDatabaseServer();
SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(SqlConfig);
sqlUserDbHelper.ConfigureDatabase();