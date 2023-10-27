using DupIQ.IssueIdentity;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace DupIQ.IssueIdentity.Providers
{
	public class TaggedIssueProvider : IIssueIdentityProvider
	{
		const string TaggedIssueRegex = "\\[TAGGEDISSUEID=\"(.*?)\"\\]";
		private IIssueDbIOHelper _sqlIOHelper;
		private ILogger _logger;

		public TaggedIssueProvider(IIssueDbIOHelper sqlIOHelper)
		{
			Initialize(sqlIOHelper, LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<TaggedIssueProvider>());
		}

		public TaggedIssueProvider(IIssueDbIOHelper sqlIOHelper, ILogger logger)
		{
			Initialize(sqlIOHelper, logger);
		}

		private void Initialize(IIssueDbIOHelper sqlIOHelper, ILogger logger)
		{
			_logger = logger;
			_sqlIOHelper = sqlIOHelper;
		}

		public bool CanHandleIssue(string issueMessage)
		{
			MatchCollection matches = Regex.Matches(issueMessage, TaggedIssueRegex);
			return matches.Count() > 0;
		}

		public void Configure(string configJson)
		{
		}

		public static string BuildIssueIdTagFromIssueMessage(string issueMessage)
		{
			string result = string.Empty;
			var matches = Regex.Matches(issueMessage, TaggedIssueRegex);
			if (matches.Count() > 0)
			{
				if (matches[0].Groups.Count > 1)
				{
					result = $"issuetag_{matches[0].Groups[1].Value}";
				}
			}
			return result;
		}

		public static string MakeTagForInsideIssueMessage(string issueTag)
		{
			return $"[TAGGEDISSUEID=\"{issueTag}\"]";
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration)
		{
			using (DbDataReader reader = _sqlIOHelper.GetIssueProfile(id, tenantConfiguration, string.Empty))
			{
				if (!reader.HasRows)
				{
					throw new IssueDoesNotExistException(id);
				}

				reader.Read();
				IssueProfile issueProfile = new IssueProfile()
				{
					IssueId = reader["IssueId"].ToString().Trim(),
					ExampleMessage = reader["ExampleMessage"].ToString().Trim(),
					FirstReportedDate = Convert.ToDateTime(reader["FirstReportedDate"].ToString()),
					ProviderId = this.ID()
				};
				return issueProfile;
			}
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration)
		{
			string issueTag = BuildIssueIdTagFromIssueMessage(issueMessage);
			List<RelatedIssueProfile> related = new List<RelatedIssueProfile>();
			RelatedIssueProfile relatedIssueProfile;

			try
			{
				IssueProfile temp = GetIssueProfile(issueTag, tenantConfiguration);
				relatedIssueProfile = new RelatedIssueProfile()
				{
					IssueId = temp.IssueId,
					IsNew = false,
					ExampleMessage = temp.ExampleMessage,
					FirstReportedDate = temp.FirstReportedDate,
					Similarity = 1.0f
				};
			}
			catch (IssueDoesNotExistException e)
			{
				relatedIssueProfile = new RelatedIssueProfile()
				{
					IssueId = issueTag,
					ExampleMessage = issueMessage,
					FirstReportedDate = DateTime.Now,
					IsNew = true
				};
			}
			related.Add(relatedIssueProfile);
			return related.ToArray<RelatedIssueProfile>();
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration)
		{
			_sqlIOHelper.AddIssueProfile(issueProfile, tenantConfiguration, string.Empty);
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			return 1.0f;
		}

		public string ID()
		{
			return this.GetType().Name;
		}
	}
}
