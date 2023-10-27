using Microsoft.Extensions.Logging;

namespace DupIQ.IssueIdentity.Tests
{
	[TestClass]
	public class IssueIdentityManager_unittests
	{
		[TestMethod]
		public void AddIssueProfile()
		{
			IssueProfile passedInIssueProfile = null;
			MockLogger logger = new MockLogger();

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideAddIssueProfile = (i, t, s) => { passedInIssueProfile = i; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);
			IssueProfile issueProfile = new IssueProfile()
			{
				IssueId = "testid",
				IsNew = true,
				ExampleMessage = "test message",
				FirstReportedDate = DateTime.Now,
				ProviderId = issueIdentityProvider.ID()

			};
			issueIdentityManager.AddIssueProfile(issueProfile, new TenantConfiguration());
			Assert.IsNotNull(passedInIssueProfile, "Fail if the passedInIssueProfile is null.");
		}

		[TestMethod]
		public void DeleteIssueProfile()
		{
			IssueProfile passedInIssueProfile = null;
			string deleteThisIssueId = string.Empty;
			TenantConfiguration tenantPartition = null;

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetIssueProfileTenantProject = (i, t, s) => { return new IssueProfile(); };
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideAddIssueProfile = (i, t, s) => { passedInIssueProfile = i; };
			issueIdentityProvider.overrideDeleteIssueProfileProjId = (i, t, s) => { deleteThisIssueId = i; tenantPartition = t; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);
			IssueProfile issueProfile = new IssueProfile()
			{
				IssueId = "testid",
				IsNew = true,
				ExampleMessage = "test message",
				FirstReportedDate = DateTime.Now,
				ProviderId = issueIdentityProvider.ID()

			};
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "tenant1" };
			issueIdentityManager.AddIssueProfile(issueProfile, tenantConfiguration);
			Assert.IsNotNull(passedInIssueProfile, "Fail if the passedInIssueProfile is null.");
			issueIdentityManager.DeleteIssueProfile(issueProfile.IssueId, tenantConfiguration);
			Assert.AreEqual("testid", deleteThisIssueId, "Fail if the db helper was not called to delete the expected issue.");
			Assert.IsNotNull(tenantPartition, "Fail if the tenant partition was not passed in.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile()
		{
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s, i, t, s2) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { IssueId = "something old", Similarity = 1.0f } }; };
			issueIdentityProvider.overrideIdenticialIssueThreshold = (t, s) => { return 1.0f; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Assert.AreEqual("something old", issueProfile.IssueId, "fail if Id does not match expected value.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_usingTenantManager()
		{
			MockTenantManager mockTenantManager = new MockTenantManager();
			mockTenantManager._overrideGetTenantProfile = (s) => { return new TenantProfile() { TenantId = "tenant1" }; };
			mockTenantManager._overrideGetProject = (t, s) => { return new Project() { Name = "test project", ProjectId = "project1", TenantId = "tenant1", SimilarityThreshold = 1.0f }; };

			var logger = (ILogger?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<IssueIdentityManager>();

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s, i, t, s2) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { IssueId = "something old", Similarity = 1.0f } }; };
			issueIdentityProvider.overrideIdenticialIssueThreshold = (t, s) => { return 1.0f; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, mockTenantManager, logger, string.Empty);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Assert.AreEqual("something old", issueProfile.IssueId, "fail if Id does not match expected value.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_usetaggedproviderifitreturnstrue()
		{
			bool calledAddIssueProfile = false;
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideCanHandleIssue = (s) => { return true; };
			taggedIssueIdProvider.overrideGetRelatedIssueProfilesTenantConfigProjId = (s, c, t, s2) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { IssueId = "taggedissue", Similarity = 1.0f } }; };
			taggedIssueIdProvider.overrideAddIssueProfileTenantConfig = (i, t) => { calledAddIssueProfile = true; };
			taggedIssueIdProvider.overrideIdenticalIssueThreshold = (t, p) => { return 1.0f; };


			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetIssueProfile = (s) => { return new IssueProfile() { IssueId = "testprofile" }; };
			issueIdentityProvider.overrideIdenticialIssueThreshold = (t, s) => { return 1.0f; };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s1, i, t, s2) => { return new RelatedIssueProfile[] { }; };
			issueIdentityProvider.overrideAddIssueProfile = (i, t, s) => { };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { taggedIssueIdProvider, issueIdentityProvider }, null);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Assert.AreEqual("taggedissue", issueProfile.IssueId, "fail if Id does not match expected value.");
			Assert.IsFalse(calledAddIssueProfile, "fail if called AddIssueProfile.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_usetaggedproviderifitreturnstruewithnorelatedissues()
		{
			bool calledAddIssueProfile = false;
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideCanHandleIssue = (s) => { return true; };
			taggedIssueIdProvider.overrideGetRelatedIssueProfilesTenantConfigProjId = (s, c, t, s2) => { return new RelatedIssueProfile[] { }; };
			taggedIssueIdProvider.overrideAddIssueProfileTenantConfigProjId = (i, t, s2) => { calledAddIssueProfile = true; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetIssueProfile = (s) => { return new IssueProfile() { IssueId = "testprofile" }; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { taggedIssueIdProvider, issueIdentityProvider }, null);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Guid temp;
			Assert.IsTrue(Guid.TryParse(issueProfile.IssueId, out temp), $"fail if Id is not a valid Guid. id={issueProfile.IssueId}");
			Assert.IsTrue(calledAddIssueProfile, "fail if AddIssueProfile was not called.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_throwsifbothcanhandlefalse()
		{
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideCanHandleIssue = (s) => { return false; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return false; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);

			try
			{
				IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
				Assert.Fail("fail gets here because it should have thrown an exception.");
			}
			catch (UnableToMatchIssueException e)
			{
				Assert.AreEqual("test this", e.IssueMessage, "Fail if did not throw for right issue message.");
			}
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_ExistingProfile()
		{
			bool calledAddIssueProfile = false;

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetIssueProfile = (s) => { return new IssueProfile() { IssueId = "testprofile" }; };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s, i, t, s2) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { IssueId = "something old", Similarity = 1.0f } }; };
			issueIdentityProvider.overrideAddIssueProfile = (i, t, s) => { calledAddIssueProfile = true; };
			issueIdentityProvider.overrideIdenticialIssueThreshold = (t, s) => { return 1.0f; };
			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Assert.AreEqual("something old", issueProfile.IssueId, "fail if Id does not match expected value of existing profile.");
			Assert.AreEqual(false, issueProfile.IsNew, "fail if the issue is new.");
			Assert.IsFalse(calledAddIssueProfile, "fail if AddIssueProfile was called.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_NewIssueProfile()
		{
			bool calledAddIssue = false;

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetIssueProfile = (s) => { return new IssueProfile() { IssueId = "testprofile" }; };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s, i, t, s2) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { IssueId = "something old", Similarity = 0.0f } }; };
			issueIdentityProvider.overrideAddIssueProfile = (i, t, s) => { calledAddIssue = true; };
			issueIdentityProvider.overrideIdenticialIssueThreshold = (t, s) => { return 1.0f; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Assert.AreNotEqual("something old", issueProfile.IssueId, "fail if is same as old one that was returned - expected a GUID instead.");
			Assert.AreEqual(true, issueProfile.IsNew, "fail if the issue is not new.");
			Assert.IsTrue(calledAddIssue, "fail if AddIssueProfile was not called.");
		}

		[TestMethod]
		public void GetOrCreateIssueProfile_NoRelatedIssueProfile()
		{
			bool calledAddIssue = false;
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideCanHandleIssue = (s) => { return false; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideCanHandleIssue = (s) => { return true; };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s, i, t, s2) => { return new RelatedIssueProfile[] { }; };
			issueIdentityProvider.overrideAddIssueProfile = (i, t, s) => { calledAddIssue = true; };

			IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null);
			IssueProfile issueProfile = issueIdentityManager.GetOrCreateIssueProfile("test this", new TenantConfiguration());
			Assert.AreEqual(true, issueProfile.IsNew, "fail if the issue is not new.");
			Guid temp;
			Assert.IsTrue(Guid.TryParse(issueProfile.IssueId, out temp), "fail if the id is not a valid guid");
		}
	}
}