namespace DupIQ.IssueIdentity
{
	public interface IIssueIdentityProvider
	{
		string ID();

		/// <summary>
		/// Returns true if the provider is able to handle the specified issue.
		/// </summary>
		/// <param name="issueMessage">Issue message to evaluate.</param>
		/// <returns></returns>
		bool CanHandleIssue(string issueMessage);

		/// <summary>
		/// Allows configuration of the provider.
		/// </summary>
		/// <param name="configJson">Provider specific configuration.</param>
		void Configure(string configJson);

		/// <summary>
		/// Returns an issue profile for the matching issue id.
		/// </summary>
		/// <param name="id">Issue id to get profile for.</param>
		/// <returns></returns>
		IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Returns issue profiles related to the issue message
		/// </summary>
		/// <param name="issueMessage">The message to find matches for.</param>
		/// <param name="count">Maximum number of related profiles to return.</param>
		/// <returns></returns>
		RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Add the issue profile to the list of prior.
		/// </summary>
		/// <param name="issueProfile">Issue profile to add.</param>
		void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Delete the specified issue profile from storage.
		/// </summary>
		/// <param name="id">Id of the profile to delete.</param>
		/// <param name="tenantConfiguration">Tenant to delete profile from.</param>
		void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId);

		float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId);
	}
}
