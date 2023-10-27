using DupIQ.IssueIdentity;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	[TestClass]
	public class SqlIssueDBIOHelperTests
	{
		private SqlIOHelperConfig SqlConfig;
		private TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "tenant1", Configuration = { } };
		private TenantConfiguration tenantConfiguration2 = new TenantConfiguration() { TenantId = "tenant2", Configuration = { } };
		private string projectId = "testproject1";
		private string projectId2 = "testproject2";

		[TestInitialize]
		public void GetSqlDatabaseConfig()
		{
			var builder = new ConfigurationBuilder().AddUserSecrets<SqlTenantDatabaseHelperTests>();
			var thing = builder.Build();
			SqlConfig = thing.GetSection("SqlIOHelperConfig").Get<SqlIOHelperConfig>();
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.PurgeIssueProfiles(tenantConfiguration.TenantId, projectId);
			sqlIssueDbIOHelper.PurgeIssueReports(tenantConfiguration.TenantId, projectId);
		}

		[TestCleanup]
		public void DeleteAllRecords()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.PurgeIssueProfiles(tenantConfiguration.TenantId, projectId);
			sqlIssueDbIOHelper.PurgeIssueReports(tenantConfiguration.TenantId, projectId);
		}

		[TestMethod]
		public void AddIssueProfile()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);

			string issueId = string.Empty;
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfiles(tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueId = reader["Id"].ToString().Trim();
					break;
				}
			}
			Assert.AreEqual("testissue1", issueId, "Fail if we do not get the expected issueid.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueProfile_missingid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg", FirstReportedDate = DateTime.Now, IsNew = true },
				tenantConfiguration,
				projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueProfile_missingmessage()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddIssueProfile_nullprofile()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				null,
				tenantConfiguration,
				projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddIssueProfile_nulltenantconfig()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				null,
				projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueProfile_emptyprojectid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				string.Empty);
		}

		[TestMethod]
		public void AddIssueReport()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message" }, tenantConfiguration, projectId);

			string instanceId = string.Empty;
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					instanceId = reader["InstanceId"].ToString().Trim();
					break;
				}
			}
			Assert.AreEqual("instance1", instanceId, "Fail if we do not get the expected instanceid.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddIssueReport_nullreport()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(null, tenantConfiguration, projectId);

		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueReport_noissueid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueMessage = "test message" }, tenantConfiguration, projectId);

			string instanceId = string.Empty;
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					instanceId = reader["InstanceId"].ToString().Trim();
					break;
				}
			}
			Assert.AreEqual("instance1", instanceId, "Fail if we do not get the expected instanceid.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueReport_nomessage()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1" }, tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueReport_noinstanceid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message" }, tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueReport_notenantid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			TenantConfiguration newTenantWithoutId = new TenantConfiguration();
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message" }, newTenantWithoutId, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddIssueReport_nulltenantconfiguration()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message" }, null, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddIssueReport_emptyprojectid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message" }, tenantConfiguration, string.Empty);
		}

		[TestMethod]
		public void GetIssueProfile()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);

			string issueId = string.Empty;
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueId = reader["Id"].ToString().Trim();
					break;
				}
			}
			Assert.AreEqual("testissue1", issueId, "Fail if we do not get the expected issueid.");
		}

		[TestMethod]
		public void GetIssueProfile_doesnotexist()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			string issueId = string.Empty;
			int itemcount = 0;
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					itemcount++;
				}
			}
			Assert.AreEqual(0, itemcount, "Fail if there were any issue profiles returned.");
		}

		[TestMethod]
		public void GetIssueProfile_onlygetsforspecifiedtenant()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 1", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 2", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration2,
				projectId);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["ExampleMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issueMessages.Count, "Fail if we get more than one issue returned.");
			Assert.AreEqual("test msg 1", issueMessages[0], "Fail if we do not get the expected issue message.");
		}

		[TestMethod]
		public void GetIssueProfile_onlygetsforspecifiedproject()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 1", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 2", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId2);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["ExampleMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issueMessages.Count, "Fail if we get more than one issue returned.");
			Assert.AreEqual("test msg 1", issueMessages[0], "Fail if we do not get the expected issue message.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueProfile_emptytestissue()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile(string.Empty, tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetIssueProfile_nulltenantconfiguration()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", null, projectId);

		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueProfile_emptytenantid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			TenantConfiguration emptyTenantIdConfiguration = new TenantConfiguration();

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", emptyTenantIdConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueProfile_emptyprojectId()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, string.Empty);
		}

		[TestMethod]
		public void GetIssueProfiles()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "tenant1", Configuration = { } };
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 1", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 2", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue2" },
				tenantConfiguration,
				projectId);

			List<string> issues = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfiles(tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issues.Add(reader["Id"].ToString().Trim());
				}
			}
			Assert.AreEqual(2, issues.Count, "Fail if we do not get back expected number of issues.");
			Assert.AreEqual("testissue1", issues[0], "Fail if we do not get the expected issueid from first issue.");
			Assert.AreEqual("testissue2", issues[1], "Fail if we do not get the expected issueid from second issue.");
		}

		[TestMethod]
		public void GetIssueProfiles_noneexist()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "tenant1", Configuration = { } };

			List<string> issues = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfiles(tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issues.Add(reader["Id"].ToString().Trim());
				}
			}
			Assert.AreEqual(0, issues.Count, "Fail if any issue profiles are returned.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetIssueProfiles_nulltenantconfiguration()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfiles(null, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueProfiles_emptytenantid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			TenantConfiguration tenantConfiguration = new TenantConfiguration();

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfiles(tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueProfiles_emptyprojectid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "tenant1", Configuration = { } };

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfiles(tenantConfiguration, string.Empty);
		}

		[TestMethod]
		public void GetIssueReport_doesnotexist()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["IssueMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(0, issueMessages.Count, "Fail if we get any issue returned.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueReport_emptyinstanceid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport(string.Empty, tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetIssueReport_nulltenantconfiguration()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", null, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueReport_emptytenantid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", new TenantConfiguration(), projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueReport_emptyprojectid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, string.Empty);
		}

		[TestMethod]
		public void GetIssueReport_onlygetsforspecifiedtenant()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 1 " },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 2" },
				tenantConfiguration2,
				projectId);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["IssueMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issueMessages.Count, "Fail if we get more than one issue returned.");
			Assert.AreEqual("test msg 1", issueMessages[0], "Fail if we do not get the expected issue message.");
		}

		[TestMethod]
		public void GetIssueReport_onlygetsforspecifiedproject()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 1 " },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 2" },
				tenantConfiguration,
				projectId2);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["IssueMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issueMessages.Count, "Fail if we get more than one issue returned.");
			Assert.AreEqual("test msg 1", issueMessages[0], "Fail if we do not get the expected issue message.");
		}

		[TestMethod]
		public void GetIssueReports_doesnotexist()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["IssueMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(0, issueMessages.Count, "Fail if we get any issue returned.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetIssueReports_nullissueprofile()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(null, tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueReports_emptyissueid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile(), tenantConfiguration, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetIssueReports_nulltenantconfiguration()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, null, projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueReports_emptytenantid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, new TenantConfiguration(), projectId);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GetIssueReports_emptyprojectid()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, tenantConfiguration, string.Empty);
		}

		[TestMethod]
		public void GetIssueReports_onlygetsforspecifiedtenant()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 1 " },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 2" },
				tenantConfiguration2,
				projectId);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["IssueMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issueMessages.Count, "Fail if we get more than one issue returned.");
			Assert.AreEqual("test msg 1", issueMessages[0], "Fail if we do not get the expected issue message.");
		}

		[TestMethod]
		public void GetIssueReports_onlygetsforspecifiedproject()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 1 " },
				tenantConfiguration,
				projectId);
			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test msg 2" },
				tenantConfiguration,
				projectId2);

			List<string> issueMessages = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issueMessages.Add(reader["IssueMessage"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issueMessages.Count, "Fail if we get more than one issue returned.");
			Assert.AreEqual("test msg 1", issueMessages[0], "Fail if we do not get the expected issue message.");
		}

		[TestMethod]
		public void GetReports()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message 1" }, tenantConfiguration, projectId);
			sqlIssueDbIOHelper.AddIssueReport(new IssueReport() { InstanceId = "instance2", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message 1" }, tenantConfiguration, projectId);

			List<string> reports = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReports(new IssueProfile() { IssueId = "issue1" }, tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					reports.Add(reader["InstanceId"].ToString().Trim());
				}
			}
			Assert.AreEqual(2, reports.Count, "Fail if we did not get back the exected number of issue reports.");
			Assert.AreEqual("instance1", reports[0], "Fail if we do not get the expected instanceid on first report.");
			Assert.AreEqual("instance2", reports[1], "Fail if we do not get the expected instanceid on second report.");
		}

		[TestMethod]
		public void DeleteIssueProfile()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);

			sqlIssueDbIOHelper.PurgeIssueProfiles(tenantConfiguration.TenantId, projectId);
			sqlIssueDbIOHelper.AddIssueProfile(
				new IssueProfile() { ExampleMessage = "test msg 1", FirstReportedDate = DateTime.Now, IsNew = true, IssueId = "testissue1" },
				tenantConfiguration,
				projectId);

			List<string> issues = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issues.Add(reader["Id"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, issues.Count, "Interim check to be sure we have something to delete.");

			sqlIssueDbIOHelper.DeleteIssueProfile("testissue1", tenantConfiguration, projectId);

			issues.Clear();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueProfile("testissue1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					issues.Add(reader["Id"].ToString().Trim());
				}
			}
			Assert.AreEqual(0, issues.Count, "Fail if testissue1 is still in the database.");

		}

		[TestMethod]
		public void DeleteIssueReport()
		{
			SqlIssueDbIOHelper sqlIssueDbIOHelper = new SqlIssueDbIOHelper(SqlConfig);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "tenant1", Configuration = { } };
			sqlIssueDbIOHelper.PurgeIssueReports(tenantConfiguration.TenantId, projectId);

			sqlIssueDbIOHelper.AddIssueReport(
				new IssueReport() { InstanceId = "instance1", IssueDate = DateTime.Now, IssueId = "issue1", IssueMessage = "test message 1" },
				tenantConfiguration,
				projectId);

			List<string> reports = new List<string>();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					reports.Add(reader["InstanceId"].ToString().Trim());
				}
			}
			Assert.AreEqual(1, reports.Count, "Interim check to ensure we have something to delete.");

			sqlIssueDbIOHelper.DeleteIssueReport("instance1", tenantConfiguration, projectId);

			reports.Clear();
			using (DbDataReader reader = sqlIssueDbIOHelper.GetIssueReport("instance1", tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					reports.Add(reader["InstanceId"].ToString().Trim());
				}
			}
			Assert.AreEqual(0, reports.Count, "Fail if the issue report is still in the database.");
		}
	}
}
