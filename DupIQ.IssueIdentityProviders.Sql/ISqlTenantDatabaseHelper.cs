using DupIQ.IssueIdentity;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public interface ISqlTenantDatabaseHelper
	{
		public string ConnectionString { get; set; }
		void AddOrUpdateProject(string tenantId, Project project);
		void AddOrUpdateProjectForUser(string tenantId, string projectId, string userId);
		public void AddOrUpdateProjectExtendedProperties(string tenantId, string projectId, PropStuffer properties);
		public void AddOrUpdateTenantConfiguration(TenantConfiguration tenantConfiguration);
		public void AddOrUpdateTenantProfile(TenantProfile tenantProfile);
		public void AddOrUpdateTenantProfileToUserProfileList(string tenantId, string userId, string userName, string email, UserTenantAuthorization auth);
		public void AddOrUpdateUserServiceAuthorization(string userId, UserServiceAuthorization authorization);
		public void AddOrUpdateUserTenantAuthorizaation(string tenantId, string userId, UserTenantAuthorization authorization);
		public void ConfigureTenantDatabase();
		void DeleteProjects(string tenantId);

		void DeleteTenantConfiguration(string tenantId);
		void DeleteTenantProfile(string tenantId);
		Project GetProject(string tenantId, string projectId);

		public PropStuffer GetProjectExtendedProperties(string tenantId, string projectId);
		DbDataReader GetProjects(string tenantId);
		DbDataReader GetProjects(string tenantId, string userId);

		public DbDataReader GetTenantConfiguration(string tenantId);

		public DbDataReader GetTenantProfile(string tenantId);

		public DbDataReader GetTenants();

		public DbDataReader GetTenants(string userId);

		public DbDataReader GetUserServiceAuthorization(string userId);

		DbDataReader GetUserTenantAuthorization(string tenantId, string userId);

		public void PurgeTenants(bool purgeDependencies);
	}
}
