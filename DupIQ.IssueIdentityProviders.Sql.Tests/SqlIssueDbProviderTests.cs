using DupIQ.IssueIdentity;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	[TestClass]
	public class SqlIssueDbProviderTests
	{
		[TestMethod]
		public void AddIssueProfile()
		{
			string issueId = string.Empty;
			string tenantId = string.Empty;
			string projectId = string.Empty;
			MockIssueDbIOHelper mockIssueDbIOHelper = new MockIssueDbIOHelper();
			mockIssueDbIOHelper.overrideAddIssueProfile = (i, t, p) => { issueId = i.IssueId; tenantId = t.TenantId; projectId = p; };

			SqlIssueDbProvider sqlIssueDbProvider = new SqlIssueDbProvider(mockIssueDbIOHelper);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "testtenant1" };
			const string ProjectId = "project1";
			sqlIssueDbProvider.AddIssueProfile(new IssueProfile() { IssueId = "testissue1" }, tenantConfiguration, ProjectId);

			Assert.AreEqual("testissue1", issueId);
			Assert.AreEqual("testtenant1", tenantId);
			Assert.AreEqual("project1", projectId);
		}

		[TestMethod]
		public void AddIssueReport()
		{
			string instanceId = string.Empty;
			string tenantId = string.Empty;
			string projectId = string.Empty;
			MockIssueDbIOHelper mockIssueDbIOHelper = new MockIssueDbIOHelper();
			mockIssueDbIOHelper.overrideAddIssueReport = (i, t, p) => { instanceId = i.InstanceId; tenantId = t.TenantId; projectId = p; };

			SqlIssueDbProvider sqlIssueDbProvider = new SqlIssueDbProvider(mockIssueDbIOHelper);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "testtenant1" };
			const string ProjectId = "project1";
			sqlIssueDbProvider.AddIssueReport(new IssueReport() { InstanceId = "instance1" }, tenantConfiguration, ProjectId);

			Assert.AreEqual("instance1", instanceId);
			Assert.AreEqual("testtenant1", tenantId);
			Assert.AreEqual("project1", projectId);
		}

		[TestMethod]
		public void DeleteIssueProfile()
		{
			string issueId = string.Empty;
			string tenantId = string.Empty;
			string projectId = string.Empty;
			MockIssueDbIOHelper mockIssueDbIOHelper = new MockIssueDbIOHelper();
			mockIssueDbIOHelper.overrideDeleteIssueProfile = (i, t, p) => { issueId = i; tenantId = t.TenantId; projectId = p; };

			SqlIssueDbProvider sqlIssueDbProvider = new SqlIssueDbProvider(mockIssueDbIOHelper);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "testtenant1" };
			const string ProjectId = "project1";
			sqlIssueDbProvider.DeleteIssueProfile("testissue1", tenantConfiguration, ProjectId);

			Assert.AreEqual("testissue1", issueId);
			Assert.AreEqual("testtenant1", tenantId);
			Assert.AreEqual("project1", projectId);
		}

		[TestMethod]
		public void DeleteIssueReport()
		{
			string instanceId = string.Empty;
			string tenantId = string.Empty;
			string projectId = string.Empty;
			MockIssueDbIOHelper mockIssueDbIOHelper = new MockIssueDbIOHelper();
			mockIssueDbIOHelper.overrideDeleteIssueReport = (i, t, p) => { instanceId = i; tenantId = t.TenantId; projectId = p; };

			SqlIssueDbProvider sqlIssueDbProvider = new SqlIssueDbProvider(mockIssueDbIOHelper);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "testtenant1" };
			const string ProjectId = "project1";
			sqlIssueDbProvider.DeleteIssueReport("instance1", tenantConfiguration, ProjectId);

			Assert.AreEqual("instance1", instanceId);
			Assert.AreEqual("testtenant1", tenantId);
			Assert.AreEqual("project1", projectId);
		}
	}
}
