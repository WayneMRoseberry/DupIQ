 using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace DupIQ.IssueIdentity
{
	public class DBProvider : IIssueDbProvider, IIssueIdentityProvider
	{
		private IIssueDbIOHelper dbIOHelper;
		private ILogger _logger;

		public DBProvider(IIssueDbIOHelper dbIOHelper)
		{
			Initialize(dbIOHelper, (ILogger<DBProvider>?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<DBProvider>());
		}

		public DBProvider(IIssueDbIOHelper dbIOHelper, ILogger logger)
		{
			Initialize(dbIOHelper, logger);
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration)
		{
			this.AddIssueProfile(issueProfile, tenantConfiguration, string.Empty);
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			dbIOHelper.AddIssueProfile(issueProfile, tenantConfiguration, projectId);
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration)
		{
			this.AddIssueReport(issueReport, tenantConfiguration, string.Empty);
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId)
		{
			dbIOHelper.AddIssueReport(issueReport, tenantConfiguration, projectId);
		}

		public bool CanHandleIssue(string issueMessage)
		{
			throw new NotImplementedException();
		}

		public void Configure(string configJson)
		{
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration)
		{
			this.DeleteIssueProfile(id, tenantConfiguration, string.Empty);
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			dbIOHelper.DeleteIssueProfile(id, tenantConfiguration, projectId);
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration)
		{
			this.DeleteIssueReport(instanceId, tenantConfiguration, string.Empty);
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			dbIOHelper.DeleteIssueReport(instanceId, tenantConfiguration, projectId);
		}

		public IssueProfile GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration)
		{
			return this.GetIssueProfile(issueId, tenantConfiguration, string.Empty);
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			using (var reader = dbIOHelper.GetIssueProfile(id, tenantConfiguration, projectId))
			{
				if (reader.Read())
				{
					IssueProfile issueProfile = new IssueProfile()
					{
						IssueId = reader["IssueId"].ToString().Trim(),
						FirstReportedDate = Convert.ToDateTime(reader["FirstReportedDate"].ToString()),
						ExampleMessage = reader["ExampleMessage"].ToString().Trim(),
						ProviderId = this.ID()
					};
					return issueProfile;
				}
				else
				{
					throw new IssueDoesNotExistException(id);
				}
			}
		}

		public IssueProfile[] GetIssueProfiles(TenantConfiguration tenantConfiguration)
		{
			return this.GetIssueProfiles(tenantConfiguration, string.Empty);
		}

		public IssueProfile[] GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId)
		{
			List<IssueProfile> result = new List<IssueProfile>();
			using (DbDataReader reader = dbIOHelper.GetIssueProfiles(tenantConfiguration, projectId))
			{
				while (reader.Read())
				{
					result.Add(new IssueProfile()
					{
						IssueId = reader["IssueId"].ToString().Trim(),
						ExampleMessage = reader["ExampleMessage"].ToString().Trim(),
						FirstReportedDate = Convert.ToDateTime(reader["FirstReportedDate"].ToString()),
						ProviderId = this.ID()
					}
					);
				}
			}
			return result.ToArray<IssueProfile>();
		}

		public IssueReport GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration)
		{
			return this.GetIssueReport(instanceId, tenantConfiguration, string.Empty);
		}

		public IssueReport GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			using (DbDataReader reader = dbIOHelper.GetIssueReport(instanceId, tenantConfiguration, projectId))
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

		public IssueReport[] GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration)
		{
			return this.GetIssueReports(issueProfile, tenantConfiguration, string.Empty);
		}

		public IssueReport[] GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			List<IssueReport> result = new List<IssueReport>();
			using (DbDataReader reader = dbIOHelper.GetIssueReports(issueProfile, tenantConfiguration, projectId))
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

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count)
		{
			throw new NotImplementedException();
		}
		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public string ID()
		{
			throw new NotImplementedException();
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId)
		{
			throw new NotImplementedException();
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public void UpdateIssueReportIssueId(string instanceId, string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}


		private void Initialize(IIssueDbIOHelper dbIOHelper, ILogger logger)
		{
			_logger = logger;
			this.dbIOHelper = dbIOHelper;
		}
	}
}
