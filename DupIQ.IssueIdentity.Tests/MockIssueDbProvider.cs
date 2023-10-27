namespace DupIQ.IssueIdentity.Tests
{
	public class MockIssueDbProvider : IIssueDbProvider
	{
		public Action<string> overrideConfigure = (configJson) => { throw new NotImplementedException(); };
		public Action<IssueReport> overrideAddIssueReport = (issueReport) => { throw new NotImplementedException(); };
		public Action<IssueReport, TenantConfiguration> overrideAddIssueReportTenantConfig = (issueReport, tenantConfig) => { throw new NotImplementedException(); };
		public Action<IssueReport, TenantConfiguration, string> overrideAddIssueReportTenantConfigProjId = (issueReport, tenantConfig, projectId) => { throw new NotImplementedException(); };
		public Action<IssueProfile, TenantConfiguration, string> overrideAddIssueProfileTenantConfigProjId = (issueProfile, tenantConfig, projectId) => { throw new NotImplementedException(); };
		public Action<IssueProfile> overrideAddIssueProfile = (issueProfile) => { throw new NotImplementedException(); };
		public Func<string, IssueReport> overrideGetIssueReport = (instanceId) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, IssueReport> overrideGetIssueReportTenantConfiguration = (instanceId, t) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, IssueReport> overrideGetIssueReportTenantConfig = (instanceId, TenantConfiguration) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, IssueReport> overrideGetIssueReportTenantConfigProjId = (instanceId, TenantConfiguration, s) => { throw new NotImplementedException(); };
		public Func<IssueProfile, IssueReport[]> overrideGetIssueReports = (issueProfile) => { throw new NotImplementedException(); };
		public Func<IssueProfile, TenantConfiguration, IssueReport[]> overrideGetIssueReportsTenantConfiguration = (issueProfile, tenantConfiguration) => { throw new NotImplementedException(); };
		public Func<IssueProfile, TenantConfiguration, string, IssueReport[]> overrideGetIssueReportsTenantConfigurationProjId = (issueProfile, tenantConfiguration, projectId) => { throw new NotImplementedException(); };
		public Func<string, IssueProfile> overrideGetIssueProfile = (s) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, IssueProfile> overrideGetIssueProfileTenantConfig = (s, t) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, IssueProfile> overrideGetIssueProfileTenantConfigProjId = (s, t, s2) => { throw new NotImplementedException(); };
		public Func<IssueProfile[]> overrideGetIssueProfiles = () => { throw new NotImplementedException(); };
		public Func<TenantConfiguration, IssueProfile[]> overrideGetIssueProfilesTenantConfiguration = (t) => { throw new NotImplementedException(); };
		public Func<TenantConfiguration, string, IssueProfile[]> overrideGetIssueProfilesTenantConfigurationProjId = (t, s) => { throw new NotImplementedException(); };
		public Action<string, string> overrideUpdateIssueReportIssueId = (instanceId, IssueId) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration> overrideDeleteIssueProfile = (issueId, tenantConfiguration) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration, string> overrideDeleteIssueProfileProjId = (issueId, tenantConfiguration, projectId) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration> overrideDeleteIssueReport = (instanceId, tenantConfiguration) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration, string> overrideDeleteIssueReportProjId = (instanceId, tenantConfiguration, projectId) => { throw new NotImplementedException(); };

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddIssueProfileTenantConfigProjId(issueProfile, tenantConfiguration, projectId);
		}

		public void AddIssueReport(IssueReport issueReport)
		{
			overrideAddIssueReport(issueReport);
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration)
		{
			overrideAddIssueReportTenantConfig(issueReport, tenantConfiguration);
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddIssueReportTenantConfigProjId(issueReport, tenantConfiguration, projectId);
		}

		public void Configure(string configJson)
		{
			overrideConfigure(configJson);
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration)
		{
			overrideDeleteIssueProfile(id, tenantConfiguration);
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideDeleteIssueProfileProjId(id, tenantConfiguration, projectId);
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration)
		{
			overrideDeleteIssueReport(instanceId, tenantConfiguration);
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideDeleteIssueReportProjId(instanceId, tenantConfiguration, projectId);
		}

		public IssueProfile GetIssueProfile(string issueId)
		{
			return overrideGetIssueProfile(issueId);
		}

		public IssueProfile GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration)
		{
			return overrideGetIssueProfileTenantConfig(issueId, tenantConfiguration);
		}

		public IssueProfile GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueProfileTenantConfigProjId(issueId, tenantConfiguration, projectId);
		}

		public IssueProfile[] GetIssueProfiles()
		{
			return overrideGetIssueProfiles();
		}

		public IssueProfile[] GetIssueProfiles(TenantConfiguration tenantConfiguration)
		{
			return overrideGetIssueProfilesTenantConfiguration(tenantConfiguration);
		}

		public IssueProfile[] GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueProfilesTenantConfigurationProjId(tenantConfiguration, projectId);
		}

		public IssueReport GetIssueReport(string instanceId)
		{
			return overrideGetIssueReport(instanceId);
		}

		public IssueReport GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration)
		{
			return overrideGetIssueReportTenantConfiguration(instanceId, tenantConfiguration);
		}

		public IssueReport GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueReportTenantConfigProjId(instanceId, tenantConfiguration, projectId);
		}

		public IssueReport[] GetIssueReports(IssueProfile issueProfile)
		{
			return overrideGetIssueReports(issueProfile);
		}

		public IssueReport[] GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration)
		{
			return overrideGetIssueReportsTenantConfiguration(issueProfile, tenantConfiguration);
		}

		public IssueReport[] GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueReportsTenantConfigurationProjId(issueProfile, tenantConfiguration, projectId);
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId)
		{
			overrideUpdateIssueReportIssueId(instanceId, issueId);
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}
	}
}