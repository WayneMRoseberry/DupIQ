namespace DupIQ.IssueIdentity.Providers
{
	public class EmptyTenantManager : ITenantManager
	{


		public string DefaultTenant()
		{
			return "DEFAULTTENANT";
		}


		public TenantConfiguration GetTenantConfiguration(string tenantId)
		{
			return new TenantConfiguration() { TenantId = tenantId };
		}

		public TenantProfile GetTenantProfile(string tenantId)
		{
			throw new NotImplementedException();
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

		public void AddOrUpdateProjectExtendedProperties<T>(string tenantId, string projectId, T properties)
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
			throw new NotImplementedException();
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

		public void AddUserTenantAuthorization(string tenantId, string userId, UserTenantAuthorization authorization)
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
