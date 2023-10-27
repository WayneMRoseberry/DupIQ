namespace DupIQ.IssueIdentity
{
	public interface IIssueDbProvider
	{
		/// <summary>
		/// Allows configuration of provider.
		/// </summary>
		/// <param name="configJson">provider specific configuration string</param>
		void Configure(string configJson);

		/// <summary>
		/// Add issue report to issue database.
		/// </summary>
		/// <param name="issueReport">Issue report to be stored.</param>
		void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId);
		/// <summary>
		/// Add issue profile to the issue database.
		/// </summary>
		/// <param name="issueProfile">Issue profile to be stored.</param>
		void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId);

		void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId);
		void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Returns specified issue report.
		/// </summary>
		/// <param name="instanceId">Corresponds to the instance id property on the issue report desired</param>
		/// <returns>Matching IssueReport.</returns>
		IssueReport GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId);
		/// <summary>
		/// Returns issue reports from issue database which match the issue profile specified.
		/// </summary>
		/// <param name="issueProfile">Issue profile to match for.</param>
		/// <returns>Array of IssueReports where IssueId matches id on specified IssueProfile.</returns>
		IssueReport[] GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId);
		/// <summary>
		/// Returns specified issue profile.
		/// </summary>
		/// <param name="issueId">Id of issue to return.</param>
		/// <returns>Matching IssueProfile.</returns>
		IssueProfile GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId);
		/// <summary>
		/// Returns all the issue profiles in the issue database.
		/// </summary>
		/// <returns>Array of IssueProfile objects.</returns>
		IssueProfile[] GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId);
		/// <summary>
		/// Finds specified issue report and changes its issue id to match value provided.
		/// </summary>
		/// <param name="instanceId">Instance of IssueReport to update.</param>
		/// <param name="issueId">Id of issue to change IssueId on issue report to.</param>
		void UpdateIssueReportIssueId(string instanceId, string issueId, TenantConfiguration tenantConfiguration, string projectId);
	}
}
