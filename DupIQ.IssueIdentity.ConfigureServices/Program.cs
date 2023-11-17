// See https://aka.ms/new-console-template for more information
using DupIQ.IssueIdentity.ConfigureServices;
using DupIQ.IssueIdentityProviders.Sql;
using Microsoft.Extensions.Configuration;

AppArguments appArguments = new AppArguments(Environment.GetCommandLineArgs());	

Console.WriteLine($"{System.AppDomain.CurrentDomain.FriendlyName}");

var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
var thing = builder.Build();
var SqlConfig = thing.GetSection("SqlIOHelperConfig").Get<SqlIOHelperConfig>();
SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(SqlConfig);
SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);

switch (appArguments.Command)
{
	case "database":
		{

			SqlIssueDbIOHelper sqlIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlTenantDatabaseHelper.ConfigureTenantDatabase();
			sqlIOHelper.ConfigureDatabaseServer();
			sqlUserDbHelper.ConfigureDatabase();

			SqlIssueIdentityUserManager sqlIssueIdentityUserManager = new SqlIssueIdentityUserManager(sqlUserDbHelper);
			SqlTenantManager tenantManager = new SqlTenantManager(sqlTenantDatabaseHelper);
			break;
		}
	case "serviceadmin":
		{
			SqlIssueIdentityUserManager sqlIssueIdentityUserManager = new SqlIssueIdentityUserManager(sqlUserDbHelper);
			SqlTenantManager tenantManager = new SqlTenantManager(sqlTenantDatabaseHelper);
			var user = new DupIQ.IssueIdentity.IssueIdentityUser() 
			{ 
				Name=appArguments.ServiceAdminName, FirstName=appArguments.ServiceAdminFirstName, LastName=appArguments.ServiceAdminLastName, Email=appArguments.ServiceAdminEmail, Userstatus= DupIQ.IssueIdentity.IssueIdentityUserStatus.Active
			};
			sqlIssueIdentityUserManager.AddOrUpdateUser(user);
			tenantManager.AddOrUpdateUserServiceAuthorization(user.Id, DupIQ.IssueIdentity.UserServiceAuthorization.Admin);
			break;
		}
}