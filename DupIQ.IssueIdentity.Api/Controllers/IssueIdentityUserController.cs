using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;

namespace DupIQ.IssueIdentityAPI.Controllers
{
	[Microsoft.AspNetCore.Mvc.Route("[controller]")]
	[ApiController]
	public class IssueIdentityUserController : IssueIdentityControllerBaseClass
	{
		ILogger<IssueIdentityUserController> logger;
		IConfiguration _configuration;
		public IssueIdentityUserController(IConfiguration configuration, ILogger<IssueIdentityUserController> logger)
		{
			this.logger = logger;
			this._configuration = configuration;
			GlobalConfiguration.InitializeConfiguration(_configuration, logger);
		}

		[Microsoft.AspNetCore.Mvc.HttpGet]
		public IssueIdentityUser Get(string userId)
		{
			return GlobalConfiguration.UserManager.GetUserById(userId);
		}

		[Microsoft.AspNetCore.Mvc.HttpPost]
		public string AddUser(IssueIdentityUser user)
		{
			return GlobalConfiguration.UserManager.AddOrUpdateUser(user);

		}

		[Microsoft.AspNetCore.Mvc.HttpDelete]
		public void DeleteUser(string userId)
		{
			GlobalConfiguration.UserManager.DeleteUser(userId);
		}
	}
}
