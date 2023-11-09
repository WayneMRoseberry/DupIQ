using Azure;
using DupIQ.IssueIdentity;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public class SqlIssueDbProvider : IIssueDbProvider, IIssueIdentityProvider
	{
		private IIssueDbIOHelper sqlIOHelper;
		private ILogger _logger;

		public SqlIssueDbProvider(IIssueDbIOHelper sqlIOHelper)
		{
			Initialize(sqlIOHelper, (ILogger<SqlIssueDbProvider>?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<SqlIssueDbProvider>());
		}

		public SqlIssueDbProvider(IIssueDbIOHelper sqlIOHelper, ILogger logger)
		{
			Initialize(sqlIOHelper, logger);
		}

		private void Initialize(IIssueDbIOHelper sqlIOHelper, ILogger logger)
		{
			_logger = logger;
			this.sqlIOHelper = sqlIOHelper;
		}

		public bool CanHandleIssue(string issueMessage)
		{
			throw new NotImplementedException();
		}

		public void Configure(string configJson)
		{
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId)
		{
			sqlIOHelper.AddIssueReport(issueReport, tenantConfiguration, projectId);
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			sqlIOHelper.AddIssueProfile(issueProfile, tenantConfiguration, projectId);
		}

		public IssueReport GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			this._logger.LogInformation($"SqlIssueDbProvider.GetIssueReport: instanceId:{instanceId}, tenantId:{tenantConfiguration.TenantId}, projectId:{projectId}");
			using (DbDataReader reader = sqlIOHelper.GetIssueReport(instanceId, tenantConfiguration, projectId))
			{
				if (reader.Read())
				{
					IssueReport issueReport = new IssueReport()
					{
						InstanceId = reader["InstanceId"].ToString().Trim(),
						IssueId = reader["IssueId"].ToString().Trim(),
						IssueMessage = reader["IssueMessage"].ToString().Trim(),
						IssueDate = Convert.ToDateTime(reader["IssueDate"].ToString())
					};
					return issueReport;
				}
				else
				{
					throw new IssueDoesNotExistException(instanceId);
				}
			}
		}

		public IssueReport[] GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId, int page=0)
		{
			List<IssueReport> result = new List<IssueReport>();
			using (DbDataReader reader = sqlIOHelper.GetIssueReports(issueProfile, tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					result.Add(new IssueReport()
					{
						InstanceId = reader["InstanceId"].ToString().Trim(),
						IssueId = reader["IssueId"].ToString().Trim(),
						IssueMessage = reader["IssueMessage"].ToString().Trim(),
						IssueDate = Convert.ToDateTime(reader["IssueDate"])
					}
					);
				}
			}
			return result.ToArray<IssueReport>();
		}

		public IssueProfile GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			this._logger.LogInformation($"SqlIssueDbProvider.GetIssueProfile: issueId:{issueId}, tenantId:{tenantConfiguration.TenantId}, projectId:{projectId}");
			using (var reader = sqlIOHelper.GetIssueProfile(issueId, tenantConfiguration, projectId))
			{
				if (reader.Read())
				{
					IssueProfile issueProfile = new IssueProfile()
					{
						IssueId = reader["Id"].ToString().Trim(),
						FirstReportedDate = Convert.ToDateTime(reader["FirstReportedDate"].ToString()),
						ExampleMessage = reader["ExampleMessage"].ToString().Trim(),
						ProviderId = ID()
					};
					return issueProfile;
				}
				else
				{
					throw new IssueDoesNotExistException(issueId);
				}
			}
		}

		public IssueProfile[] GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId, int page = 0)
		{
			List<IssueProfile> result = new List<IssueProfile>();
			using (DbDataReader reader = sqlIOHelper.GetIssueProfiles(tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					result.Add(new IssueProfile()
					{
						IssueId = reader["Id"].ToString().Trim(),
						ExampleMessage = reader["ExampleMessage"].ToString().Trim(),
						FirstReportedDate = Convert.ToDateTime(reader["FirstReportedDate"].ToString()),
						ProviderId = ID()
					}
					);
				}
			}
			return result.ToArray<IssueProfile>();
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			sqlIOHelper.DeleteIssueProfile(id, tenantConfiguration, projectId);
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			sqlIOHelper.DeleteIssueReport(instanceId, tenantConfiguration, projectId);
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public string ID()
		{
			return GetType().Name;
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId, int page = 0)
		{
			throw new NotImplementedException();
		}
	}
}
