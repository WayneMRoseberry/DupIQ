using DupIQ.IssueIdentity.Providers;
using Microsoft.Extensions.Logging;

namespace DupIQ.IssueIdentity
{
	public class IssueRepository
	{
		private IIssueDbProvider _dbProvider;
		private IssueIdentityManager _identityManager;
		private ILogger _logger;
		private ITenantManager _tenantManager;

		public IssueRepository() { }

		public IssueRepository(IIssueDbProvider dbProvider, IssueIdentityManager identityManager)
		{
			var temp = LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<IssueRepository>();

			InitializeRepository(dbProvider, identityManager, temp);
		}

		public IssueRepository(IIssueDbProvider dbProvider, IssueIdentityManager identityManager, ILogger logger)
		{
			InitializeRepository(dbProvider, identityManager, logger);
		}

		public IssueRepository(IIssueDbProvider dbProvider, IssueIdentityManager identityManager, ILogger logger, ITenantManager tenantManager) : this(dbProvider, identityManager, logger)
		{
			InitializeRepository(dbProvider, identityManager, logger, tenantManager);
		}

		private void InitializeRepository(IIssueDbProvider dbProvider, IssueIdentityManager identityManager, ILogger logger)
		{
			var tenantManager = new EmptyTenantManager();
			InitializeRepository(dbProvider, identityManager, logger, tenantManager);
		}

		private void InitializeRepository(IIssueDbProvider dbProvider, IssueIdentityManager identityManager, ILogger logger, ITenantManager tenantManager)
		{
			_logger = logger;
			_logger.LogTrace(SharedEvents.ProviderStartup, "Starting IssueRepository.");
			_dbProvider = dbProvider;
			_identityManager = identityManager;
			_dbProvider.Configure(string.Empty);
			_identityManager.Configure();
			_tenantManager = tenantManager;
		}

		#region public methods

		/// <summary>
		/// Report issue to repository. Will be assigned issue id of either an
		/// existing issue or a new id. Issue report will be written to issue database,
		/// and an issue profile likewise if this is a new issue.
		/// </summary>
		/// <param name="issueReport">Issue to be reported.</param>
		/// <returns>Profile of the issue as either new or existing.</returns>
		public IssueProfile ReportIssue(IssueReport issueReport, string tenantId, string projectId)
		{
			TenantConfiguration tenantConfig = _tenantManager.GetTenantConfiguration(tenantId);
			return ReportIssue(issueReport, tenantConfig, projectId);
		}

		public IssueProfile ReportIssue(IssueReport issueReport, TenantConfiguration tenantConfig, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_ReportIssue, "Report Issue, InstanceId={InstanceId}", issueReport.InstanceId);
			if (string.IsNullOrWhiteSpace(issueReport.IssueMessage))
			{
				throw new ArgumentException("Message cannot be null, blank, or whitespace.");
			}
			if (issueReport.IssueDate.Equals(DateTime.MinValue) || issueReport.IssueDate.Equals(DateTime.MaxValue))
			{
				issueReport.IssueDate = DateTime.Now;
			}
			IssueProfile issueProfile = _identityManager.GetOrCreateIssueProfile(issueReport.IssueMessage, tenantConfig, projectId);
			issueReport.IssueId = issueProfile.IssueId;
			_dbProvider.AddIssueReport(issueReport, tenantConfig, projectId);
			if (issueProfile.IsNew)
			{
				_logger.LogTrace(SharedEvents.IssueRepository_ReportNewIssue, "New issue, IssueId={IssueId}", issueReport.IssueId);
				issueProfile.ExampleMessage = issueReport.IssueMessage;
				issueProfile.FirstReportedDate = issueReport.IssueDate;
				_dbProvider.AddIssueProfile(issueProfile, tenantConfig, projectId);
			}
			else
			{
				PopulateIssueProfileDataFromExistingProfileOrCreateNewIfDataOutOfSync(issueReport, issueProfile, tenantConfig, projectId);

			}
			return issueProfile;
		}

		/// <summary>
		/// Returns the IssueReport from the issue database.
		/// </summary>
		/// <param name="instanceId">Instance identifier of corresponding IssueReport</param>
		/// <returns>Specified IssueReport.</returns>
		public IssueReport IssueReport(string instanceId, string tenantId, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_GetIssueReport, "Get Issue Report, InstanceId={InstanceId}, TenantId={TenantId}", instanceId, tenantId);
			try
			{
				TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
				return _dbProvider.GetIssueReport(instanceId, tenantConfiguration, projectId);
			}
			catch (IssueDoesNotExistException e)
			{
				_logger.LogError(SharedEvents.IssueReportDoesNotExist, "Issue report does not exist. InstanceId={InstanceId}", instanceId);
				throw new IssueReportDoesNotExistException(instanceId, e);
			}
			catch (Exception e)
			{
				_logger.LogError("Unable to get issue report. InstanceId={0}", instanceId);
				throw;
			}
		}

		/// <summary>
		/// Returns an array of IssueReports that are of the specified IssueProfile.
		/// </summary>
		/// <param name="issueProfile">IssueProfile to match for where its Id field corresponds to IssueId on issue reports.</param>
		/// <returns>Array of matching IssueReport objects.</returns>

		public IssueReport[] GetIssueReportsFromProject(IssueProfile issueProfile, string tenantId, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_GetIssueReports, "Get Issue Reports, IssueId={IssueId}, TenantId={TenantId}", issueProfile.IssueId, tenantId);
			TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
			return _dbProvider.GetIssueReports(issueProfile, tenantConfiguration, projectId);
		}

		/// <summary>
		/// Adds specified profile to the system. If exists already will update.
		/// </summary>
		/// <param name="issueProfile">Profile to add.</param>
		/// <param name="tenantId">Tenant to add it to.</param>
		public void AddIssueProfile(IssueProfile issueProfile, string tenantId, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_GetIssueProfile, "Add issue profile. IssueId={issueProfile.IssueId}, TenantId={TenantId}", issueProfile.IssueId, tenantId);
			TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
			_dbProvider.AddIssueProfile(issueProfile, tenantConfiguration, projectId);
			_identityManager.AddIssueProfile(issueProfile, tenantConfiguration, projectId);
		}

		public void DeleteIssueProfile(string id, string tenantId, bool deleteRelatedReports, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_DeleteIssueProfile, "Delete issue profile. IssueId={id}, TenantId={TenantId}", id, tenantId);
			TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
			if (deleteRelatedReports)
			{
				IssueProfile profile = _dbProvider.GetIssueProfile(id, tenantConfiguration, projectId);
				IssueReport[] reports = _dbProvider.GetIssueReports(profile, tenantConfiguration, projectId);
				foreach (IssueReport report in reports)
				{
					_dbProvider.DeleteIssueReport(report.InstanceId, tenantConfiguration, projectId);
				}
			}
			try
			{
				_dbProvider.DeleteIssueProfile(id, tenantConfiguration, projectId);
			}
			catch (Exception e)
			{
				_logger.LogError("Delete IssueProfile from db failed. Proceeding to delete from vector db. Exception={0}", e.Message);
			}
			try
			{
				_identityManager.DeleteIssueProfile(id, tenantConfiguration, projectId);
			}
			catch (Exception e)
			{
				_logger.LogError("Delete IssueProfile from vector db failed. Exception={0}", e.Message);
			}
		}

		public void DeleteIssueReport(string instanceId, string tenantId, string projectid)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_DeleteIssueProfile, "Delete issue profile. InstanceId={instanceId}, TenantId={TenantId}", instanceId, tenantId);
			TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
			_dbProvider.DeleteIssueReport(instanceId, tenantConfiguration, projectid);
		}

		/// <summary>
		/// Gets specified IssueProfile from database.
		/// </summary>
		/// <param name="issueId">Id of issue to look for.</param>
		/// <returns>Specified IssueProfile object.</returns>
		/// <exception cref="IssueDoesNotExistException">Thrown if unable to find issue.</exception>
		public IssueProfile GetIssueProfile(string issueId, string tenantId, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_GetIssueProfile, "Get issue profile. IssueId={IssueId}, TenantId={TenantId}", issueId, tenantId);
			try
			{
				TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
				return _dbProvider.GetIssueProfile(issueId, tenantConfiguration, projectId);
			}
			catch (Exception e)
			{
				_logger.LogError(SharedEvents.IssudeDoesNotExist, "Issue does not exist. IssueId={IssueId}", issueId);
				throw new IssueDoesNotExistException(issueId, e);
			}
		}

		/// <summary>
		/// Returns all issue profiles in the issue database.
		/// </summary>
		/// <returns>Array of IssueProfile objects.</returns>
		public IssueProfile[] GetIssueProfiles(string tenantId, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_GetIssueProfiles, "Get issue profiles. TenantId={TenantId}", tenantId);
			TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
			return _dbProvider.GetIssueProfiles(tenantConfiguration, projectId);
		}

		/// <summary>
		/// Gets any related issues to the one specified in the issue report.
		/// </summary>
		/// <param name="issueReport">Issue to get related issues for.</param>
		/// <param name="count">Maximum number of issue profiles to return.</param>
		/// <returns>List of related issue profiles.</returns>
		public RelatedIssueProfile[] GetRelatedIssueProfiles(IssueReport issueReport, int count, string tenantId, string projectId)
		{
			TenantConfiguration tenantConfiguration = _tenantManager.GetTenantConfiguration(tenantId);
			_logger.LogTrace(SharedEvents.IssueRepository_GetRelatedIssueProfiles, "Get related issue profiles, InstanceId={InstanceId}, MaxCount={MaxCount}, TenantId={TenantId}", issueReport.InstanceId, count, tenantId);
			RelatedIssueProfile[] relatedIssueProfiles = _identityManager.GetRelatedIssueProfiles(issueReport.IssueMessage, count, tenantConfiguration, projectId);

			AddAnyFieldsThatMightHaveBeenMissingInTheRelatedProfileLookup(relatedIssueProfiles, tenantConfiguration, projectId);
			return relatedIssueProfiles;
		}
		#endregion

		#region private methods
		private void PopulateIssueProfileDataFromExistingProfileOrCreateNewIfDataOutOfSync(IssueReport issueReport, IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			_logger.LogTrace(SharedEvents.IssueRepository_ReportExistingIssue, "Existing issue, IssueId={IssueId}", issueReport.IssueId);
			try
			{
				IssueProfile temp = _dbProvider.GetIssueProfile(issueProfile.IssueId, tenantConfiguration, projectId);
				issueProfile.ExampleMessage = temp.ExampleMessage;
				issueProfile.FirstReportedDate = temp.FirstReportedDate;
			}
			catch (IssueDoesNotExistException e)
			{
				issueProfile.ExampleMessage = issueReport.IssueMessage;
				issueProfile.FirstReportedDate = issueReport.IssueDate;
				_dbProvider.AddIssueProfile(issueProfile, tenantConfiguration, projectId);
			}
		}

		private void AddAnyFieldsThatMightHaveBeenMissingInTheRelatedProfileLookup(RelatedIssueProfile[] relatedIssueProfiles, TenantConfiguration tenantConfiguration, string projectId)
		{
			for (int i = 0; i < relatedIssueProfiles.Count(); i++)
			{
				if (string.IsNullOrEmpty(relatedIssueProfiles[i].ExampleMessage))
				{
					try
					{
						var storedProfile = _dbProvider.GetIssueProfile(relatedIssueProfiles[i].IssueId, tenantConfiguration, projectId);
						relatedIssueProfiles[i].ExampleMessage = storedProfile.ExampleMessage;
						relatedIssueProfiles[i].FirstReportedDate = storedProfile.FirstReportedDate;
					}
					catch (IssueDoesNotExistException e)
					{
						_logger.LogError(SharedEvents.IssudeDoesNotExist, "GetRelatedIssueProfiles:Issue does not exist. Continuing with next issue. issueId={IssueId}", relatedIssueProfiles[i].IssueId);
					}
				}
			}
		}
		#endregion
	}
}