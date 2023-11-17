using DupIQ.IssueIdentity;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public class SqlIssueIdentityUserManager : IUserManager
	{
		private ISqlUserDbHelper sqlUserDbHelper;

		public SqlIssueIdentityUserManager(ISqlUserDbHelper sqlUserDbHelper)
		{
			this.sqlUserDbHelper = sqlUserDbHelper;
		}

		public string AddOrUpdateUser(IssueIdentityUser user)
		{
			bool nameChangeOrInitialize = true;
			if (string.IsNullOrEmpty(user.Id) || !sqlUserDbHelper.UserExists(user.Id))
			{
				user.Id = Guid.NewGuid().ToString();
			}
			else
			{
				using (DbDataReader reader = sqlUserDbHelper.GetUser(user.Id))
				{
					while (reader.Read())
					{
						if (user.Name.Equals(reader["UserName"].ToString()))
						{
							nameChangeOrInitialize = false;
						}
					}
				}
			}
			if (nameChangeOrInitialize)
			{
				if (!sqlUserDbHelper.UserNameAvailable(user.Name))
				{
					throw new UserNameUnavailableException(user.Name);
				}
			}
			sqlUserDbHelper.AddOrUpdateUser(user);
			return user.Id;
		}

		public void DeleteUser(string userId)
		{
			if (!sqlUserDbHelper.UserExists(userId))
			{
				throw new UserDoesNotExistException(userId);
			}
			sqlUserDbHelper.DeleteUser(userId);
		}

		public bool Exists(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				throw new ArgumentException();
			}
			return sqlUserDbHelper.UserExists(userId);
		}

		public IssueIdentityUser GetUserById(string id)
		{
			using (DbDataReader reader = sqlUserDbHelper.GetUser(id))
			{
				IssueIdentityUser user = new IssueIdentityUser();
				while (reader.Read())
				{
					user.Id = reader["UserId"].ToString().Trim();
					user.Name = reader["UserName"].ToString().Trim();
					user.FirstName = reader["FirstName"].ToString().Trim();
					user.LastName = reader["LastName"].ToString().Trim();
					user.Email = reader["Email"].ToString().Trim();
					return user;
				}
			}
			throw new UserDoesNotExistException(id);
		}

		public IssueIdentityUser GetUserByName(string userName)
		{
			throw new NotImplementedException();
		}
	}
}
