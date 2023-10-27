using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DupIQ.IssueIdentityAPI
{
	public class IssueIdentityControllerBaseClass : ControllerBase
	{

		//[FromHeader]
		public string ApiKey { get; set; }

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
