using DupIQ.IssueIdentity;
using System.Data;
using System.Text.Json;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	[TestClass]
	public class SqlTenantManagerUnitTests
	{

		[TestMethod]
		public void AddOrUpdateProjectExtendedProperties()
		{
			PropStuffer passedInProp = null;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideAddOrUpdateProjectExtendedProperties = (s1, s2, p) => { passedInProp = p; };
			var tenantManager = new SqlTenantManager(databaseHelper);

			tenantManager.AddOrUpdateProjectExtendedProperties("tenant1", "project1", new PropStuffer());
			Assert.IsNotNull(passedInProp, "Fail if the properties were not passed to the call to write to the db.");
		}

		[TestMethod]
		public void AddProject()
		{
			string passedInProfile = string.Empty;
			Project passedInProject = null;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideAddOrUpdateProject = (t, p) => { passedInProfile = t; passedInProject = p; };

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			sqlTenantManager.AddProject(tenantProfile.TenantId, new Project() { ProjectId = "project1" });
			Assert.AreEqual("tenant1", passedInProfile, "Fail if tenantid passed in is not expected value.");
			Assert.IsNotNull(passedInProject, "Fail if the project was not passed in.");
		}

		[TestMethod]
		public void AddProjectForUser()
		{
			string passedInTenantId = string.Empty;
			string passedInUserId = string.Empty;
			string passedInProjectId = string.Empty;
			PropStuffer passedInProp = null;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideAddProjectForUser = (u, p, t) => { passedInTenantId = t; passedInUserId = u; passedInProjectId = p; };
			var tenantManager = new SqlTenantManager(databaseHelper);

			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			tenantManager.AddProjectForUser(tenantProfile.TenantId, "user1", "project1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the wrong tenantid is passed in.");
			Assert.AreEqual("user1", passedInUserId, "Fail if the wrong userid is passed in.");
			Assert.AreEqual("project1", passedInProjectId, "Fail if the wrong projectid is passed in.");
		}

		[TestMethod]
		public void AddTenant()
		{
			TenantProfile passedInProfile = null;
			PropStuffer passedInProp = null;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideAddOrUpdateTenantProfile = (t) => { passedInProfile = t; };

			var tenantManager = new SqlTenantManager(databaseHelper);

			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			tenantManager.AddTenant(tenantProfile);
			Assert.AreEqual("tenant1", passedInProfile.TenantId, "Fail if the passed in profile does not match expected.");
		}

		[TestMethod]
		public void AddTenantProfileToUserIdentityTenantList()
		{
			string passedInProfileId = string.Empty;
			string passedInUserId = string.Empty;

			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideAddOrUpdateTenantProfileToUserProfileList = (t, u, n, e, a) => { passedInProfileId = t; passedInUserId = u; };

			var tenantManager = new SqlTenantManager(databaseHelper);

			tenantManager.AddTenantProfileToUserIdentityTenantList("tenant1", "user1", "user 1", "user@email.com", UserTenantAuthorization.Admin);

			Assert.AreEqual("tenant1", passedInProfileId, "Fail if the passed in profile does not match expected.");
			Assert.AreEqual("user1", passedInUserId, "Fail if the tenant is assigned to the wrong user.");
		}

		[TestMethod]
		public void AddUserTenantAuthorization()
		{
			string passedInTenantId = string.Empty;
			string passedInUserId = string.Empty;
			UserTenantAuthorization passedInAuth = UserTenantAuthorization.None;

			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideAddOrUpdateUserTenantAuthorization = (t, u, a) => { passedInTenantId = t; passedInUserId = u; passedInAuth = a; };

			var tenantManager = new SqlTenantManager(databaseHelper);

			tenantManager.AddUserTenantAuthorization("tenant1", "user1", UserTenantAuthorization.Reader);
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the passed in profile does not match expected.");
			Assert.AreEqual("user1", passedInUserId, "Fail if the tenant is assigned to the wrong user.");
			Assert.AreEqual(UserTenantAuthorization.Reader, passedInAuth, "Fail if the passed in authorization is not expected.");
		}

		[TestMethod]
		public void DeleteTenant()
		{
			string passedInTenantId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideDeleteTenantProfile = (t) => { passedInTenantId = t; };

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			sqlTenantManager.DeleteTenant(tenantProfile.TenantId);
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant id was not passed in.");
		}

		[TestMethod]
		public void GenerateAPIKey()
		{
			TenantProfile passedInProfile = null;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1", ApiKey = "should overwrite this key" };
			string key = sqlTenantManager.GenerateApiKey(tenantProfile.TenantId);
			Assert.IsFalse(string.IsNullOrEmpty(key), "Fail if the API key is null.");
			key = sqlTenantManager.GenerateApiKey("tenant1", "project1");
			Assert.IsFalse(string.IsNullOrEmpty(key), "Fail if the API key is null - tenant and project.");
		}

		[TestMethod]
		public void GetProject()
		{
			string passedInTenantId = string.Empty;
			string passedInProjectId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetProject = (t, p) => { passedInTenantId = t; passedInProjectId = p; return new Project() { ProjectId = p }; };

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			Project result = sqlTenantManager.GetProject(tenantProfile.TenantId, "project1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("project1", passedInProjectId, "Fail if the project id was not passed in.");
			Assert.AreEqual("project1", result.ProjectId, "Fail if the returned project was not expected.");
		}

		[TestMethod]
		public void GetProject_OnlyGetsForSpecifiedTenant()
		{
			string passedInTenantId = string.Empty;
			string passedInProjectId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetProject = (t, p) => { passedInTenantId = t; passedInProjectId = p; return new Project() { ProjectId = p }; };

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			Project result = sqlTenantManager.GetProject("tenant1", "project1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("project1", passedInProjectId, "Fail if the project id was not passed in.");
			Assert.AreEqual("project1", result.ProjectId, "Fail if the returned project was not expected.");
		}

		[TestMethod]
		public void GetProjectExtendedProperties()
		{
			string passedInTenantId = string.Empty;
			string passedInProjectId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetProjectExtendedProperties = (t, p) =>
			{ passedInTenantId = t; passedInProjectId = p; PropStuffer propStuffer = new PropStuffer(); propStuffer.AddProperties("desired property"); return propStuffer; };

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			Project project = new Project() { TenantId = "tenant1", ProjectId = "project1" };
			string result = sqlTenantManager.GetProjectExtendedProperties<string>(project.TenantId, project.ProjectId);
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("project1", passedInProjectId, "Fail if the project id was not passed in.");
			Assert.AreEqual("desired property", result, "Fail if the properties are not as expected.");
		}

		[TestMethod]
		public void GetProjectExtendedPropertyTypeNames()
		{
			string passedInTenantId = string.Empty;
			string passedInProjectId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetProjectExtendedProperties = (t, p) =>
			{ passedInTenantId = t; passedInProjectId = p; PropStuffer propStuffer = new PropStuffer(); propStuffer.AddProperties("desired property"); return propStuffer; };

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			Project project = new Project() { TenantId = "tenant1", ProjectId = "project1" };
			string[] typeNames = sqlTenantManager.GetProjectExtendedPropertyTypeNames(project.TenantId, project.ProjectId);
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("project1", passedInProjectId, "Fail if the project id was not passed in.");
			Assert.AreEqual(1, typeNames.Count(), "Fail if we did not get back expected number of type names.");
			Assert.AreEqual("String", typeNames[0], "Fail if we did not get back the expected property type name.");
		}

		[TestMethod]
		public void GetProjects()
		{
			string passedInTenantId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetProjects = (t) =>
			{
				passedInTenantId = t;
				DataTable table = CreateProjectDataTable();

				table.Rows.Add(t, "project1", "user1", "tenant 1", DateTime.Now);
				return table.CreateDataReader();
			};


			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			Project[] projects = sqlTenantManager.GetProjects("tenant1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual(1, projects.Count(), "Fail if we did not get back expected number of projects.");
			Assert.AreEqual("project1", projects[0].ProjectId, "Fail if we did not get back the expected project.");
		}

		[TestMethod]
		public void GetProjects_forUserId()
		{
			string passedInTenantId = string.Empty;
			string passedInUserId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetProjects_UserId = (t, u) =>
			{
				passedInTenantId = t;
				passedInUserId = u;
				DataTable table = CreateProjectDataTable();

				table.Rows.Add(t, "project1", u, "tenant 1", DateTime.Now);
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = new TenantProfile() { TenantId = "tenant1" };
			Project[] projects = sqlTenantManager.GetProjects(tenantProfile.TenantId, "user1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("user1", passedInUserId, "Fail if the userid was not passed in.");
			Assert.AreEqual(1, projects.Count(), "Fail if we did not get back expected number of projects.");
			Assert.AreEqual("project1", projects[0].ProjectId, "Fail if we did not get back the expected project.");
		}

		[TestMethod]
		public void GetTenantConfiguration()
		{
			string passedInTenantId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetTenantConfiguration = (s) =>
			{
				passedInTenantId = s;
				DataTable table = CreateTenantConfigurationsTable();
				Dictionary<string, string> configValues = new Dictionary<string, string>();
				configValues.Add("config1", "value1");
				string configJson = JsonSerializer.Serialize(configValues);
				table.Rows.Add(s, configJson);
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantConfiguration config = sqlTenantManager.GetTenantConfiguration("tenant1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("tenant1", config.TenantId, "Fail if the configuration.tenantid does not match.");
		}

		[TestMethod]
		public void GetTenantProfile()
		{
			string passedInTenantId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetTenantProfile = (s) =>
			{
				passedInTenantId = s;
				DataTable table = CreateTenantProfileTable();

				table.Rows.Add("tenant1", "tenant 1", "user1", "backupuser1", DateTime.Now);
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile tenantProfile = sqlTenantManager.GetTenantProfile("tenant1");
			Assert.AreEqual("tenant1", passedInTenantId, "Fail if the tenant was not passed in.");
			Assert.AreEqual("tenant1", tenantProfile.TenantId, "Fail if the wrong tenant was returned.");
			Assert.AreEqual("user1", tenantProfile.OwnerId, "Fail if the user identity is not as expected.");
		}

		[TestMethod]
		public void GetTenants()
		{
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetTenants = () =>
			{
				DataTable table = CreateTenantProfileTable();
				table.Rows.Add("tenant1");
				table.Rows.Add("tenant2");
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			string[] tenantIds = sqlTenantManager.GetTenants();
			Assert.AreEqual(2, tenantIds.Count(), "Fail if expected number of tenantIds was not returned.");
			Assert.AreEqual("tenant1", tenantIds[0], "Fail if first tenantid is not as expected.");
			Assert.AreEqual("tenant2", tenantIds[1], "Fail if first tenantid is not as expected.");
		}

		[TestMethod]
		public void GetTenants_userId()
		{
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetTenants_UserId = (u) =>
			{
				DataTable table = CreateTenantProfileTable();
				table.Rows.Add("tenant1", "tenant 1", "owner1");
				table.Rows.Add("tenant2", "tenant 2", "owner2");
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			TenantProfile[] tenants = sqlTenantManager.GetTenants("user1");
			Assert.AreEqual(2, tenants.Count(), "Fail if expected number of tenantIds was not returned.");
			Assert.AreEqual("tenant1", tenants[0].TenantId, "Fail if first tenantid is not as expected.");
			Assert.AreEqual("owner1", tenants[0].OwnerId, "Fail if first ownerid is not as expected.");
			Assert.AreEqual("tenant2", tenants[1].TenantId, "Fail if first tenantid is not as expected.");
			Assert.AreEqual("owner2", tenants[1].OwnerId, "Fail if second ownerid is not as expected.");
		}

		[TestMethod]
		public void GetUserTenantAuthorization()
		{
			string passedInTenatId = string.Empty;
			string passedInUserId = string.Empty;
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetUserTenantAuthorization = (t, u) =>
			{
				passedInTenatId = t;
				passedInUserId = u;
				DataTable table = CreateUserTenantAuthorizationTable();

				table.Rows.Add(t, u, "Admin");
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			UserTenantAuthorization auth = sqlTenantManager.GetUserTenantAuthorization("tenant1", "user1");
			Assert.AreEqual("tenant1", passedInTenatId, "Fail if the tenantid was not passed in.");
			Assert.AreEqual("user1", passedInUserId, "Fail if the userid was not passed in.");
			Assert.AreEqual(UserTenantAuthorization.Admin, auth, "Fail if the authorization was not expected value.");
			databaseHelper.overrideGetUserTenantAuthorization = (t, u) =>
			{
				DataTable table = CreateUserTenantAuthorizationTable();

				table.Rows.Add(t, u, "Developer");
				return table.CreateDataReader();
			};
			auth = sqlTenantManager.GetUserTenantAuthorization("tenant1", "user1");
			Assert.AreEqual(UserTenantAuthorization.Developer, auth, "Fail if the authorization was not expected value.");
			databaseHelper.overrideGetUserTenantAuthorization = (t, u) =>
			{
				DataTable table = CreateUserTenantAuthorizationTable();

				table.Rows.Add(t, u, "Writer");
				return table.CreateDataReader();
			};
			auth = sqlTenantManager.GetUserTenantAuthorization("tenant1", "user1");
			Assert.AreEqual(UserTenantAuthorization.Writer, auth, "Fail if the authorization was not expected value.");
			databaseHelper.overrideGetUserTenantAuthorization = (t, u) =>
			{
				DataTable table = CreateUserTenantAuthorizationTable();

				table.Rows.Add(t, u, "Reader");
				return table.CreateDataReader();
			};
			auth = sqlTenantManager.GetUserTenantAuthorization("tenant1", "user1");
			Assert.AreEqual(UserTenantAuthorization.Reader, auth, "Fail if the authorization was not expected value.");
		}

		[TestMethod]
		public void GetUserTenantAuthorization_userhasnoauth()
		{
			MockSqlTenantDatabaseHelper databaseHelper = new MockSqlTenantDatabaseHelper();
			databaseHelper.overrideGetUserTenantAuthorization = (t, u) =>
			{
				DataTable table = CreateUserTenantAuthorizationTable();
				return table.CreateDataReader();
			};

			SqlTenantManager sqlTenantManager = new SqlTenantManager(databaseHelper);
			UserTenantAuthorization auth = sqlTenantManager.GetUserTenantAuthorization("tenant1", "user1");
			Assert.AreEqual(UserTenantAuthorization.None, auth, "Fail if the user has any authorization other than None because there were no entries for the user.");
		}



		private static DataTable CreateUserTenantAuthorizationTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("TenantId", typeof(string));
			table.Columns.Add("UserId", typeof(string));
			table.Columns.Add("Role", typeof(string));
			return table;
		}

		private static DataTable CreateTenantProfileTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("TenantId", typeof(string));
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("OwnerId", typeof(string));
			table.Columns.Add("BackupOwnerId", typeof(string));
			table.Columns.Add("CreatedDate", typeof(DateTime));
			return table;
		}

		private static DataTable CreateTenantConfigurationsTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("TenantId", typeof(string));
			table.Columns.Add("PropertyJson", typeof(string));
			return table;
		}

		private static DataTable CreateProjectDataTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("TenantId", typeof(string));
			table.Columns.Add("ProjectId", typeof(string));
			table.Columns.Add("OwnerId", typeof(string));
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("CreatedDate", typeof(DateTime));
			return table;
		}
	}

}