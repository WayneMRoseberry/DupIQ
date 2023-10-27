namespace DupIQ.IssueIdentity.Tests
{
	public class MockTaggedIssueIdProvider : IIssueIdentityProvider
	{
		public Action<IssueProfile> overrideAddIssueProfile = (issueProfile) => { throw new NotImplementedException(); };
		public Action<IssueProfile, TenantConfiguration> overrideAddIssueProfileTenantConfig = (issuseProfile, tenantconfig) => { throw new NotImplementedException(); };
		public Action<IssueProfile, TenantConfiguration, string> overrideAddIssueProfileTenantConfigProjId = (issuseProfile, tenantconfig, projectId) => { throw new NotImplementedException(); };
		public Action<string> overrideConfigure = (configJson) => { throw new NotImplementedException(); };
		public Func<string, bool> overrideCanHandleIssue = (issueMessage) => { throw new NotImplementedException(); };
		public Func<string, IssueProfile> overrideGetIssueProfile = (issueMessage) => { throw new NotImplementedException(); };
		public Func<string, TenantConfiguration, string, IssueProfile> overrideGetIssueProfileTenantProject = (issueMessage, tenantConfig, projectId) => { throw new NotImplementedException(); };
		public Func<string, int, RelatedIssueProfile[]> overrideGetRelatedIssueProfiles = (issueMessage, count) => { throw new NotImplementedException(); };
		public Func<string, int, TenantConfiguration, RelatedIssueProfile[]> overrideGetRelatedIssueProfilesTenantConfig = (s, c, t) => { throw new NotImplementedException(); };
		public Func<string, int, TenantConfiguration, string, RelatedIssueProfile[]> overrideGetRelatedIssueProfilesTenantConfigProjId = (s, c, t, s2) => { throw new NotImplementedException(); };
		public Func<TenantConfiguration, string, float> overrideIdenticalIssueThreshold = (t, p) => { throw new NotImplementedException(); };

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddIssueProfileTenantConfigProjId(issueProfile, tenantConfiguration, projectId);
		}

		public bool CanHandleIssue(string issueMessage)
		{
			return overrideCanHandleIssue(issueMessage);
		}

		public void Configure(string configJson)
		{
			overrideConfigure(configJson);
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetIssueProfileTenantProject(id, tenantConfiguration, projectId);
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideGetRelatedIssueProfilesTenantConfigProjId(issueMessage, count, tenantConfiguration, projectId);
		}

		public string ID()
		{
			return GetType().Name;
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideIdenticalIssueThreshold(tenantConfiguration, projectId);
		}
	}
}