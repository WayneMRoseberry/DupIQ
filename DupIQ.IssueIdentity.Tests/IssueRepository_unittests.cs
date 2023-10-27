using DupIQ.IssueIdentity;
using DupIQ.IssueIdentity.Providers;

namespace DupIQ.IssueIdentity.Tests
{
	[TestClass]
	public class IssueRepository_unittests
	{
		[TestMethod]
		public void IssueRepository_instantiate()
		{
			IssueRepository repository = new IssueRepository();
			Assert.IsNotNull(repository);
		}

		[TestMethod]
		public void IssueRepository_InstantiatesProviders_instantiate()
		{
			bool dbConfigCalled = false;
			bool tagConfigCalled = false;
			bool idConfigCalled = false;

			MockIssueDbProvider mockIssueDbProvider = new MockIssueDbProvider() { overrideConfigure = (s) => { dbConfigCalled = true; } };

			MockTaggedIssueIdProvider mockTaggedProvider = new MockTaggedIssueIdProvider() { overrideConfigure = (s) => { tagConfigCalled = true; } };
			MockIssueIdProvider mockIdProvider = new MockIssueIdProvider() { overrideConfigure = (s) => { idConfigCalled = true; } };

			IssueIdentityManager identityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { mockIdProvider }, null);

			IssueRepository repository = new IssueRepository(mockIssueDbProvider, identityManager);

			Assert.IsTrue(dbConfigCalled, "fail if Configure not called on db provider.");
			//Assert.IsTrue(tagConfigCalled, "fail if Configure not called on tag id provider.");
			//Assert.IsTrue(idConfigCalled, "fail if Configure not called on id provider.");
		}

		[TestMethod]
		public void ReportIssue_newissue()
		{
			IssueReport passedInIssueReport = null;
			IssueProfile passedInIssueProfile = null;
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueReportTenantConfigProjId = (i, t, s2) => { passedInIssueReport = i; };
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (i, t, s2) => { passedInIssueProfile = i; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideAddIssueProfile = (i, t, p) => { };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (i, c, t, p) => { return new RelatedIssueProfile[] { }; };
			issueIdentityProvider.overrideCanHandleIssue = (i) => { return true; };
			issueIdentityProvider.overrideConfigure = (s) => { };
			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			DateTime testdate = new DateTime(2000, 01, 02);
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			IssueProfile issueProfile = issueRepository.ReportIssue(new IssueReport() { IssueMessage = "testmessage", IssueDate = testdate }, emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.IsTrue(issueProfile.IsNew, "Fail if the issue is not new.");
			Assert.IsNotNull(passedInIssueReport, "Fail if the passed in issue report is null.");
			Assert.IsNotNull(passedInIssueProfile, "Fail if the passed in issue profile is null.");
			Assert.AreEqual(testdate.ToString(), issueProfile.FirstReportedDate.ToString(), "Fail if the first reported date is not as expected.");
			Assert.AreEqual("testmessage", passedInIssueProfile.ExampleMessage, "Fail if the example message is not expected value.");
			Guid temp;
			Assert.IsTrue(Guid.TryParse(passedInIssueReport.IssueId, out temp), "Fail if issue id is not a guid.");
			Assert.AreEqual("testmessage", passedInIssueReport.IssueMessage, "Fail if did not get expected issue message on passedInIssueReport.");
		}

		[TestMethod]
		public void ReportIssue_specifytenantid_newissue()
		{
			IssueReport passedInIssueReport = null;
			IssueProfile passedInIssueProfile = null;
			string passedInTenantId = string.Empty;

			MockTenantManager tenantManager = new MockTenantManager();
			tenantManager._overrideGetTenantConfiguration = (i) => { passedInTenantId = i; return new TenantConfiguration() { TenantId = "returnedtenant" }; };

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueReportTenantConfigProjId = (i, t, s2) => { passedInIssueReport = i; };
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (i, t, s2) => { passedInIssueProfile = i; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideAddIssueProfile = (i, t, p) => { };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (i, c, t, p) => { return new RelatedIssueProfile[] { }; };
			issueIdentityProvider.overrideCanHandleIssue = (i) => { return true; };
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null), new MockLogger(), tenantManager);
			DateTime testdate = new DateTime(2000, 01, 02);
			IssueProfile issueProfile = issueRepository.ReportIssue(new IssueReport() { IssueMessage = "testmessage", IssueDate = testdate }, "tenant1", string.Empty);
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the IssueRepository did not call GetTenantConfiguration.");
			Assert.IsTrue(issueProfile.IsNew, "Fail if the issue is not new.");
			Assert.IsNotNull(passedInIssueReport, "Fail if the passed in issue report is null.");
			Assert.IsNotNull(passedInIssueProfile, "Fail if the passed in issue profile is null.");
			Assert.AreEqual(testdate.ToString(), issueProfile.FirstReportedDate.ToString(), "Fail if the first reported date is not as expected.");
			Assert.AreEqual("testmessage", passedInIssueProfile.ExampleMessage, "Fail if the example message is not expected value.");
			Guid temp;
			Assert.IsTrue(Guid.TryParse(passedInIssueReport.IssueId, out temp), "Fail if issue id is not a guid.");
			Assert.AreEqual("testmessage", passedInIssueReport.IssueMessage, "Fail if did not get expected issue message on passedInIssueReport.");
		}

		[TestMethod]
		public void ReportIssue_existingissue()
		{
			DateTime oldIssueReportedDate = new DateTime(2000, 01, 02);
			IssueReport passedInIssueReport = null;
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (i, t, p) => { };
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueReportTenantConfigProjId = (i, t, s2) => { passedInIssueReport = i; };
			dbProvider.overrideGetIssueProfileTenantConfigProjId = (s, t, s2) => { return new IssueProfile() { FirstReportedDate = oldIssueReportedDate }; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideAddIssueProfile = (i, t, p) => { };
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (i, c, t, p) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { Similarity = 1.0f, IssueId = "oldissue" } }; };
			issueIdentityProvider.overrideCanHandleIssue = (i) => { return true; };
			issueIdentityProvider.overrideConfigure = (s) => { };
			issueIdentityProvider.overrideIdenticialIssueThreshold = (t, s) => { return 1.0f; };
			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			IssueProfile issueProfile = issueRepository.ReportIssue(new IssueReport() { IssueMessage = "I had a failure." }, emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.IsFalse(issueProfile.IsNew, "Fail if the issue is new.");
			Assert.AreEqual(oldIssueReportedDate.ToString(), issueProfile.FirstReportedDate.ToString(), "Fail if the old issue first reported date is not present.");
			Assert.IsNotNull(passedInIssueReport, "Fail if the passed in issue report is null.");
			Assert.AreEqual("oldissue", passedInIssueReport.IssueId, "fail if did not get the expected old issue id.");
			Assert.AreEqual("I had a failure.", passedInIssueReport.IssueMessage, "Fail if did not get expected issue message on passedInIssueReport.");
		}

		[TestMethod]
		public void GetIssueReport()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueReportTenantConfigProjId = (instanceId, tenantConfiguration, projectId) => { return new IssueReport() { InstanceId = instanceId, IssueMessage = "test message" }; };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			IssueReport issueReport = issueRepository.IssueReport("testinstanceid", "testtenant", string.Empty);
			Assert.AreEqual("test message", issueReport.IssueMessage);
			Assert.AreEqual("testinstanceid", issueReport.InstanceId);
		}

		[TestMethod]
		public void GetIssueReport_nomatchingissue_throws()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueReportTenantConfigProjId = (instanceId, TenantConfiguration, projectId) => { throw new IssueDoesNotExistException("testinstanceid"); };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));

			try
			{
				IssueReport issueReport = issueRepository.IssueReport("testinstanceid", "testtenant", string.Empty);
				Assert.Fail("should not get here because it should throw.");
			}
			catch (IssueReportDoesNotExistException e)
			{
				Assert.AreEqual("testinstanceid", e.InstanceId, "Fail if does not match issue report instance.");
			}
		}

		[TestMethod]
		public void GetIssueReports()
		{
			string issueIdPassedIn = string.Empty;
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueReportsTenantConfigurationProjId = (i, tenantConfiguration, s2) => { return new IssueReport[] { new IssueReport() { InstanceId = "1", IssueId = i.IssueId }, new IssueReport() { InstanceId = "2", IssueId = i.IssueId } }; };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			IssueReport[] reports = issueRepository.GetIssueReportsFromProject(new IssueProfile() { IssueId = "testissue" }, emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.AreEqual(2, reports.Count(), "Fail if did not get back expected number of reports.");
		}

		[TestMethod]
		public void AddIssueProfile()
		{
			IssueProfile passedToDb = null;
			IssueProfile passedToIdProvider = null;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (p, t, p2) => { passedToDb = p; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };
			issueIdentityProvider.overrideAddIssueProfile = (p, t, s2) => { passedToIdProvider = p; };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));

			IssueProfile issueProfile = new IssueProfile()
			{
				IssueId = "test issue",
				ExampleMessage = "test message",
				ProviderId = issueIdentityProvider.ID()
			};
			issueRepository.AddIssueProfile(issueProfile, "tenant1", string.Empty);
			Assert.IsNotNull(passedToDb, "Fail if the addissueprofile was not called on db.");
			Assert.IsNotNull(passedToIdProvider, "Fail if addissueprofile was not called on id provider");
		}

		[TestMethod]
		public void DeleteIssueProfile()
		{
			IssueProfile passedToDb = null;
			IssueProfile passedToIdProvider = null;
			string deleteThisDbId = string.Empty;
			string deleteThisId = string.Empty;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (p, t, p2) => { passedToDb = p; };
			dbProvider.overrideDeleteIssueProfileProjId = (i, t, s2) => { deleteThisDbId = i; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetIssueProfileTenantProject = (s, t, p) => { return new IssueProfile(); };
			issueIdentityProvider.overrideConfigure = (s) => { };
			issueIdentityProvider.overrideAddIssueProfile = (p, t, s2) => { passedToIdProvider = p; };
			issueIdentityProvider.overrideDeleteIssueProfileProjId = (i, t, s2) => { deleteThisId = i; };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));

			IssueProfile issueProfile = new IssueProfile()
			{
				IssueId = "test issue",
				ExampleMessage = "test message",
				ProviderId = issueIdentityProvider.ID()
			};
			issueRepository.AddIssueProfile(issueProfile, "tenant1", string.Empty);
			Assert.IsNotNull(passedToDb, "Fail if the addissueprofile was not called on db.");
			issueRepository.DeleteIssueProfile(issueProfile.IssueId, "tenant1", false, string.Empty);
			Assert.AreEqual("test issue", deleteThisId, "Fail if the id was not passed in to IdentityProvider DeleteIssueProfile.");
			Assert.AreEqual("test issue", deleteThisDbId, "Fail if the id was not passed in to the DB provider DeleteIssueProfile.");

		}

		[TestMethod]
		public void DeleteIssueProfile_deleteresultstooeqTRUE()
		{
			IssueProfile passedToDb = null;
			IssueProfile passedToIdProvider = null;
			string deleteThisDbId = string.Empty;
			string deleteThisId = string.Empty;
			string passedInIssueId = string.Empty;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideDeleteIssueReportProjId = (i, t, s2) => { deleteThisDbId = i; };
			dbProvider.overrideGetIssueReportsTenantConfigurationProjId = (i, t, s) => { passedToDb = i; return new IssueReport[] { new IssueReport() { InstanceId = "report1" } }; };
			dbProvider.overrideGetIssueProfileTenantConfigProjId = (i, t, s) => { return new IssueProfile() { IssueId = i }; };
			dbProvider.overrideDeleteIssueProfileProjId = (i, t, s) => { passedInIssueId = i; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetIssueProfileTenantProject = (i, t, p) => { return new IssueProfile(); };
			issueIdentityProvider.overrideConfigure = (s) => { };
			issueIdentityProvider.overrideDeleteIssueProfileProjId = (i, t, s2) => { deleteThisId = i; };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			issueRepository.GetIssueReportsFromProject(new IssueProfile(), emptyTenantManager.DefaultTenant(), string.Empty);

			issueRepository.DeleteIssueProfile("test issue", "tenant1", true, string.Empty);
			Assert.AreEqual("test issue", deleteThisId, "Fail if the id was not passed in to IdentityProvider DeleteIssueProfile.");
			Assert.AreEqual("report1", deleteThisDbId, "Fail if the id was not passed in to the DB provider DeleteIssueReport.");
			Assert.AreEqual("test issue", passedToDb.IssueId, "Fail if the issue profile being deleted was never passed to GetIssueReports.");
			Assert.AreEqual("test issue", passedInIssueId, "Fail if the passedInIssueId to delete was never passed to DeleteIssueProfile in the db.");

		}

		[TestMethod]
		public void DeleteIssueProfile_dbproviderthrows_stilldeletesvector()
		{
			IssueProfile passedToDb = null;
			IssueProfile passedToIdProvider = null;
			string deleteThisDbId = string.Empty;
			string deleteThisId = string.Empty;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (p, t, p2) => { passedToDb = p; };
			dbProvider.overrideDeleteIssueProfileProjId = (i, t, s) => { deleteThisId = i; throw new Exception(); };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetIssueProfileTenantProject = (i, t, p) => { return new IssueProfile(); };
			issueIdentityProvider.overrideConfigure = (s) => { };
			issueIdentityProvider.overrideAddIssueProfile = (p, t, s2) => { passedToIdProvider = p; };
			issueIdentityProvider.overrideDeleteIssueProfileProjId = (i, t, s2) => { deleteThisDbId = i; };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));

			issueRepository.DeleteIssueProfile("test issue", "tenant1", false, string.Empty);
			Assert.AreEqual("test issue", deleteThisDbId, "Fail if the id was not passed in to the DB provider DeleteIssueProfile. Even though it throws, we want to know it was called.");
			Assert.AreEqual("test issue", deleteThisId, "Fail if the id was not passed in to IdentityProvider DeleteIssueProfile.");
		}
		[TestMethod]
		public void DeleteIssueProfile_idproviderthrows_stilldeletesdbentry()
		{
			IssueProfile passedToDb = null;
			IssueProfile passedToIdProvider = null;
			string deleteThisDbId = string.Empty;
			string deleteThisId = string.Empty;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideAddIssueProfileTenantConfigProjId = (p, t, p2) => { passedToDb = p; };
			dbProvider.overrideDeleteIssueProfileProjId = (i, t, s2) => { deleteThisDbId = i; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetIssueProfileTenantProject = (i, t, p) => { return new IssueProfile(); };
			issueIdentityProvider.overrideConfigure = (s) => { };
			issueIdentityProvider.overrideAddIssueProfile = (p, t, s2) => { passedToIdProvider = p; };
			issueIdentityProvider.overrideDeleteIssueProfileProjId = (i, t, s2) => { deleteThisId = i; throw new Exception(); };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));

			issueRepository.DeleteIssueProfile("test issue", "tenant1", false, string.Empty);
			Assert.AreEqual("test issue", deleteThisDbId, "Fail if the id was not passed in to the DB provider DeleteIssueProfile.");
			Assert.AreEqual("test issue", deleteThisId, "Fail if the id was not passed in to IdentityProvider DeleteIssueProfile. Even though it throws, we want to know it was called.");
		}


		[TestMethod]
		public void DeleteIssueReport()
		{
			string deleteThisDbId = string.Empty;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideDeleteIssueReportProjId = (i, t, s2) => { deleteThisDbId = i; };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			issueRepository.DeleteIssueReport("report 1", "tenant1", string.Empty);
			Assert.AreEqual("report 1", deleteThisDbId, "Fail if the id was not passed in to the DB provider DeleteIssueProfile.");
		}

		[TestMethod]
		public void DeleteIssueReport_nonexistentitem()
		{
			string deleteThisDbId = string.Empty;

			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideDeleteIssueReportProjId = (i, t, s2) => { deleteThisDbId = i; throw new IssueReportDoesNotExistException(i, new Exception()); };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			try
			{
				issueRepository.DeleteIssueReport("report 1", "tenant1", string.Empty);
				Assert.Fail("Fail if got here because it ought to throw.");
			}
			catch (IssueReportDoesNotExistException e)
			{
				Assert.AreEqual("report 1", e.InstanceId, "Fail if the exception does not mention the IssueReportID.");
			}

		}

		[TestMethod]
		public void GetIssueProfile()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueProfileTenantConfigProjId = (s, t, s2) => { return new IssueProfile() { IssueId = s }; };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			IssueProfile issueProfile = issueRepository.GetIssueProfile("issueId", emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.AreEqual("issueId", issueProfile.IssueId);
		}

		[TestMethod]
		public void GetIssueProfile_nomatchingissue_throws()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueProfile = (s) => { throw new Exception("meaningless exception"); };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			try
			{
				IssueProfile issueProfile = issueRepository.GetIssueProfile("issueId", emptyTenantManager.DefaultTenant(), string.Empty);
				Assert.Fail("Should fail if we get here because it should have thrown exception.");
			}
			catch (IssueDoesNotExistException e)
			{
				Assert.AreEqual("issueId", e.IssueId, "Fail if exception does not indicated correct issue id.");
			}
		}

		[TestMethod]
		public void GetIssueProfiles()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueProfilesTenantConfigurationProjId = (t, s) => { return new IssueProfile[] { new IssueProfile() { IssueId = "testissue" } }; };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			IssueProfile[] issueProfiles = issueRepository.GetIssueProfiles(emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.AreEqual(1, issueProfiles.Count(), "Fail if there is not 1 item in the result list.");
			Assert.AreEqual("testissue", issueProfiles[0].IssueId, "Fail if the id is not expected value.");
		}

		[TestMethod]
		public void GetIssueProfiles_noIssueProfiles()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueProfilesTenantConfigurationProjId = (t, s) => { return new IssueProfile[] { }; };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			IssueProfile[] issueProfiles = issueRepository.GetIssueProfiles(emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.AreEqual(0, issueProfiles.Count(), "Fail if the list of profiles is not empty.");
		}

		[TestMethod]
		public void GetRelatedIssues()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			dbProvider.overrideGetIssueProfileTenantConfigProjId = (s, t, s2) => { return new IssueProfile(); };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			taggedIssueIdProvider.overrideCanHandleIssue = (s) => { return true; };
			taggedIssueIdProvider.overrideGetRelatedIssueProfilesTenantConfigProjId = (s, c, t, s2) => { return new RelatedIssueProfile[] { new RelatedIssueProfile() { IssueId = "oldissue1", Similarity = 1.0f }, new RelatedIssueProfile() { IssueId = "oldissue2", Similarity = 0.8f } }; };

			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetIssueProfileTenantProject = (i, t, p) => { return new IssueProfile(); };
			issueIdentityProvider.overrideCanHandleIssue = (i) => { return true; };
			issueIdentityProvider.overrideConfigure = (s) => { };
			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { taggedIssueIdProvider, issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			RelatedIssueProfile[] profiles = issueRepository.GetRelatedIssueProfiles(new IssueReport() { IssueMessage = "I had a failure." }, 5, emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.AreEqual(2, profiles.Count(), "Fail if there are not at least two related issue profiles.");
			Assert.AreEqual(1.0f, profiles[0].Similarity, "Fail if the similarity of first issue is not 1.0f.");
		}

		[TestMethod]
		public void GetRelatedIssues_norelatedissues()
		{
			MockIssueDbProvider dbProvider = new MockIssueDbProvider();
			dbProvider.overrideConfigure = (s) => { };
			MockTaggedIssueIdProvider taggedIssueIdProvider = new MockTaggedIssueIdProvider();
			taggedIssueIdProvider.overrideConfigure = (s) => { };
			taggedIssueIdProvider.overrideCanHandleIssue = (s) => { return true; };
			taggedIssueIdProvider.overrideGetRelatedIssueProfilesTenantConfigProjId = (s, c, t, s2) => { return new RelatedIssueProfile[] { }; };
			MockIssueIdProvider issueIdentityProvider = new MockIssueIdProvider();
			issueIdentityProvider.overrideGetRelatedProfilesTenantConfigProjId = (s, c, t, s2) => { return new RelatedIssueProfile[] { }; };
			issueIdentityProvider.overrideCanHandleIssue = (i) => { return true; };
			issueIdentityProvider.overrideConfigure = (s) => { };

			IssueRepository issueRepository = new IssueRepository(dbProvider, new IssueIdentityManager(new IIssueIdentityProvider[] { issueIdentityProvider }, null));
			EmptyTenantManager emptyTenantManager = new EmptyTenantManager();
			RelatedIssueProfile[] profiles = issueRepository.GetRelatedIssueProfiles(new IssueReport() { IssueMessage = "I had a failure." }, 5, emptyTenantManager.DefaultTenant(), string.Empty);
			Assert.AreEqual(0, profiles.Count(), "Fail if there are not at least two related issue profiles.");
		}

	}
}