using System.Data.Common;

namespace DupIQ.IssueIdentity
{
	/// <summary>
	/// The whole point of this interface is so the caller does
	/// not need to place a dependency on any of the SQL specific methods,
	/// or namespaces. Allows mocked implementations to imitate the database
	/// behaviors and return values, moving the business logic of packing/unpacking
	/// database rows from/to business objects up into the caller.
	/// </summary>
	public interface IIssueDbIOHelper
	{
		/// <summary>
		/// Returns a database reader with the requested issue profile in result set.
		/// </summary>
		/// <param name="issueId">Id of issue requested.</param>
		/// <returns>DbDataReader that abstracts away actual database.</returns>
		public DbDataReader GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Returns a database reader with the requested issue report in result set.
		/// </summary>
		/// <param name="instanceId">Id of the instance of the issue report requested.</param>
		/// <returns>DbDataReader that abstracts away actual database.</returns>
		public DbDataReader GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Returns all issue profile objects from issue database.
		/// </summary>
		/// <returns>AbDataReader that abstracts away actual database.</returns>
		public DbDataReader GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Returns list of all issue reports which match the issue Profile id
		/// </summary>
		/// <param name="issueProfile">Issue profile to match on.</param>
		/// <returns>bDataReader that abstracts away actual database.</returns>
		public DbDataReader GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Add the specified IssueProfile to the issue database.
		/// </summary>
		/// <param name="issueProfile">Profile to add</param>
		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Add the specified IssueReport to the issue database.
		/// </summary>
		/// <param name="issueReport">Report to add.</param>
		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Delete the specified IssueProfile.
		/// </summary>
		/// <param name="issueId">Id of the profile to delete.</param>
		/// <param name="tenantConfiguration">Tenant to delete profile from.</param>
		public void DeleteIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Delete the specified IssueReport.
		/// </summary>
		/// <param name="instanceId">InstanceId of the IssueReport to delete.</param>
		/// <param name="tenantConfiguration">Tenant to delete the IssueReport from.</param>
		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Update the data base row issue id of the issue report indicated.
		/// </summary>
		/// <param name="instanceId">InstanceId that corresponds to the issue report to update.</param>
		/// <param name="IssueId">Issue id value to write to the row.</param>
		public void UpdateIssueReportIssueId(string instanceId, string IssueId, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Deletes all issue profiles from the database.
		/// </summary>
		public void PurgeIssueProfiles(TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Deletes all issue reports from the database.
		/// </summary>
		public void PurgeIssueReports(TenantConfiguration tenantConfiguration, string projectId);
	}
}
