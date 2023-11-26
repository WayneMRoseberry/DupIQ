using DupIQ.IssueIdentity;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	[TestClass]
	public class SqlTenantDatabaseHelperTests
	{
		private SqlIOHelperConfig SqlConfig;

		[TestInitialize]
		public void GetSqlDatabaseConfig()
		{
			var builder = new ConfigurationBuilder().AddUserSecrets<SqlTenantDatabaseHelperTests>();
			var thing = builder.Build();
			SqlConfig = thing.GetSection("SqlIOHelperConfig").Get<SqlIOHelperConfig>();
		}

		[TestCleanup]
		public void DeleteAllRecords()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			sqlTenantDatabaseHelper.PurgeTenants(true);
		}

		[TestMethod]
		public void AddOrUpdateProject()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = "testtenant1",
				Name = "test tenant",
				OwnerId = "user1"
			};

			Project project = new Project() { ProjectId = "project1", Name = "Project 1", OwnerId = "user1", TenantId = "tenant1", SimilarityThreshold = 0.5f };

			sqlTenantDatabaseHelper.AddOrUpdateProject("testtenant1", project);

			Project result = sqlTenantDatabaseHelper.GetProject("testtenant1", "project1");
			Assert.AreEqual("testtenant1", result.TenantId, "Fail if the wrong tenant is returned for the project.");
			Assert.AreEqual("project1", result.ProjectId, "Fail if the project has the wrong id.");
			Assert.AreEqual("Project 1", result.Name, "Fail if the project does not have the expected name.");
			Assert.AreEqual(0.5f, result.SimilarityThreshold, "Fail if the project does not have the expected similarity threshold.");

			PropStuffer props = sqlTenantDatabaseHelper.GetProjectExtendedProperties("testtenant1", "project1");
			Assert.IsNotNull(props, "Fail if the extended properties are null.");

			DbDataReader reader = sqlTenantDatabaseHelper.GetProjects("testtenant1", "user1");
			string projectId = string.Empty;
			while (reader.Read())
			{
				projectId = reader["ProjectId"].ToString().Trim();
			}
			Assert.AreEqual("project1", projectId, "Fail if there is not a project id matching the project we created assigned to the owner.");
		}

		[TestMethod]
		public void AddOrUpdateServiceUserAuth()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			sqlTenantDatabaseHelper.AddOrUpdateUserServiceAuthorization("user1", UserServiceAuthorization.Admin);

			DbDataReader reader = sqlTenantDatabaseHelper.GetUserServiceAuthorization("user1");
			Assert.IsNotNull(reader);
			string authorization = UserServiceAuthorization.Guest.ToString();
			int recordCount = 0;
			while(reader.Read())
			{
				recordCount++;
				authorization = reader["Role"].ToString();
			}
			Assert.AreEqual(1, recordCount, "Fail if there is not one record for the user.");
			Assert.AreEqual(UserServiceAuthorization.Admin.ToString(), authorization, "Fail if the user authorization was not set as expected.");
		}

		[TestMethod]
		public void AddOrUpdateServiceUserAuth_settoguest()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			sqlTenantDatabaseHelper.AddOrUpdateUserServiceAuthorization("user1", UserServiceAuthorization.Guest);

			DbDataReader reader = sqlTenantDatabaseHelper.GetUserServiceAuthorization("user1");
			Assert.IsNotNull(reader);
			string authorization = UserServiceAuthorization.Admin.ToString();
			int recordCount = 0;
			while (reader.Read())
			{
				recordCount++;
				authorization = reader["Role"].ToString();
			}
			Assert.AreEqual(1, recordCount, "Fail if there is not one record for the user.");
			Assert.AreEqual(UserServiceAuthorization.Guest.ToString(), authorization, "Fail if the user authorization was not set as expected.");
		}

		[TestMethod]
		public void GetUserServiceAuth_userdoesnotexist()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);

			DbDataReader reader = sqlTenantDatabaseHelper.GetUserServiceAuthorization("nonexistentuser");
			Assert.IsNotNull(reader);
			int recordCount = 0;
			while (reader.Read())
			{
				recordCount++;
			}
			Assert.AreEqual(0, recordCount, "Fail if anything was returned for non-existent user.");
		}

		[TestMethod]
		public void AddOrUpdateTenantProfile()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = "testtenant1",
				Name = "test tenant",
				OwnerId = "user1"
			};
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantId = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if the wrong tenant is returned.");
			reader = sqlTenantDatabaseHelper.GetTenants("user1");
			tenantId = string.Empty;
			string userRole = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
				userRole = reader["Role"].ToString().Trim();
				break;
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if GetTenants does not return testtenant1 for user1");
			Assert.AreEqual("Admin", userRole, "Fail if the owner is not assigned appropriate authorization.");

			reader = sqlTenantDatabaseHelper.GetTenantConfiguration("testtenant1");
			string props = string.Empty;
			while (reader.Read())
			{
				props = reader["PropertyJson"].ToString().Trim();
			}
			Assert.AreEqual("{}", props, "Fail if the tenant configuration did not get set.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddOrUpdateTenantProfile_emptyTenantId()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile() { Name = "test tenant", OwnerId = "user1" };
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantId = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if the wrong tenant is returned.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddOrUpdateTenantProfile_noTenantName()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "testtenant1", OwnerId = "user1" };
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantId = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if the wrong tenant is returned.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddOrUpdateTenantProfile_noOwnerId()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "testtenant1", Name = "test tenant" };
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantId = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if the wrong tenant is returned.");
		}

		[TestMethod]
		public void AddOrUpdateTenantProfile_changetenantName()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = "testtenant1",
				Name = "test tenant",
				OwnerId = "user1",
			};
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			tenantProfile.Name = "change name";
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantName = string.Empty;
			while (reader.Read())
			{
				tenantName = reader["Name"].ToString().Trim();
			}
			Assert.AreEqual("change name", tenantName, "Fail if the wrong tenant is returned.");
		}

		[TestMethod]
		public void AddOrUpdateTenantProfile_changeowner()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = "testtenant1",
				Name = "test tenant",
				OwnerId = "user1",
			};
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			tenantProfile.OwnerId = "user2";
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants("user2");
			Assert.IsNotNull(reader);
			string user2Tenant = string.Empty;
			while (reader.Read())
			{
				user2Tenant = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", user2Tenant, "Fail if the tenant is not assigned to user2.");
		}

		[TestMethod]
		public void DeleteProfile_thathasnoprojects()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = "testtenant1",
				Name = "test tenant",
				OwnerId = "user1",
			};
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantId = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if the tenant was not created.");

			sqlTenantDatabaseHelper.DeleteTenantProfile(tenantProfile.TenantId);

			reader = sqlTenantDatabaseHelper.GetTenantProfile("testtenant1");
			int profileCount = 0;
			while (reader.Read())
			{
				profileCount++;
			}
			Assert.AreEqual(0, profileCount, "Fail if there was a tenant profile returned after deleting the tenant.");
			reader = sqlTenantDatabaseHelper.GetTenantConfiguration("testtenant1");
			int configCount = 0;
			while (reader.Read())
			{
				configCount++;
			}
			Assert.AreEqual(0, configCount, "Fail if there was a tenant configuration row returned after deleting the tenant.");
			reader = sqlTenantDatabaseHelper.GetUserTenantAuthorization("testtenant1", "user1");
			int userAuthCount = 0;
			while (reader.Read())
			{
				userAuthCount++;
			}
			Assert.AreEqual(0, userAuthCount, "Fail if there was a user authorization row left for the owner after deleting the tenant.");
			reader = sqlTenantDatabaseHelper.GetProjects(tenantProfile.TenantId);
			int projectCount = 0;
			while (reader.Read())
			{
				projectCount++;
			}
			Assert.AreEqual(0, projectCount, "Fail if there were any projects assigned to the tenant after deleting the tenant.");

		}

		[TestMethod]
		public void DeleteProfile_thathasprojects()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = "testtenant1",
				Name = "test tenant",
				OwnerId = "user1"
			};
			sqlTenantDatabaseHelper.AddOrUpdateTenantProfile(tenantProfile);
			sqlTenantDatabaseHelper.AddOrUpdateProject(tenantProfile.TenantId, new Project() { ProjectId = "project1", Name = "project 1", OwnerId = "user1", SimilarityThreshold = 0.5f, TenantId = tenantProfile.TenantId });

			DbDataReader reader = sqlTenantDatabaseHelper.GetTenants();
			Assert.IsNotNull(reader);
			string tenantId = string.Empty;
			while (reader.Read())
			{
				tenantId = reader["TenantId"].ToString().Trim();
			}
			Assert.AreEqual("testtenant1", tenantId, "Fail if the tenant was not created.");

			sqlTenantDatabaseHelper.DeleteTenantProfile(tenantProfile.TenantId);

			reader = sqlTenantDatabaseHelper.GetTenantProfile("testtenant1");
			int profileCount = 0;
			while (reader.Read())
			{
				profileCount++;
			}
			Assert.AreEqual(0, profileCount, "Fail if there was a tenant profile returned after deleting the tenant.");
			reader = sqlTenantDatabaseHelper.GetTenantConfiguration("testtenant1");
			int configCount = 0;
			while (reader.Read())
			{
				configCount++;
			}
			Assert.AreEqual(0, configCount, "Fail if there was a tenant configuration row returned after deleting the tenant.");
			reader = sqlTenantDatabaseHelper.GetUserTenantAuthorization("testtenant1", "user1");
			int userAuthCount = 0;
			while (reader.Read())
			{
				userAuthCount++;
			}
			Assert.AreEqual(0, userAuthCount, "Fail if there was a user authorization row left for the owner after deleting the tenant.");
			reader = sqlTenantDatabaseHelper.GetProjects(tenantProfile.TenantId);
			int projectCount = 0;
			while (reader.Read())
			{
				projectCount++;
			}
			Assert.AreEqual(0, projectCount, "Fail if there were any projects assigned to the tenant after deleting the tenant.");

		}

		[TestMethod]
		public void GetProjects_onlygetsforspecifiedtenant()
		{
			SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(SqlConfig);
			sqlTenantDatabaseHelper.AddOrUpdateProject("tenant1", new Project() { TenantId = "tenant1", ProjectId = "project1", Name = "project one", OwnerId = "user1", SimilarityThreshold = 0.5f });
			sqlTenantDatabaseHelper.AddOrUpdateProject("tenant2", new Project() { TenantId = "tenant2", ProjectId = "project1", Name = "project one", OwnerId = "user1", SimilarityThreshold = 0.5f });

			DbDataReader reader = sqlTenantDatabaseHelper.GetProjects("tenant1");
			Assert.IsNotNull(reader);
			List<string> projects = new List<string>();
			while (reader.Read())
			{
				projects.Add(reader["ProjectId"].ToString().Trim());
			}
			Assert.AreEqual(1, projects.Count, "Fail if there is less or more than 1 project returned.");
			Assert.AreEqual("project1", projects[0], "Fail if the returned project id is different than expected.");
		}
	}
}