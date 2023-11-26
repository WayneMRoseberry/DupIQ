using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DupIQ.IssueIdentityAPI.Controllers
{
	/// <summary>
	/// Allows creation and retrieval of IssueReports. An IssueReport is an instance of an issue occurring.
	/// </summary>
	[Microsoft.AspNetCore.Authorization.Authorize]
	[Route("[controller]")]
	[ApiController]
	public class IssueReportsController : IssueIdentityControllerBaseClass
	{
		IConfiguration _configuration;
		public IssueReportsController(IConfiguration configuration, ILogger<IssueReportsController> logger)
		{
			_configuration = configuration;
			this.logger = logger;
			GlobalConfiguration.InitializeConfiguration(_configuration, this.logger);
		}

		/// <summary>
		/// Gets all the IssueReports that are instances associated with the specified IssueProfile.
		/// </summary>
		/// <param name="issueid">Id of the IssueProfile these IssueReports are instances of.</param>
		/// <returns>Array of IssueReport objects.</returns>
		// GET: api/<IssueReportsController>
		[HttpGet("Related")]
		public IEnumerable<IssueReport> GetForTenant(string issueid, string tenantId, string projectId, int page = 0)
		{
			logger.LogInformation("GetIssueReports. issueId:{issueid}, tenantId:{tenantId}", issueid, tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.Repository.GetIssueReportsFromProject(new IssueProfile() { IssueId = issueid }, tenantId, projectId, page);
			}
			return null;
		}

		/// <summary>
		/// Gets a specific IssueReport.
		/// </summary>
		/// <param name="instanceId">InstanceId of the specified IssueReport. An instanceId is either specified on initial report (if submitted as POST) or assigned randomly.</param>
		/// <returns>The requested IssueReport.</returns>
		// GET api/<IssueReportsController>/5
		[HttpGet("IssueReport")]
		public IssueReport GetIssueReport(string instanceId, string tenantId, string projectId)
		{
			logger.LogInformation("GetIssueReport instanceId={instanceId}, tenantId={tenantId}", instanceId, tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.Repository.IssueReport(instanceId, tenantId, projectId);
			}
			return null;
		}

		/// <summary>
		/// Creates an IssueReport and assigns an IssueProfile. If the message is a new issue, a new IssueProfile will be created. If the message is similar to an existing issue, that will be used instead. InstanceId will be given a random value.
		/// </summary>
		/// <param name="message">The text to check for similar issues.</param>
		/// <returns>The IssueProfile corresponding to the report.</returns>
		// GET api/<IssueReportsController>/5
		[HttpGet("Report")]
		public IssueProfile ReportIssue(string message, string tenantId, string projectId)
		{
			logger.LogInformation("ReportIssue. tenantId:{tenantId}", tenantId);
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
				CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity)))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				if (DoesApiKeyMatchForTenant(tenantId))
				{
					IssueReport issueReport = new IssueReport()
					{
						IssueMessage = message,
						InstanceId = Guid.NewGuid().ToString(),
						IssueDate = DateTime.Now
					};
					return GlobalConfiguration.Repository.ReportIssue(issueReport, tenantId, projectId);
				}
			}

			return null;
		}

		/// <summary>
		/// Submits an IssueReport and assigns an IssueProfile. IssueId field is overwritten. If the message is a new issue, a new IssueProfile will be created. If the message is similar to an existing issue, that will be used instead.
		/// </summary>
		/// <param name="value">IssueReport object to submit.</param>
		/// <returns>The IssueProfile corresponding to the report.</returns>
		// POST api/<IssueReportsController>
		[HttpPost("Report")]
		public IssueProfile Post([FromBody] IssueReport value, string tenantId, string projectId)
		{
			logger.LogInformation("Post ReportIssue. tenantId:{tenantId}", tenantId);
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
	CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity)))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				if (DoesApiKeyMatchForTenant(tenantId))
				{
					return GlobalConfiguration.Repository.ReportIssue(value, tenantId, projectId);
				}
			}
			return null;
		}

		/// <summary>
		/// Submits an list of IssueReports and assigns an IssueProfile to each. IssueId field is overwritten. If the message is a new issue, a new IssueProfile will be created. If the message is similar to an existing issue, that will be used instead.
		/// </summary>
		/// <param name="value">Array of IssueReport objects to submit.</param>
		/// <returns>Array of the IssueProfiles corresponding to the report.</returns>
		// POST api/<IssueReportsController>
		[HttpPost("Reports")]
		public IssueProfile[] PostMultiple([FromBody] IssueReport[] values, string tenantId, string projectId)
		{
			logger.LogInformation("Post ReportIssues. tenantId:{tenantId}", tenantId);
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
	CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity)))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{

				if (DoesApiKeyMatchForTenant(tenantId))
				{
					List<IssueProfile> profiles = new List<IssueProfile>();
					foreach (IssueReport value in values)
					{
						profiles.Add(GlobalConfiguration.Repository.ReportIssue(value, tenantId, projectId));
					}
					return profiles.ToArray();
				}
			}
			return null;
		}

		/// <summary>
		/// Deletes the specified IssueReport.
		/// </summary>
		/// <param name="instanceId">InstanceId of the IssueReport to delete.</param>
		// DELETE api/<IssueReportsController>/5
		[HttpDelete("IssueReport")]
		public void Delete(string instanceId, string tenantId, string projectId)
		{
			logger.LogInformation("Delete IssueReport. instanceId={instanceId}, tenantid={tenantId}", instanceId, tenantId);
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest) &&
	CheckIfUserTenantRoleIsBelowAuthorzationLevel(UserTenantAuthorization.Reader, GetHighestTenantRoleForIdentity(tenantId, HttpContext.User.Identity as ClaimsIdentity)))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				if (DoesApiKeyMatchForTenant(tenantId))
				{
					GlobalConfiguration.Repository.DeleteIssueReport(instanceId, tenantId, projectId);

				}
			}
		}
	}
}
