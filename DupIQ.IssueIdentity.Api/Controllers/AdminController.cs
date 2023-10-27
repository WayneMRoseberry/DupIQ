using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DupIQ.IssueIdentityAPI.Controllers
{
	/// <summary>
	/// Provides administrative support to the service.
	/// </summary>
	[Route("[controller]")]
	[ApiController]
	public class AdminController : IssueIdentityControllerBaseClass
	{
		ILogger logger;
		IConfiguration _configuration;
		public AdminController(IConfiguration configuration, ILogger<AdminController> logger)
		{
			_configuration = configuration;
			this.logger = logger;
			GlobalConfiguration.InitializeConfiguration(_configuration, this.logger);
		}

		/// <summary>
		/// Retrieves the configuration of the service.
		/// </summary>
		/// <returns>An object describing configuration settings.</returns>
		// GET: api/<AdminController>
		[HttpGet("config")]
		public IssueIdentityAPIConfig Get(string tenantId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return new IssueIdentityAPIConfig() { };
			}
			return null;
		}

		/// <summary>
		/// Purges all IssueProfile and IssueReport records from the store.
		/// </summary>
		// DELETE api/<AdminController>/5
		[HttpDelete("allrecords")]
		public void DeleteAllRecords(string tenantId, string projectId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantConfiguration tenantConfiguration = GlobalConfiguration.TenantManager.GetTenantConfiguration(tenantId);
				GlobalConfiguration.dbIOHelper.PurgeIssueProfiles(tenantConfiguration, projectId);
				GlobalConfiguration.dbIOHelper.PurgeIssueReports(tenantConfiguration, projectId);
			}
		}

		/// <summary>
		/// Purges rows from the vector database.
		/// </summary>
		// DELETE api/<AdminController>/5
		[HttpDelete("allindexes")]
		public void DeleteAllIndexes(string tenantId, string projectId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				GlobalConfiguration.word2Vec_Pinecone_VectorHelper.EmptyIndex(tenantId, projectId);

			}
		}
	}
}
