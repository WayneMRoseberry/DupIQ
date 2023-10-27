namespace DupIQ.IssueIdentity.Tests
{
	public class MockTenantManager : ITenantManager
	{
		public Func<string, TenantConfiguration> _overrideGetTenantConfiguration = (t) => { throw new NotImplementedException(); };
		public Func<string> _overrideDefaultTenant = () => { throw new NotImplementedException(); };
		public Func<string, TenantProfile> _overrideGetTenantProfile = (s) => { throw new NotImplementedException(); };
		public Func<string, string, Project> _overrideGetProject = (t, p) => { throw new NotImplementedException(); };


		public TenantConfiguration GetTenantConfiguration(string tenantId)
		{
			return _overrideGetTenantConfiguration(tenantId);
		}

		public TenantProfile GetTenantProfile(string tenantId)
		{
			return _overrideGetTenantProfile(tenantId);
		}

		public string[] GetTenants()
		{
			throw new NotImplementedException();
		}

		public UserTenantAuthorization GetUserTenantAuthorization(string tenantId, string userId)
		{
			throw new NotImplementedException();
		}

		public void UpdateTenantConfiguration(string tenantId, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public void UpdateTenantConfiguration(TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public void UpdateTenantProfile(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public void AddTenantProfileToUserTenantList(string userId, TenantProfile tenantProfile, UserTenantAuthorization authorization)
		{
			throw new NotImplementedException();
		}

		public void AddProjectForUser(string tenantId, string userId, string projectID)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdateProjectExtendedProperties<T>(string projectId, T properties)
		{
			throw new NotImplementedException();
		}

		public void AddProject(string tenantId, Project project)
		{
			throw new NotImplementedException();
		}

		public void AddTenantProfileToUserTenantList(string userId, string tenantId, UserTenantAuthorization authorization)
		{
			throw new NotImplementedException();
		}

		public void DeleteTenant(string tenantId)
		{
			throw new NotImplementedException();
		}

		public string GenerateApiKey(string tenantId)
		{
			throw new NotImplementedException();
		}

		public string GenerateApiKey(string tenantId, string projectId)
		{
			throw new NotImplementedException();
		}

		public Project GetProject(string tenantId, string projectId)
		{
			return _overrideGetProject(tenantId, projectId);
		}

		public T GetProjectExtendedProperties<T>(string projectId)
		{
			throw new NotImplementedException();
		}

		public string[] GetProjectExtendedPropertyTypeNames(string projectId)
		{
			throw new NotImplementedException();
		}

		public Project[] GetProjects(string tenantId)
		{
			throw new NotImplementedException();
		}

		public Project[] GetProjects(string tenantId, string userId)
		{
			throw new NotImplementedException();
		}

		public TenantProfile[] GetTenants(string userId)
		{
			throw new NotImplementedException();
		}

		public void UpdateProject(string tenantId, Project project)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdateProjectExtendedProperties<T>(string tenantId, string projectId, T properties)
		{
			throw new NotImplementedException();
		}

		public T GetProjectExtendedProperties<T>(string tenantId, string projectId)
		{
			throw new NotImplementedException();
		}

		public void AddProjectForUser(TenantProfile tenantProfile, string userId, string projectID)
		{
			throw new NotImplementedException();
		}

		public void AddOrUpdateProjectExtendedProperties<T>(Project project, T properties)
		{
			throw new NotImplementedException();
		}

		public void AddProject(TenantProfile tenantProfile, Project project)
		{
			throw new NotImplementedException();
		}

		public string AddTenant(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		//		public void AddTenantProfileToUserIdentityTenantList(IdentityUser identityUser, TenantProfile tenantProfile)
		//		{
		//			throw new NotImplementedException();
		//		}

		public void AddUserTenantAuthorization(string tenantId, string userId, UserTenantAuthorization authorization)
		{
			throw new NotImplementedException();
		}

		public string DefaultTenant()
		{
			throw new NotImplementedException();
		}

		public void DeleteTenant(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public string GenerateApiKey(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public string GenerateApiKey(TenantProfile tenantProfile, string projectId)
		{
			throw new NotImplementedException();
		}

		public Project GetProject(TenantProfile tenantProfile, string projectId)
		{
			throw new NotImplementedException();
		}

		public T GetProjectExtendedProperties<T>(Project project)
		{
			throw new NotImplementedException();
		}

		public string[] GetProjectExtendedPropertyTypeNames(Project project)
		{
			throw new NotImplementedException();
		}

		public Project[] GetProjects(TenantProfile tenantProfile)
		{
			throw new NotImplementedException();
		}

		public Project[] GetProjects(TenantProfile tenantProfile, string userId)
		{
			throw new NotImplementedException();
		}

		//		public TenantProfile[] GetTenants(IdentityUser identityUser)
		//		{
		//			throw new NotImplementedException();
		//		}

		public void UpdateProject(TenantProfile tenantProfile, Project project)
		{
			throw new NotImplementedException();
		}

		public string[] GetProjectExtendedPropertyTypeNames(string tenentId, string projectId)
		{
			throw new NotImplementedException();
		}

		public void AddTenantProfileToUserIdentityTenantList(string tenantId, string userId, string userName, string email, UserTenantAuthorization auth)
		{
			throw new NotImplementedException();
		}
	}

}
