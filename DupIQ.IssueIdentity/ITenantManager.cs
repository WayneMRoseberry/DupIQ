namespace DupIQ.IssueIdentity
{
	/// <summary>
	/// Provides platform specific access to tenant information.
	/// </summary>
	public interface ITenantManager
	{
		public void AddProjectForUser(string tenantId, string userId, string projectID);

		public void AddOrUpdateProjectExtendedProperties<T>(string tenantId, string projectId, T properties);

		public void AddProject(string tenantId, Project project);

		public string AddTenant(TenantProfile tenantProfile);

		public void AddTenantProfileToUserTenantList(string userId, string tenantId, UserTenantAuthorization authorization);

		public void AddUserTenantAuthorization(string tenantId, string userId, UserTenantAuthorization authorization);

		public string DefaultTenant();

		public void DeleteTenant(string tenantId);

		public string GenerateApiKey(string tenantId);

		public string GenerateApiKey(string tenantId, string projectId);

		public Project GetProject(string tenantId, string projectId);

		public T GetProjectExtendedProperties<T>(string projectId);

		public string[] GetProjectExtendedPropertyTypeNames(string tenentId, string projectId);

		public string[] GetProjectExtendedPropertyTypeNames(string projectId);

		public Project[] GetProjects(string tenantId);

		public Project[] GetProjects(string tenantId, string userId);

		/// <summary>
		/// Returns the configuration specific to the given tenant.
		/// </summary>
		/// <param name="tenantId">Id of the specified tenant.</param>
		/// <returns>A TenantConfiguration object describing the platform specific configuration settings for that tenant.</returns>
		public TenantConfiguration GetTenantConfiguration(string tenantId);

		TenantProfile GetTenantProfile(string tenantId);

		public string[] GetTenants();

		/// <summary>
		/// Gets the tenants that are available for the specified user.
		/// </summary>
		/// <param name="identityUser">IdentityUser object of the user whose tenants to return.</param>
		/// <returns>Array of TenantProfile objects</returns>
		public TenantProfile[] GetTenants(string userId);

		public UserTenantAuthorization GetUserTenantAuthorization(string tenantId, string userId);

		public void UpdateTenantConfiguration(TenantConfiguration tenantConfiguration);

		public void UpdateTenantProfile(TenantProfile tenantProfile);

		public void UpdateProject(string tenantId, Project project);

		T GetProjectExtendedProperties<T>(string tenantId, string projectId);
		void AddTenantProfileToUserIdentityTenantList(string tenantId, string userId, string userName, string email, UserTenantAuthorization auth);
	}

	public enum TenantStatus
	{
		Active,
		Inactive,
		Deleted
	}

	public enum UserTenantAuthorization
	{
		Admin,
		Developer,
		Writer,
		Reader,
		None
	}

	public class ProjectDoesNotExistException : Exception
	{
		public string ProjectId { get; set; }
		public Exception InnerException { get; set; }

		public ProjectDoesNotExistException(string projectId,  Exception innerException)
		{
			this.ProjectId = projectId;
			this.InnerException = innerException;
		}
	}
}
