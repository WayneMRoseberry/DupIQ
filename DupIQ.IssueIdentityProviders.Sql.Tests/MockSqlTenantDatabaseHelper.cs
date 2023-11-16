using DupIQ.IssueIdentity;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	public class MockSqlTenantDatabaseHelper : ISqlTenantDatabaseHelper
	{
		public Action<string, Project> overrideAddOrUpdateProject = (t, p) => { throw new NotImplementedException(); };
		public Action<string, string, PropStuffer> overrideAddOrUpdateProjectExtendedProperties = (s1, s2, p) => { throw new NotImplementedException(); };
		public Action<TenantProfile, string, string> overrideAddProjectForUser_tenantProfile = (t, u, p) => { throw new NotImplementedException(); };
		public Action<string, string, string> overrideAddProjectForUser = (t, u, p) => { throw new NotImplementedException(); };
		public Action<TenantProfile> overrideAddOrUpdateTenantProfile = (t) => { throw new NotImplementedException(); };
		public Action<string, string, string, string, UserTenantAuthorization> overrideAddOrUpdateTenantProfileToUserProfileList = (t, u, n, e, a) => { throw new NotImplementedException(); };
		public Action<string, UserServiceAuthorization> overrideAddOrUpdateUserServiceAuthorization = (s, u) => { throw new NotImplementedException(); };
		public Action<string, string, UserTenantAuthorization> overrideAddOrUpdateUserTenantAuthorization = (t, u, a) => { throw new NotImplementedException(); };
		public Action<string> overrideDeleteTenantProfile = (t) => { throw new NotImplementedException(); };
		public Func<string, string, Project> overrideGetProject = (p, t) => { throw new NotImplementedException(); };
		public Func<string, string, PropStuffer> overrideGetProjectExtendedProperties = (t, p) => { throw new NotImplementedException(); };
		public Func<string, DbDataReader> overrideGetProjects = (t) => { throw new NotImplementedException(); };
		public Func<string, string, DbDataReader> overrideGetProjects_UserId = (t, u) => { throw new NotImplementedException(); };
		public Func<string, DbDataReader> overrideGetTenantConfiguration = (s) => { throw new NotImplementedException(); };
		public Func<string, DbDataReader> overrideGetTenantProfile = (s) => { throw new NotImplementedException(); };
		public Func<DbDataReader> overrideGetTenants = () => { throw new NotImplementedException(); };
		public Func<string, DbDataReader> overrideGetTenants_UserId = (u) => { throw new NotImplementedException(); };
		public Func<string, string, DbDataReader> overrideGetUserTenantAuthorization = (t, u) => { throw new NotImplementedException(); };

		public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void AddOrUpdateProject(TenantProfile tenantProfile, Project project)
		{
			throw new NotImplementedException(); ;
		}

		public void AddOrUpdateProject(string tenantId, Project project)
		{
			overrideAddOrUpdateProject(tenantId, project);
		}

		public void AddOrUpdateProjectExtendedProperties(string tenantId, string projectId, PropStuffer properties)
		{
			overrideAddOrUpdateProjectExtendedProperties(tenantId, projectId, properties);
		}

		public void AddOrUpdateProjectForUser(TenantProfile tenantProfile, string userId, string projectId)
		{
			overrideAddProjectForUser_tenantProfile(tenantProfile, userId, projectId);
		}

		public void AddOrUpdateProjectForUser(string tenantId, string projectId, string userId)
		{
			overrideAddProjectForUser(userId, projectId, tenantId);
		}

		public void AddOrUpdateTenantConfiguration(TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdateTenantProfile(TenantProfile tenantProfile)
		{
			overrideAddOrUpdateTenantProfile(tenantProfile);
		}

		public void AddOrUpdateTenantProfileToUserProfileList(TenantProfile tenantProfile, string userId)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdateTenantProfileToUserProfileList(string tenantId, string userId, string userName, string email, UserTenantAuthorization auth)
		{
			overrideAddOrUpdateTenantProfileToUserProfileList(tenantId, userId, userName, email, auth);
		}

		public void AddOrUpdateUserServiceAuthorization(string userId, UserServiceAuthorization authorization)
		{
			overrideAddOrUpdateUserServiceAuthorization(userId, authorization);
		}

		public void AddOrUpdateUserTenantAuthorizaation(string tenantId, string userId, UserTenantAuthorization authorization)
		{
			overrideAddOrUpdateUserTenantAuthorization(tenantId, userId, authorization);
		}

		public void ConfigureTenantDatabase()
		{
			throw new NotImplementedException();
		}

		public void DeleteProjects(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public void DeleteProjects(string tenantId)
		{
			throw new NotImplementedException();
		}

		public void DeleteTenantConfiguration(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public void DeleteTenantConfiguration(string tenantId)
		{
			throw new NotImplementedException();
		}

		public void DeleteTenantProfile(string tenantId)
		{
			overrideDeleteTenantProfile(tenantId);
		}

		public void DeleteTenantProfile(TenantProfile tenant)
		{
			throw new NotFiniteNumberException(); ;
		}

		public Project GetProject(TenantProfile tenant, string projectId)
		{
			throw new NotImplementedException();
		}

		public Project GetProject(string tenantId, string projectId)
		{
			return overrideGetProject(tenantId, projectId);
		}

		public PropStuffer GetProjectExtendedProperties(string tenantId, string projectId)
		{
			return overrideGetProjectExtendedProperties(tenantId, projectId);
		}

		public DbDataReader GetProjects(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public DbDataReader GetProjects(TenantProfile tenantProfile, string user)
		{
			throw new NotImplementedException();
		}

		public DbDataReader GetProjects(string tenantId)
		{
			return overrideGetProjects(tenantId);
		}

		public DbDataReader GetProjects(string tenantId, string userId)
		{
			return overrideGetProjects_UserId(tenantId, userId);
		}

		public DbDataReader GetTenantConfiguration(string tenantId)
		{
			return overrideGetTenantConfiguration(tenantId);
		}

		public DbDataReader GetTenantProfile(string tenantId)
		{
			return overrideGetTenantProfile(tenantId);
		}

		public DbDataReader GetTenants()
		{
			return overrideGetTenants();
		}

		public DbDataReader GetTenants(string userId)
		{
			return overrideGetTenants_UserId(userId);
		}

		public DbDataReader GetUserServiceAuthorization(string userId)
		{
			throw new NotImplementedException();
		}

		public DbDataReader GetUserTenantAuthorization(string tenantId, string userId)
		{
			return overrideGetUserTenantAuthorization(tenantId, userId);
		}

		public void PurgeTenants(bool purgeDependencies)
		{
			throw new NotImplementedException();
		}
	}

}