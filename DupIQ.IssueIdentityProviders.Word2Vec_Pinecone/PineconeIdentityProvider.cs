using DupIQ.IssueIdentity;
using Microsoft.Extensions.Logging;

namespace DupIQ.IssueIdentityProviders.Word2Vec_Pinecone
{
	public class PineconeIdentityProvider : IIssueIdentityProvider
	{
		private IVectorHelper vectorHelper;
		ILogger _logger;

		public PineconeIdentityProvider(IVectorHelper vectorHelper)
		{
			Initialize(vectorHelper, (ILogger?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<PineconeIdentityProvider>());
		}

		public PineconeIdentityProvider(IVectorHelper vectorHelper, ILogger logger)
		{
			Initialize(vectorHelper, logger);
		}

		private void Initialize(IVectorHelper vectorHelper, ILogger logger)
		{
			_logger = logger;
			this.vectorHelper = vectorHelper;
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (string.IsNullOrEmpty(issueProfile.IssueId))
			{
				throw new ArgumentNullException("IssueId");
			}

			if (string.IsNullOrEmpty(issueProfile.ExampleMessage))
			{
				throw new ArgumentNullException("ExampleMessage");
			}

			// first get vector from word2vec
			var vectorthing = vectorHelper.GetEmbeddings(issueProfile.ExampleMessage);
			// then write the vector to the vector database
			vectorHelper.AddVector(issueProfile.IssueId, vectorthing, tenantConfiguration, projectId);
		}

		public bool CanHandleIssue(string issueMessage)
		{
			// this issue provider will try to handle anything.
			return true;
		}

		public void Configure(string configJson)
		{
		}

		public void DeleteIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			vectorHelper.DeleteVector(id, tenantConfiguration, projectId);
		}

		public IssueProfile GetIssueProfile(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			Embeddings embeddings = vectorHelper.GetVector(id);
			return new IssueProfile() { IssueId = embeddings.Id, IsNew = false, ProviderId = ID() };
		}

		public RelatedIssueProfile[] GetRelatedIssueProfiles(string issueMessage, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			List<RelatedIssueProfile> relatedIssueProfiles = new List<RelatedIssueProfile>();
			// first get vector based on message
			Embeddings embeddings = vectorHelper.GetEmbeddings(issueMessage);
			// then query vector db for related issue profiles
			Embeddings[] results = vectorHelper.Query(embeddings, count, tenantConfiguration, projectId);
			foreach (Embeddings e in results)
			{
				relatedIssueProfiles.Add(new RelatedIssueProfile() { IssueId = e.Id, IsNew = false, Similarity = e.Similarity });
			}
			return relatedIssueProfiles.ToArray<RelatedIssueProfile>();
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			return vectorHelper.IdenticalIssueThreshold();
		}

		public string ID()
		{
			return GetType().Name;
		}
	}
}
