using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Web.Http;

namespace DupIQ.IssueIdentityAPI.Controllers
{
	[Microsoft.AspNetCore.Authorization.Authorize]
	[Microsoft.AspNetCore.Mvc.Route("[controller]")]
	[ApiController]
	public class TenantController : IssueIdentityControllerBaseClass
	{
		ILogger<TenantController> logger;
		IConfiguration _configuration;

		public TenantController(ILogger<TenantController> logger, IConfiguration configuration)
		{
			this.logger = logger;
			this._configuration = configuration;
			GlobalConfiguration.InitializeConfiguration(_configuration, logger);
		}

		[Microsoft.AspNetCore.Mvc.HttpPost("Project")]
		public void AddProject(string tenantId, Project project)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) && 
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Writer, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity	 as ClaimsIdentity))
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				GlobalConfiguration.TenantManager.AddProject(tenantId, project);
			}
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("Projects")]
		public IEnumerable<Project> GetProjects(string tenantId)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity	 as ClaimsIdentity), UserServiceAuthorization.Guest) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity))
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return null;
			}
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProjects(tenantProfile.TenantId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("ProjectsForUser")]
		public IEnumerable<Project> GetProjectsForUser(string tenantId, string userId)
		{
			var identity = HttpContext.User.Identity as ClaimsIdentity;
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(identity), UserServiceAuthorization.Guest) &&
				!CheckIfCurrentUserMatchesUserId(userId, identity) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Writer, GetHighestTenantRoleForIdentity(tenantId, identity)
				)
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return null;
			}
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProjects(tenantProfile.TenantId, userId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("Project")]
		public Project GetProject(string tenantId, string projectId)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity))
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return null;
			}
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProject(tenantProfile.TenantId, projectId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpPost("Tenant")]
		public string AddTenant(string tenantName, string ownerId)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return string.Empty;
			}
			string tenantId = System.Guid.NewGuid().ToString();
			TenantProfile tenantProfile = new TenantProfile()
			{
				TenantId = tenantId,
				Name = tenantName,
				OwnerId = ownerId,
				ApiKey = GlobalConfiguration.TenantManager.GenerateApiKey(tenantId)
			};
			return GlobalConfiguration.TenantManager.AddTenant(tenantProfile);
		}

		[Microsoft.AspNetCore.Mvc.HttpPost("Tenant/UserAuthorization")]
		public void AddTenantUserAuthorization(string tenantId, string userId, UserTenantAuthorization userTenantAuthorization)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Developer, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity))
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				GlobalConfiguration.TenantManager.AddUserTenantAuthorization(tenantId, userId, userTenantAuthorization);
			}
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("Tenant")]
		public TenantProfile GetTenant(string tenantId)
		{

			ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(identity), UserServiceAuthorization.Guest) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, identity))
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return null;
			}

			return GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
		}

		[Microsoft.AspNetCore.Mvc.HttpGet]
		public string[] GetTenants()
		{
			if(CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.None))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return new string[] { };
			}
			return GlobalConfiguration.TenantManager.GetTenants();
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("Tenant/UserAuthorization")]
		public UserTenantAuthorization GetTenantUserAuthorization(string tenantId, string userId)
		{
			var identity = HttpContext.User.Identity	 as ClaimsIdentity;
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
				!CheckIfCurrentUserMatchesUserId(userId, identity) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Writer, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity))
				)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return UserTenantAuthorization.None;
			}
			return GlobalConfiguration.TenantManager.GetUserTenantAuthorization(tenantId, userId);
		}
	}
}
