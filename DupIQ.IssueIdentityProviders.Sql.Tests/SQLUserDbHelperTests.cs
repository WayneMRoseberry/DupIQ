using DupIQ.IssueIdentity;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	[TestClass]
	public class SQLUserDbHelperTests
	{
		SqlIOHelperConfig sqlIOHelperConfig;

		[TestInitialize]
		public void Init()
		{
			var builder = new ConfigurationBuilder().AddUserSecrets<SqlTenantDatabaseHelperTests>();
			var thing = builder.Build();
			sqlIOHelperConfig = thing.GetSection("SqlIOHelperConfig").Get<SqlIOHelperConfig>();
		}

		[TestMethod]
		public void AddOrUpdateUser()
		{
			SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlIOHelperConfig);
			try
			{
				sqlUserDbHelper.AddOrUpdateUser(new IssueIdentityUser()
				{
					Id = "testuser",
					Name = "test user",
					FirstName = "test",
					LastName = "user",
					Email = "user@email.com",
					Userstatus = IssueIdentityUserStatus.Active
				});
				using (DbDataReader reader = sqlUserDbHelper.GetUser("testuser"))
				{
					int resultcounter = 0;
					string userId = string.Empty;
					while (reader.Read())
					{
						userId = reader["UserId"].ToString().Trim();
						resultcounter++;
					}
					Assert.AreEqual(1, resultcounter, "Fail if there is anything other than 1 user returned.");
					Assert.AreEqual("testuser", userId, "Fail if the userid was not returned.");
				}
			}
			finally
			{
				sqlUserDbHelper.DeleteUser("testuser");
			}

		}

		[TestMethod]
		public void AddOrUpdateUser_useralreadyexists()
		{
			SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlIOHelperConfig);
			IssueIdentityUser user = new IssueIdentityUser()
			{
				Id = "testuser",
				Name = "test user",
				FirstName = "test",
				LastName = "user",
				Email = "user@email.com",
				Userstatus = IssueIdentityUserStatus.Active
			};

			try
			{
				sqlUserDbHelper.AddOrUpdateUser(user);
				using (DbDataReader reader = sqlUserDbHelper.GetUser("testuser"))
				{
					string userId = string.Empty;
					string userName = string.Empty;
					while (reader.Read())
					{
						userId = reader["UserId"].ToString().Trim();
						userName = reader["UserName"].ToString().Trim();
					}
					Assert.AreEqual("testuser", userId, "Checking if the user was created so subsequent adds are against existing user.");
					Assert.AreEqual("test user", userName, "Checking if the user was created so subsequent adds are against existing user.");
				}

				user.Name = "changed my name";

				sqlUserDbHelper.AddOrUpdateUser(user);
				using (DbDataReader reader = sqlUserDbHelper.GetUser("testuser"))
				{
					int resultcounter = 0;
					string userName = string.Empty;
					while (reader.Read())
					{
						userName = reader["UserName"].ToString().Trim();
						resultcounter++;
					}
					Assert.AreEqual("changed my name", userName, "Fail if the user name was not changed as expected.");
				}
			}
			finally
			{
				sqlUserDbHelper.DeleteUser(user.Id);
			}
		}

		[TestMethod]
		public void UserExists()
		{
			SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlIOHelperConfig);
			try
			{
				sqlUserDbHelper.AddOrUpdateUser(new IssueIdentityUser()
				{
					Id = "existinguser",
					Name = "test user",
					FirstName = "test",
					LastName = "user",
					Email = "user@email.com",
					Userstatus = IssueIdentityUserStatus.Active
				});

				Assert.IsTrue(sqlUserDbHelper.UserExists("existinguser"), "Fail if the user does not exist.");
			}
			finally
			{
				sqlUserDbHelper.DeleteUser("existinguser");
			}
		}

		[TestMethod]
		public void UserExists_userdoesnotexist()
		{
			SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlIOHelperConfig);
			Assert.IsFalse(sqlUserDbHelper.UserExists("nonexistinguser"), "Fail if the user exists.");
		}

		[TestMethod]
		public void UserNameAvailable()
		{
			SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlIOHelperConfig);
			Assert.IsTrue(sqlUserDbHelper.UserNameAvailable("test user"), "Fail if the user name is unavailable.");

		}

		[TestMethod]
		public void UserNameAvailable_isnotavailable()
		{
			SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlIOHelperConfig);
			try
			{
				sqlUserDbHelper.AddOrUpdateUser(new IssueIdentityUser()
				{
					Id = "existinguser",
					Name = "test user",
					FirstName = "test",
					LastName = "user",
					Email = "user@email.com",
					Userstatus = IssueIdentityUserStatus.Active
				});

				Assert.IsFalse(sqlUserDbHelper.UserNameAvailable("test user"), "Fail if the user name is available.");
			}
			finally
			{
				sqlUserDbHelper.DeleteUser("existinguser");
			}
		}


	}
}
