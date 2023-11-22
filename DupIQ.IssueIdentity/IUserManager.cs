namespace DupIQ.IssueIdentity
{
	public interface IUserManager
	{
		/// <summary>
		/// Add user to the system.
		/// </summary>
		/// <param name="user">User to add. UserName property must be unique. UserId will be overwritten by IUserManager if user does not exist. UserId cannot be changed after user is created.</param>
		/// <returns>UserId of the added or updated user.</returns>
		public string AddOrUpdateUser(IssueIdentityUser user);

		public void AddOrUpdateUserPasswordHash(string userId, string passwordHash);
		
		/// <summary>
		/// Deletes specified user.
		/// </summary>
		/// <param name="userId">UserId of the user to delete.</param>
		public void DeleteUser(string userId);

		/// <summary>
		/// Check if user exists already.
		/// </summary>
		/// <param name="userId">UserId property of the user to check for.</param>
		/// <returns>True if the user already exists, false if not.</returns>
		public bool Exists(string userId);
		
		/// <summary>
		/// Gets the user based on their user name.
		/// </summary>
		/// <param name="userName">UserName property of the user to get.</param>
		/// <returns>IssueIdentityUser object identifying the user.</returns>
		public IssueIdentityUser GetUserByName(string userName);

		/// <summary>
		/// Gets the user based on their user id.
		/// </summary>
		/// <param name="id">UserId property of the user to get.</param>
		/// <returns>IssueIdentityUser object identifying the user.</returns>
		public IssueIdentityUser GetUserById(string  id);

		public string GetUserPasswordHash(string  id);
	}
	
	public class IssueIdentityUserException : Exception
	{
		public string UserName { get; private set; }
		public IssueIdentityUserException(string userName)
		{
			UserName = userName;
		}
	}

	public class UserNameUnavailableException : IssueIdentityUserException
	{
		public UserNameUnavailableException(string userName) : base(userName)
		{
		}
	}

	public class UserDoesNotExistException : IssueIdentityUserException
	{
		public UserDoesNotExistException(string userName) : base(userName)
		{
		}
	}

}
