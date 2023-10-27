namespace DupIQ.IssueIdentity.Tests
{
	public class MockIssueIdProvider : IIssueIdentityProvider
	{
		public Action<IssueProfile, TenantConfiguration, string> overrideAddIssueProfile = (issueProfile, tenantConfiguration, s) => { throw new NotImplementedException(); };

		public Action<string, TenantConfiguration> overrideDeleteIssueProfile = (issueId, tenanatConfiguration) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration, string> overrideDeleteIssueProfileProjId = (issueId, tenanatConfiguration, projectId) => { throw new NotImplementedException(); };
		public Action<string> overrideConfigure = (configJson) => { throw new NotImplementedException(); };
		public Func<string, bool> overrideCanHandleIssue = (issueMessage) => { throw new NotImplementedException(); };
		public Func<string, IssueProfile> overrideGetIssueProfile = (issueMessage) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, IssueProfile> overrideGetIssueProfileTenantProject = (issueMessage, tenantConfiguration, projectId) => { throw new NotImplementedException(); };
		public Func<string, int, RelatedIssueProfile[]> overrideGetRelatedProfiles = (issueMessage, count) => { throw new NotImplementedException(); };
		public Func<string, int, TenantConfiguration, RelatedIssueProfile[]> overrideGetRelatedProfilesTenantConfig = (issueMessage, count, t) => { throw new NotImplementedException(); };
		public Func<string, int, TenantConfiguration, string, RelatedIssueProfile[]> overrideGetRelatedProfilesTenantConfigProjId = (issueMessage, count, t, s) => { throw new NotImplementedException(); };
		public Func<TenantConfiguration, string, float> overrideIdenticialIssueThreshold = (t, s) => { throw new NotImplementedException(); };

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddIssueProfile(issueProfile, tenantConfiguration, projectId);
		}

		public bool CanHandleIssue(string issueMessage)
		{
			return overrideCanHandleIssue(issueMessage);
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

		public IssueProfile GetIssueProfile(string issueMessage)
		{
			return overrideGetIssueProfile(issueMessage);
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueProfileTenantProject(id, tenantConfiguration, projectId);
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count)
		{
			return overrideGetRelatedProfiles(issueMessage, count);
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration)
		{
			return overrideGetRelatedProfilesTenantConfig(issueMessage, count, tenantConfiguration);
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetRelatedProfilesTenantConfigProjId(issueMessage, count, tenantConfiguration, projectId);
		}

		public string ID()
		{
			return GetType().Name;
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideIdenticialIssueThreshold(tenantConfiguration, projectId);
		}
	}
}