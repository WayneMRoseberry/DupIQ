using Microsoft.Extensions.Logging;

namespace DupIQ.IssueIdentity
{
	public class IssueIdentityManager
	{
		private Dictionary<string, IIssueIdentityProvider> providers = new Dictionary<string, IIssueIdentityProvider>();
		private string _defaultProviderName = string.Empty;
		private ITenantManager _tenantManager;
		private ILogger _logger;

		/// <summary>
		/// Instantiates issue identity manager.
		/// </summary>
		/// <param name="taggedIssueIdProvider">Handles issues with tags in the message.</param>
		/// <param name="issueIdentityProvider">Handles based on text of message.</param>
		//public IssueIdentityManager(IIssueIdentityProvider taggedIssueIdProvider, IIssueIdentityProvider issueIdentityProvider)
		//{
		//	Initialize(taggedIssueIdProvider, issueIdentityProvider, null, (ILogger?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<IssueIdentityManager>());
		//}

		public IssueIdentityManager(IIssueIdentityProvider[] providers, ITenantManager tenantManager)
		{
			Initialize(providers, tenantManager, (ILogger?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<IssueIdentityManager>());
		}

		public IssueIdentityManager(IIssueIdentityProvider[] providers, ITenantManager tenantManager, ILogger logger, string defaultProviderName)
		{
			Initialize(providers, tenantManager, logger);
			_defaultProviderName = defaultProviderName;
		}

		private void Initialize(IIssueIdentityProvider taggedIssueIdProvider, IIssueIdentityProvider issueIdentityProvider, ITenantManager tenantManager, ILogger logger)
		{
			IIssueIdentityProvider[] providers = new IIssueIdentityProvider[] { taggedIssueIdProvider, issueIdentityProvider };
			Initialize(providers, tenantManager, logger);
		}

		public void Initialize(IIssueIdentityProvider[] providers, ITenantManager tenantManager, ILogger logger)
		{
			logger.LogInformation(SharedEvents.ProviderStartup, "Starting IssueIdentityManager");
			foreach(var p in providers)
			{
				this.providers.Add(p.ID(), p);
			}
			this._tenantManager = tenantManager;
			this._logger = logger;
		}

		/// <summary>
		/// Configures the identity manager and its associated providers.
		/// Doesn't expect the caller to know the configuration strings, which
		/// means the configuration is going to have to come from some kind of
		/// file deployed with the provider, yet to be determined TODO.
		/// </summary>
		public void Configure()
		{
		}

		public void AddIssueProfile(IssueProfile issue, TenantConfiguration tenantConfiguration)
		{
			string projectId = string.Empty;
			AddIssueProfile(issue, tenantConfiguration, projectId);
		}

		public void AddIssueProfile(IssueProfile issue, TenantConfiguration tenantConfiguration, string projectId)
		{
			var provider = this.providers.ContainsKey(issue.ProviderId) ? this.providers[issue.ProviderId] : this.providers[this._defaultProviderName];
			provider.AddIssueProfile(issue, tenantConfiguration, projectId);
		}

		public void DeleteIssueProfile(string issueId, TenantConfiguration tenantConfiguration)
		{
			string projectId = string.Empty;
			DeleteIssueProfile(issueId, tenantConfiguration, projectId);
		}

		public void DeleteIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			foreach(var providerKey in this.providers.Keys)
			{
				var provider = providers[providerKey];
				try
				{
					var p = provider.GetIssueProfile(issueId, tenantConfiguration, projectId);
					// if we got this far and it did not throw, that means there is a profile there we need to delete
					provider.DeleteIssueProfile(issueId, tenantConfiguration, projectId);
				}
				catch (IssueDoesNotExistException e)
				{

				}
			}


			//_idProvider.DeleteIssueProfile(issueId, tenantConfiguration, projectId);
		}

		public IssueProfile GetOrCreateIssueProfile(string issueMessage, TenantConfiguration tenantConfiguration)
		{
			string projectId = string.Empty;
			return GetOrCreateIssueProfile(issueMessage, tenantConfiguration, projectId);
		}

		public IssueProfile GetOrCreateIssueProfile(string issueMessage, TenantConfiguration tenantConfiguration, string projectId)
		{
			IIssueIdentityProvider provider = null;
			provider = FindProviderThatCanHandleMessage(issueMessage);
			_logger.LogTrace(SharedEvents.IssueIdentityManager_GetOrCreateIssueProfile, "GetOrCreateIssueProfile with {Provider} for message='{Message}'", provider.GetType().Name, issueMessage);

			RelatedIssueProfile[] checkOldIssues = provider.GetRelatedIssueProfiles(issueMessage, 1, tenantConfiguration, projectId);
			if (checkOldIssues.Count() > 0)
			{
				float similarityThreshold = GetSimilarityThresholdFromProjectOrProvider(tenantConfiguration, projectId, provider);
				RelatedIssueProfile p = checkOldIssues[0];
				if (p.Similarity >= similarityThreshold)
				{
					return new IssueProfile() { IssueId = p.IssueId, IsNew = false, ExampleMessage = issueMessage, ProviderId = provider.ID() };
				}
			}
			IssueProfile result = new IssueProfile() { IssueId = Guid.NewGuid().ToString(), IsNew = true, ExampleMessage = issueMessage, ProviderId = provider.ID() };
			provider.AddIssueProfile(result, tenantConfiguration, projectId);

			return result;

			float GetSimilarityThresholdFromProjectOrProvider(TenantConfiguration tenantConfiguration, string projectId, IIssueIdentityProvider provider)
			{
				float similarityThreshold = provider.IdenticalIssueThreshold(tenantConfiguration, projectId);
				if (_tenantManager != null)
				{
					TenantProfile tenant = _tenantManager.GetTenantProfile(tenantConfiguration.TenantId);
					Project project = _tenantManager.GetProject(tenant.TenantId, projectId);
					similarityThreshold = project.SimilarityThreshold;
				}

				return similarityThreshold;
			}
		}

		/// <summary>
		/// Get related issues to the one indicated in the message.
		/// </summary>
		/// <param name="issueMessage">Message to look for related issues to.</param>
		/// <param name="count">Maximum number of related issue profiles to return.</param>
		/// <returns>List of related issue profiles.</returns>
		/// <exception cref="UnableToMatchIssueException">Thrown if none of the providers are able to match the issue.</exception>
		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration)
		{
			string projectId = string.Empty;
			return GetRelatedIssueProfiles(issueMessage, count, tenantConfiguration, projectId);
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			IIssueIdentityProvider provider = FindProviderThatCanHandleMessage(issueMessage);
			_logger.LogTrace(SharedEvents.IssueIdentityManager_GetRelatedIssueProfiles, "GetRelatedIssueProfiles on Tenant {TenantId} with {Provider} for message='{Message}'", tenantConfiguration.TenantId, provider.GetType().Name, issueMessage);
			return provider.GetRelatedIssueProfiles(issueMessage, count, tenantConfiguration, projectId);
		}

		private IIssueIdentityProvider FindProviderThatCanHandleMessage(string issueMessage)
		{
			foreach(string providerKey in this.providers.Keys)
			{
				if (providers[providerKey].CanHandleIssue(issueMessage))
				{
					return providers[providerKey];
				}
			}

			throw new UnableToMatchIssueException(issueMessage);
		}
	}
}
