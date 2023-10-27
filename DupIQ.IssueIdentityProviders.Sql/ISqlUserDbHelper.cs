using DupIQ.IssueIdentity;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
