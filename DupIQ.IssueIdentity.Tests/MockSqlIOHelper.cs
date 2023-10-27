using System.Data.Common;

namespace DupIQ.IssueIdentity.Tests
{
	public class MockSqlIOHelper : IIssueDbIOHelper
	{
		public Func<string, TenantConfiguration, string, DbDataReader> overrideGetIssueProfile = (s, t, s1) => { throw new NotImplementedException(); };
		public Func<TenantConfiguration, string, DbDataReader> overrideGetIssueProfiles = (t, s) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, DbDataReader> overrideGetIssueReport = (s, t, s1) => { throw new NotImplementedException(); };
		public Func<IssueProfile, TenantConfiguration, string, DbDataReader> overrideGetIssueReports = (issueReport, tenantConfiguration, projectid) => { throw new NotImplementedException(); };
		public Action<IssueProfile, TenantConfiguration, string> overrideAddIssueProfile = (i, t, p) => { throw new NotImplementedException(); };
		public Action<IssueReport, TenantConfiguration, string> overrideAddIssueReport = (i, t, p) => { throw new NotImplementedException(); };
		public Action<string, string> overrideUpdateIssueReportIssueId = (instanceId, issueId) => { throw new NotImplementedException(); };

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddIssueProfile(issueProfile, tenantConfiguration, projectId);
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddIssueReport(issueReport, tenantConfiguration, projectId);
		}

		public void DeleteIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}
		public DbDataReader GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueProfile(issueId, tenantConfiguration, projectId);
		}
		public DbDataReader GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueProfiles(tenantConfiguration, projectId);
		}

		public DbDataReader GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueReport(instanceId, tenantConfiguration, projectId);
		}

		public DbDataReader GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueReports(issueProfile, tenantConfiguration, projectId);
		}

		public void PurgeIssueProfiles(TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void PurgeIssueReports(TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void UpdateIssueReportIssueId(string instanceId, string IssueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}
	}
}