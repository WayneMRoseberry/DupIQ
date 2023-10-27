using DupIQ.IssueIdentity;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public interface ISqlUserDbHelper
	{
		void AddOrUpdateUser(IssueIdentityUser user);
		void DeleteUser(string userId);
		DbDataReader GetUser(string userId);
		bool UserExists(string userId);
		bool UserNameAvailable(string userName);
		void ConfigureDatabase();
	}
}
