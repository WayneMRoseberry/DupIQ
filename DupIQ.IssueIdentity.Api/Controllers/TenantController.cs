using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
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
			GlobalConfiguration.TenantManager.AddProject(tenantId, project);
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("Projects")]
		public IEnumerable<Project> GetProjects(string tenantId)
		{
			if(DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProjects(tenantProfile.TenantId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpGet("ProjectsForUser")]
		public IEnumerable<Project> GetProjectsForUser(string tenantId, string userId)
		{
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
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProject(tenantProfile.TenantId, projectId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpPost("Tenant")]
		public string AddTenant(string tenantName, string ownerId, string ownerEmail, string ownerName)
		{
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

		[Microsoft.AspNetCore.Mvc.HttpGet("Tenant")]
		public TenantProfile GetTenant(string tenantId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpGet]
		public string[] GetTenants()
		{
			return GlobalConfiguration.TenantManager.GetTenants();
		}

	}
}
