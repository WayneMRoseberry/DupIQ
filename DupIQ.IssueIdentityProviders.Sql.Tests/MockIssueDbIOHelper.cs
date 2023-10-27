using System.Data.Common;
using DupIQ.IssueIdentity;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	public class MockIssueDbIOHelper : IIssueDbIOHelper
	{
		public Action<IssueProfile, TenantConfiguration, string> overrideAddIssueProfile = (i, t, p) => { throw new NotImplementedException(); };
		public Action<IssueReport, TenantConfiguration, string> overrideAddIssueReport = (i, t, p) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration, string> overrideDeleteIssueProfile = (i, t, p) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration, string> overrideDeleteIssueReport = (i, t, p) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, DbDataReader> overrideGetIssueProfile = (i, t, p) => { throw new NotImplementedException(); };
		public Func<TenantConfiguration, string, DbDataReader> overrideGetIssueProfiles = (t, p) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, DbDataReader> overrideGetIssueReport = (i, t, p) => { throw new NotImplementedException(); };
		public Func<IssueProfile, TenantConfiguration, string, DbDataReader> overrideGetIssueReports = (i, t, p) => { throw new NotImplementedException(); };
		public Action<TenantConfiguration, string> overridePurgeIssueProfiles = (t, p) => { throw new NotImplementedException(); };
		public Action<TenantConfiguration, string> overridePurgeIssueReports = (t, p) => { throw new NotImplementedException(); };


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
			overrideDeleteIssueProfile(issueId, tenantConfiguration, projectId);
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideDeleteIssueReport(instanceId, tenantConfiguration, projectId);
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
			overridePurgeIssueProfiles(tenantConfiguration, projectId);
		}

		public void PurgeIssueReports(TenantConfiguration tenantConfiguration, string projectId)
		{
			overridePurgeIssueReports(tenantConfiguration, projectId);
		}

		public void UpdateIssueReportIssueId(string instanceId, string IssueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}
	}
}
