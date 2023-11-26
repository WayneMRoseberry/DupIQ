using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace DupIQ.IssueIdentityAPI
{
	public class IssueIdentityControllerBaseClass : ControllerBase
	{
		const string tenantRoleAdmin = "TenantRole_Admin";
		const string tenantRoleDeveloper = "TenantRole_Developer";
		const string tenantRoleWriter = "TenantRole_Writer";
		const string tenantRoleReader = "TenantRole_Reader";
		internal ILogger logger;

		//[FromHeader]
		public string ApiKey { get; set; }

		internal bool CheckIfBelowServiceAuthorizationLevel(UserServiceAuthorization serviceAuth, UserServiceAuthorization checkAgainst)
		{
			if (serviceAuth >= checkAgainst)
			{
				return true;
			}
			return false;
		}

		internal bool CheckIfCurrentUserMatchesUserId(string userId, ClaimsIdentity? identity)
		{
			string identityId = identity.FindFirst("userId").Value;
			this.logger.LogInformation($"CheckIfCurrentUserMatchesUserId: userId={userId}, currentUser={identityId}");
			bool idCompare = identityId.Equals(userId);
			return idCompare;
		}

		internal bool CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization roleLevel, UserTenantAuthorization authorization)
		{
			return authorization >= roleLevel;
		}

		internal UserTenantAuthorization GetHighestTenantRoleForIdentity(string tenantId, ClaimsIdentity? identity)
		{
			UserTenantAuthorization authorization = UserTenantAuthorization.None;
			string userId = identity.FindFirst("userId").Value.ToString();
			if (userIsInRoleForTenant(tenantId, identity, tenantRoleAdmin))
			{
				authorization = UserTenantAuthorization.Admin;
			}
			else if (userIsInRoleForTenant(tenantId, identity, tenantRoleDeveloper))
			{
				authorization = UserTenantAuthorization.Developer;
			}
			else if (userIsInRoleForTenant(tenantId, identity, tenantRoleWriter))
			{
				authorization = UserTenantAuthorization.Writer;
			}
			else if (userIsInRoleForTenant(tenantId, identity, tenantRoleReader))
			{
				authorization = UserTenantAuthorization.Reader;
			}
			this.logger.LogInformation($"GetHighestTenantRoleForIdentity: userId:{userId}, tenantId:{tenantId}, role:{authorization}");
			return authorization;

			static string[] splitClaimString(string adminclaimstring)
			{
				return string.IsNullOrEmpty(adminclaimstring) ? new string[] { } : adminclaimstring.Split(":");
			}

			static bool userIsInRoleForTenant(string tenantId, ClaimsIdentity? identity, string tenantRoleAdmin)
			{
				Claim? claim = identity.FindFirst(tenantRoleAdmin);
				return claim == null? false : splitClaimString(claim.Value).Contains(tenantId);
			}
		}

		internal UserServiceAuthorization GetUserServiceAuthorizationFromIdentityClaim(ClaimsIdentity? identity)
		{
			string serviceClaim = identity.FindFirst("ServiceRole").Value.ToString();
			UserServiceAuthorization serviceAuth = (UserServiceAuthorization)Enum.Parse(typeof(UserServiceAuthorization), serviceClaim);
			return serviceAuth;
		}

		internal bool DoesApiKeyMatchForTenant(string tenantId)
		{
			TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
			if (tenantProfile.ApiKey != ApiKey)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return false;
			}
			return true;
		}
	}
}
