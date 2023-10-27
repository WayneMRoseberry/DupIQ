namespace DupIQ.IssueIdentity
{
	public class SharedEvents
	{
		public const int ProviderStartup = 1000;
		public const int IssudeDoesNotExist = 1001;
		public const int IssueReportDoesNotExist = 1002;

		public const int IssueIdentityManagerBase = 2000;
		public const int IssueIdentityManager_GetIssueProfile = IssueIdentityManagerBase + 1;
		public const int IssueIdentityManager_GetOrCreateIssueProfile = IssueIdentityManagerBase + 2;
		public const int IssueIdentityManager_GetRelatedIssueProfiles = IssueIdentityManagerBase + 3;

		public const int IssueRepositoryBase = 3000;
		public const int IssueRepository_ReportIssue = IssueRepositoryBase + 1;
		public const int IssueRepository_ReportNewIssue = IssueRepositoryBase + 2;
		public const int IssueRepository_ReportExistingIssue = IssueRepositoryBase + 3;
		public const int IssueRepository_GetIssueReport = IssueRepositoryBase + 4;
		public const int IssueRepository_GetIssueReports = IssueRepositoryBase + 5;
		public const int IssueRepository_GetIssueProfile = IssueRepositoryBase + 6;
		public const int IssueRepository_GetIssueProfiles = IssueRepositoryBase + 7;
		public const int IssueRepository_GetRelatedIssueProfiles = IssueRepositoryBase + 8;
		public const int IssueRepository_UpdateIssueReport = IssueRepositoryBase + 9;
		public const int IssueRepository_DeleteIssueProfile = IssueRepositoryBase + 10;
		public const int IssueRepository_DeleteIssueResult = IssueRepositoryBase + 11;
	}
}
