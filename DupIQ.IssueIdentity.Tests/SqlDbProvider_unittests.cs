using DupIQ.IssueIdentity;
using DupIQ.IssueIdentityProviders.Sql;
using System.Data;

namespace DupIQ.IssueIdentity.Tests
{
	[TestClass]
	public class SqlDbProvider_unittests
	{
		[TestMethod]
		public void GetIssueProfile()
		{
			DateTime nowTime = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueProfile = (s, t, p) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("Id", typeof(string), string.Empty);
				dt.Columns.Add("ExampleMessage", typeof(string), string.Empty);
				dt.Columns.Add("FirstReportedDate", typeof(DateTime), string.Empty);
				dt.Rows.Add("testissue", "testmessage", nowTime);
				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			IssueProfile issueProfile = sqlDbProvider.GetIssueProfile("testissue", new TenantConfiguration(), string.Empty);
			Assert.AreEqual("testissue", issueProfile.IssueId, "Fail if issue profile id does not match expected.");
			Assert.AreEqual("testmessage", issueProfile.ExampleMessage, "Fail if example message does not match expected.");
			Assert.AreEqual(nowTime.ToString(), issueProfile.FirstReportedDate.ToString(), "Fail if date does not match expected value.");
		}

		[TestMethod]
		public void GetIssueProfile_issuedoesnotexist()
		{
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueProfile = (s, t, p) =>
			{
				// populate no rows into the table - outward
				// caller should throw in this case
				DataTable dt = new DataTable();
				dt.Columns.Add("IssueId", typeof(string), string.Empty);
				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);

			try
			{
				IssueProfile issueProfile = sqlDbProvider.GetIssueProfile("testissue", new TenantConfiguration(), string.Empty);
				Assert.Fail("Should fail if got this far because should have thrown.");
			}
			catch (IssueDoesNotExistException e)
			{
				Assert.AreEqual("testissue", e.IssueId, "Fail if exception does not mention correct issue id.");
			}
		}

		[TestMethod]
		public void GetIssueProfiles()
		{
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueProfiles = (t, s) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("Id", typeof(string), string.Empty);
				dt.Columns.Add("ExampleMessage", typeof(string), string.Empty);
				dt.Columns.Add("FirstReportedDate", typeof(DateTime), string.Empty);

				dt.Rows.Add("issue1", "message1", timeNow);
				dt.Rows.Add("issue2", "message2", timeNow);

				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			IssueProfile[] issueProfiles = sqlDbProvider.GetIssueProfiles(new TenantConfiguration(), string.Empty);
			Assert.AreEqual(2, issueProfiles.Count(), "Fail if the expected number of profiles was not returned.");
			Assert.AreEqual("issue1", issueProfiles[0].IssueId, "Fail if issue profile id does not match expected.");
			Assert.AreEqual("issue2", issueProfiles[1].IssueId, "Fail if issue profile id does not match expected.");
		}

		[TestMethod]
		public void GetIssueProfiles_noprofiles()
		{
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueProfiles = (t, s) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("IssueId", typeof(string), string.Empty);
				dt.Columns.Add("ExampleMessage", typeof(string), string.Empty);

				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			IssueProfile[] issueProfiles = sqlDbProvider.GetIssueProfiles(new TenantConfiguration(), string.Empty);
			Assert.AreEqual(0, issueProfiles.Count(), "Fail if there are any profiles returned.");
		}

		[TestMethod]
		public void GetIssueReport()
		{
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueReport = (s, t, p) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("InstanceId", typeof(string), string.Empty);
				dt.Columns.Add("IssueMessage", typeof(string), string.Empty);
				dt.Columns.Add("IssueId", typeof(string), string.Empty);
				dt.Columns.Add("IssueDate", typeof(DateTime), string.Empty);

				dt.Rows.Add("testinstance", "testmessage", "testissue", timeNow);

				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			IssueReport issueReport = sqlDbProvider.GetIssueReport("testinstance", new TenantConfiguration(), string.Empty);
			Assert.AreEqual("testinstance", issueReport.InstanceId, "Fail if issue profile id does not match expected.");
			Assert.AreEqual("testmessage", issueReport.IssueMessage, "Fail if issue message does not match expected.");
			Assert.AreEqual("testissue", issueReport.IssueId, "Fail if issueId does not match expected.");
			Assert.AreEqual(timeNow.ToString(), issueReport.IssueDate.ToString(), "Fail if IssueDate does not match expected.");
		}

		[TestMethod]
		public void GetIssueReport_noresults()
		{
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueReport = (s, t, p) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("InstanceId", typeof(string), string.Empty);
				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			try
			{
				IssueReport issueReport = sqlDbProvider.GetIssueReport("testinstance", new TenantConfiguration(), string.Empty);
				Assert.Fail("Should fail if gets this far because should have thrown.");
			}
			catch (IssueDoesNotExistException e)
			{
				Assert.AreEqual("testinstance", e.IssueId, "Fail if exception does not mention expected instance id.");
			}

		}

		[TestMethod]
		public void GetIssueReports()
		{
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueReports = (i, t, p) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("InstanceId", typeof(string), string.Empty);
				dt.Columns.Add("IssueMessage", typeof(string), string.Empty);
				dt.Columns.Add("IssueId", typeof(string), string.Empty);
				dt.Columns.Add("IssueDate", typeof(DateTime), string.Empty);

				dt.Rows.Add("testinstance1", "testmessage1", i.IssueId, timeNow);
				dt.Rows.Add("testinstance2", "testmessage2", i.IssueId, timeNow);

				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			IssueReport[] issueReports = sqlDbProvider.GetIssueReports(new IssueProfile() { IssueId = "testissue" }, new TenantConfiguration(), string.Empty);
			Assert.AreEqual(2, issueReports.Count(), "Fail if expected number of issue reports is not returned.");

			IssueReport issueReport = issueReports[0];

			Assert.AreEqual("testinstance1", issueReport.InstanceId, "Fail if issue profile id does not match expected.");
			Assert.AreEqual("testmessage1", issueReport.IssueMessage, "Fail if issue message does not match expected.");
			Assert.AreEqual("testissue", issueReport.IssueId, "Fail if issueId does not match expected.");
			Assert.AreEqual(timeNow.ToString(), issueReport.IssueDate.ToString(), "Fail if IssueDate does not match expected.");
			issueReport = issueReports[1];

			Assert.AreEqual("testinstance2", issueReport.InstanceId, "Fail if issue profile id does not match expected.");
			Assert.AreEqual("testmessage2", issueReport.IssueMessage, "Fail if issue message does not match expected.");
			Assert.AreEqual("testissue", issueReport.IssueId, "Fail if issueId does not match expected.");
			Assert.AreEqual(timeNow.ToString(), issueReport.IssueDate.ToString(), "Fail if IssueDate does not match expected.");
		}

		[TestMethod]
		public void GetIssueReports_noresults()
		{
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideGetIssueReports = (i, t, p) =>
			{
				DataTable dt = new DataTable();
				dt.Columns.Add("InstanceId", typeof(string), string.Empty);
				dt.Columns.Add("IssueMessage", typeof(string), string.Empty);
				dt.Columns.Add("IssueId", typeof(string), string.Empty);
				dt.Columns.Add("IssueDate", typeof(DateTime), string.Empty);

				return dt.CreateDataReader();
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			IssueReport[] issueReports = sqlDbProvider.GetIssueReports(new IssueProfile() { IssueId = "testissue" }, new TenantConfiguration(), string.Empty);
			Assert.AreEqual(0, issueReports.Count(), "Fail if expected number of issue reports is not returned.");
		}

		[TestMethod]
		public void AddIssueProfile()
		{
			IssueProfile passedInIssueProfile = null;
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideAddIssueProfile = (i, t, p) =>
			{
				passedInIssueProfile = i;
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			sqlDbProvider.AddIssueProfile(new IssueProfile() { IssueId = "testissue" }, new TenantConfiguration(), string.Empty);
			Assert.AreEqual("testissue", passedInIssueProfile.IssueId, "Fail if the passed in issue profile id does not match.");
		}

		[TestMethod]
		public void AddIssueReport()
		{
			IssueReport passedInIssueProfile = null;
			DateTime timeNow = DateTime.Now;
			MockSqlIOHelper mockSqlIOHelper = new MockSqlIOHelper();
			mockSqlIOHelper.overrideAddIssueReport = (i, t, p) =>
			{
				passedInIssueProfile = i;
			};
			SqlIssueDbProvider sqlDbProvider = new SqlIssueDbProvider(mockSqlIOHelper);
			sqlDbProvider.AddIssueReport(new IssueReport() { InstanceId = "testinstance" }, new TenantConfiguration(), string.Empty);
			Assert.AreEqual("testinstance", passedInIssueProfile.InstanceId, "Fail if the passed in issue report instance id does not match.");
		}
	}
}