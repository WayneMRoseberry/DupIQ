using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;

namespace DupIQ.IssueIdentityAPI.Controllers
{
	[Microsoft.AspNetCore.Mvc.Route("[controller]")]
	[ApiController]
	//[ApiKey]
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

		[HttpPost("Project")]
		public void AddProject(string tenantId, Project project)
		{
			GlobalConfiguration.TenantManager.AddProject(tenantId, project);
		}

		[HttpGet("Projects")]
		public IEnumerable<Project> GetProjects(string tenantId)
		{
			if(DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProjects(tenantProfile.TenantId);
			}
			return null;
		}

		[HttpGet("ProjectsForUser")]
		public IEnumerable<Project> GetProjectsForUser(string tenantId, string userId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProjects(tenantProfile.TenantId, userId);
			}
			return null;
		}

		[HttpGet("Project")]
		public Project GetProject(string tenantId, string projectId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				TenantProfile tenantProfile = GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
				return GlobalConfiguration.TenantManager.GetProject(tenantProfile.TenantId, projectId);
			}
			return null;
		}

		[HttpPost("Tenant")]
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

		[HttpGet("Tenant")]
		public TenantProfile GetTenant(string tenantId)
		{
			if (DoesApiKeyMatchForTenant(tenantId))
			{
				return GlobalConfiguration.TenantManager.GetTenantProfile(tenantId);
			}
			return null;
		}

	}
}
