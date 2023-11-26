using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DupIQ.IssueIdentityAPI.Controllers
{
	/// <summary>
	/// Allows creation and retrieval of IssueProfiles.
	/// An IssueProfile is a description of an issue that has been reported at least once. Specific occurrences
	/// off IssueProfiles are reporited via IssueReport(s).
	/// </summary>
	[Microsoft.AspNetCore.Authorization.Authorize]
	[Microsoft.AspNetCore.Mvc.Route("[controller]")]
	[ApiController]
	//[ApiKey]
	public class IssueProfilesController : IssueIdentityControllerBaseClass
	{
		IConfiguration _configuration;

		public IssueProfilesController(IConfiguration configuration, ILogger<IssueProfilesController> logger)
		{
			_configuration = configuration;
			GlobalConfiguration.InitializeConfiguration(_configuration, logger);
			this.logger = logger;
		}

		/// <summary>
		/// Get all the IssueProfile objects in the system.
		/// </summary>
		/// <returns>An array of IssueProfile objects.</returns>
		// GET: api/<IssueProfilesController>
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public IEnumerable<IssueProfile> GetForTenantId(string tenantId, string projectId, int page=0)
		{
			logger.LogInformation("GetIssueProfiles. tenantId:{tenantId}", tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.Repository.GetIssueProfiles(tenantId, projectId, page);
			}
			return null;
		}

		/// <summary>
		/// Get a specific IssueProfile.
		/// </summary>
		/// <param name="issueId">The id of the IssueProfile you want to get.</param>
		/// <returns>A unique IssueProfile object with the matching issueId.</returns>
		// GET api/<IssueProfilesController>/5
		/// <summary>
		/// Get a specific IssueProfile.
		/// </summary>
		/// <param name="issueId">Id of the profile to get.</param>
		/// <param name="tenantId">TenantId to get it from.</param>
		/// <returns>A unique IssueProfile object with the matching issueId.</returns>
		[Microsoft.AspNetCore.Mvc.HttpGet("IssueProfile")]
		public IssueProfile GetForTenantId(string issueId, string tenantId, string projectId)
		{
			logger.LogInformation("GetIssueProfile issueId:{issueId}, tenantId:{tenantId}", issueId, tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.Repository.GetIssueProfile(issueId, tenantId, projectId);
			}
			return null;
		}

		/// <summary>
		/// Adds an IssueProfile. If exists already, updates it.
		/// </summary>
		/// <param name="issueProfile">The issue profile to add.</param>
		[Microsoft.AspNetCore.Mvc.HttpPost("IssueProfile")]
		public void AddIssueProfileForTenant(IssueProfile issueProfile, string tenantId, string projectId)
		{
			logger.LogInformation("AddIssueProfile issueId:{issueId}, tenantId:{tenantId}", issueProfile.IssueId, tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				GlobalConfiguration.Repository.AddIssueProfile(issueProfile, tenantId, projectId);
			}
		}

		/// <summary>
		/// Adds an IssueProfile. If exists already, updates it.
		/// </summary>
		/// <param name="issueProfile">The issue profile to add.</param>
		[Microsoft.AspNetCore.Mvc.HttpPost("IssueProfiles")]
		public void AddIssueProfilesForTenant(IssueProfile[] issueProfiles, string tenantId, string projectId)
		{
			logger.LogInformation("AddIssueProfiles tenantId:{tenantId}", tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				foreach (IssueProfile issueProfile in issueProfiles)
				{
					GlobalConfiguration.Repository.AddIssueProfile(issueProfile, tenantId, projectId);
				}
			}
		}

		/// <summary>
		/// Get IssueProfiles which are similar to the issue described by a given message.
		/// </summary>
		/// <param name="message">A message to use to search for similar IssueProfiles.</param>
		/// <returns>An array of RelatedIssueProfile objects, which inherit from IssueProfile, but have a Similarity field indicating how similar they are to the specified message. The array is sorted in descending order, larger numbers being the most similar.</returns>
		// GET api/<IssueProfilesController>/5
		[Microsoft.AspNetCore.Mvc.HttpGet("Related")]
		public IEnumerable<RelatedIssueProfile> GetRelatedForTenant(string message, string tenantId, string projectId, int page=0)
		{
			logger.LogInformation("GetReltedIssueProfiles. tenantId:{tenantId}", tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				IssueReport issueReport = new IssueReport() { IssueMessage = message };
				return GlobalConfiguration.Repository.GetRelatedIssueProfiles(issueReport, 10, tenantId, projectId,page);
			}
			return null;
		}

		/// <summary>
		/// Get IssueProfiles which are similar to the specified IssueReport..
		/// </summary>
		/// <param name="value">The IssueReport to match against.</param>
		/// <returns>An array of RelatedIssueProfile objects, which inherit from IssueProfile, but have a Similarity field indicating how similar they are to the specified IssueReport. The array is sorted in descending order, larger numbers being the most similar.</returns>
		// POST api/<IssueProfilesController>
		[Microsoft.AspNetCore.Mvc.HttpPost("Related")]
		public IEnumerable<RelatedIssueProfile> Post([Microsoft.AspNetCore.Mvc.FromBody] IssueReport value, string tenantId, string projectId)
		{
			logger.LogInformation("PostGetRelatedIssueProfiles. tenantId:{tenantId}", tenantId); if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.Repository.GetRelatedIssueProfiles(value, 10, tenantId, projectId);
			}
			return null;
		}

		/// <summary>
		/// Delete the specified IssueProfile
		/// </summary>
		/// <param name="issueId">Id of the IssueProfile to delete.</param>
		/// <param name="deleteIssueReports">If true deletes issue reports related to the specified IssueProfile.</param>
		// DELETE api/<IssueProfilesController>/5
		[Microsoft.AspNetCore.Mvc.HttpDelete("IssueProfile")]
		public void DeleteForTenant(string issueId, string tenantId, string projectId, bool deleteIssueReports = false)
		{
			logger.LogInformation("Delete IssueProfile. issueId={issueId}, tenantId={tenantId}", issueId, tenantId);
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				GlobalConfiguration.Repository.DeleteIssueProfile(issueId, tenantId, deleteIssueReports, projectId);
			}
		}
	}
}
