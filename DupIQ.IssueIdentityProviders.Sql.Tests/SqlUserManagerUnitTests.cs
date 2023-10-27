using DupIQ.IssueIdentity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupIQ.IssueIdentityProviders.Sql.Tests
{
	[TestClass]
	public class SqlUserManagerUnitTests
	{
		[TestMethod]
		public void AddUser()
		{
			string userId = string.Empty;
			string userName = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideAddUser = (u) => { userId = u.Id; userName = u.Name; };
			mockSqlUserDbHelper.overrideUserExists = (u) => { return false; };
			mockSqlUserDbHelper.overrideUserNameAvailable = (u) => { return true; };
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.AddOrUpdateUser(new IssueIdentityUser() { Name = "user1", Id = "change this" });
			Assert.AreEqual("user1", userName, "fail if the user name was not passed in.");
			Assert.AreNotEqual("change this", userId, "Fail if the user id is not assigned a new id.");
		}

		[TestMethod]
		public void AddUser_useridalreadyexistsnonamechange()
		{
			string userId = string.Empty;
			string userName = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideUserExists = (u) => { return true; };
			mockSqlUserDbHelper.overrideGetUser = (u) =>
			{
				DataTable table = CreateIssueIdentityUserDataTable();
				table.Rows.Add("user1", "Imayalreadybehere", "user", "one", DateTime.Now, IssueIdentityUserStatus.Active.ToString());
				return table.CreateDataReader();
			};
			mockSqlUserDbHelper.overrideAddUser = (u) => { userId = u.Id; userName = u.Name; };

			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.AddOrUpdateUser(new IssueIdentityUser() { Id = "user1", Name = "Imayalreadybehere" });
			Assert.AreEqual("user1", userId, "Fail if the user id was changed on the way in.");
			Assert.AreEqual("Imayalreadybehere", userName, "Fail if the user name was not set on the way in");
		}

		[TestMethod]
		public void AddUser_useridalreadyexistswithnamechangeandnameavailable()
		{
			string userNameAvailableCheck = string.Empty;
			string userId = string.Empty;
			string userName = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideUserExists = (u) => { return true; };
			mockSqlUserDbHelper.overrideUserNameAvailable = (u) => { userNameAvailableCheck = u; return true; };
			mockSqlUserDbHelper.overrideGetUser = (u) =>
			{
				DataTable table = CreateIssueIdentityUserDataTable();
				table.Rows.Add("user1", "Imayalreadybehere", "user", "one", DateTime.Now, IssueIdentityUserStatus.Active.ToString());
				return table.CreateDataReader();
			};
			mockSqlUserDbHelper.overrideAddUser = (u) => { userId = u.Id; userName = u.Name; };

			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.AddOrUpdateUser(new IssueIdentityUser() { Id = "user1", Name = "Iamchangingmyname" });
			Assert.AreEqual("Iamchangingmyname", userNameAvailableCheck, "Fail if the availability of user name was never checked.");
			Assert.AreEqual("user1", userId, "Fail if the user id was changed on the way in.");
			Assert.AreEqual("Iamchangingmyname", userName, "Fail if the new changed name was not passed in to update the user.");
		}

		[TestMethod]
		[ExpectedException(typeof(UserNameUnavailableException))]
		public void AddUser_useridalreadyexistswithnamechangeandnamenotavailable()
		{
			string userNameAvailableCheck = string.Empty;
			string userId = string.Empty;
			string userName = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideUserExists = (u) => { return true; };
			mockSqlUserDbHelper.overrideUserNameAvailable = (u) => { userNameAvailableCheck = u; return false; };
			mockSqlUserDbHelper.overrideGetUser = (u) =>
			{
				DataTable table = CreateIssueIdentityUserDataTable();
				table.Rows.Add("user1", "Imayalreadybehere", "user", "one", DateTime.Now, IssueIdentityUserStatus.Active.ToString());
				return table.CreateDataReader();
			};

			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.AddOrUpdateUser(new IssueIdentityUser() { Id = "user1", Name = "Iamchangingmyname" });
			Assert.AreEqual("Iamchangingmyname", userNameAvailableCheck, "Fail if the availability of user name was never checked.");
		}
		[TestMethod]
		[ExpectedException(typeof(UserNameUnavailableException))]
		public void AddUser_usernamealreadyexists()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideAddUser = (u) => { userId = u.Id; };
			mockSqlUserDbHelper.overrideUserExists = (u) => { return false; };
			mockSqlUserDbHelper.overrideUserNameAvailable = (u) => { return false; };
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.AddOrUpdateUser(new IssueIdentityUser() { Id = "user1", Name = "Imayalreadybehere" });
		}

		[TestMethod]
		public void DeleteUser()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideDeleteUser = (u) => { userId = u; };
			mockSqlUserDbHelper.overrideUserExists = (u) => { return true; };
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.DeleteUser("user1");
			Assert.AreEqual("user1", userId, "Fail if the userid was not passed to db helper.");
		}

		[TestMethod]
		[ExpectedException(typeof(UserDoesNotExistException))]
		public void DeleteUser_userdoesnotexist()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideDeleteUser = (u) => { userId = u; };
			mockSqlUserDbHelper.overrideUserExists = (u) => { return false; };
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.DeleteUser("user1");
		}

		[TestMethod]
		public void Exists()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideUserExists = (u) => { userId = u; return true; };
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			Assert.IsTrue(userManager.Exists("user1"), "Fail if the user manager does not return Exists == true.");
			Assert.AreEqual("user1", userId, "Fail if user manager did not pass correct userid to db helper.");
		}

		[TestMethod]
		public void Exists_doesnotexist()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideUserExists = (u) => { userId = u; return false; };
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			Assert.IsFalse(userManager.Exists("user1"), "Fail if the user manager does not return Exists == false.");
			Assert.AreEqual("user1", userId, "Fail if user manager did not pass correct userid to db helper.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Exists_emptyuserid()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			userManager.Exists(string.Empty);
		}

		[TestMethod]
		public void GetUserById()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideGetUser = (u) =>
			{
				DataTable dt = CreateIssueIdentityUserDataTable();
				dt.Rows.Add(u, "user 1", "firstname", "lastname", "emailname", DateTime.Now.ToString(), IssueIdentityUserStatus.Active.ToString());
				return dt.CreateDataReader();
			};
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			IssueIdentityUser result = userManager.GetUserById("user1");
			Assert.AreEqual("user 1", result.Name, "Fail if we did not get the expected user name.");
		}

		[TestMethod]
		[ExpectedException(typeof(UserDoesNotExistException))]
		public void GetUserById_usernotindatabase()
		{
			string userId = string.Empty;
			MockSqlUserDbHelper mockSqlUserDbHelper = new MockSqlUserDbHelper();
			mockSqlUserDbHelper.overrideGetUser = (u) =>
			{
				DataTable dt = CreateIssueIdentityUserDataTable();
				return dt.CreateDataReader();
			};
			SqlIssueIdentityUserManager userManager = new SqlIssueIdentityUserManager(mockSqlUserDbHelper);
			IssueIdentityUser result = userManager.GetUserById("user1");
		}

		private static DataTable CreateIssueIdentityUserDataTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("UserId", typeof(string));
			table.Columns.Add("UserName", typeof(string));
			table.Columns.Add("FirstName", typeof(string));
			table.Columns.Add("LastName", typeof(string));
			table.Columns.Add("Email", typeof(string));
			table.Columns.Add("CreateDate", typeof(string));
			table.Columns.Add("Status", typeof(string));
			return table;
		}

	}

	public class MockSqlUserDbHelper : ISqlUserDbHelper
	{
		public Action<IssueIdentityUser> overrideAddUser = (u) => { throw new NotImplementedException(); };
		public Action<string> overrideDeleteUser = (u) => { throw new NotImplementedException(); };
		public Func<string, DbDataReader> overrideGetUser = (u) => { throw new NotImplementedException(); };
		public Func<string, bool> overrideUserExists = (u) => { throw new NotImplementedException(); };
		public Func<string, bool> overrideUserNameAvailable = (u) => { throw new NotImplementedException(); };

		public void AddOrUpdateUser(IssueIdentityUser user)
		{
			overrideAddUser(user);
		}

		public void ConfigureDatabase()
		{
			throw new NotImplementedException();
		}

		public void DeleteUser(string userId)
		{
			overrideDeleteUser(userId);
		}

		public DbDataReader GetUser(string userId)
		{
			return overrideGetUser(userId);
		}

		public bool UserExists(string userId)
		{
			return overrideUserExists(userId);
		}

		public bool UserNameAvailable(string userName)
		{
			return overrideUserNameAvailable(userName);
		}
	}
}
