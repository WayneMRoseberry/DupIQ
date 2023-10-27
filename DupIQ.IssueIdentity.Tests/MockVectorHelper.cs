namespace DupIQ.IssueIdentity.Tests
{
	public class MockVectorHelper : IVectorHelper
	{
		public Action<string, Embeddings> overrideAddVector = (id, embeddings) => { throw new NotImplementedException(); };

		public Action<string, Embeddings, TenantConfiguration> overrideAddVectorTenantConfig = (id, embeddings, config) => { throw new NotImplementedException(); };
		public Action<string, Embeddings, TenantConfiguration, string> overrideAddVectorTenantConfigProjId = (id, embeddings, config, projectId) => { throw new NotImplementedException(); };

		public Action<string, TenantConfiguration> overrideDeleteVector = (id, config) => { throw new NotImplementedException(); };
		public Action<string, TenantConfiguration, string> overrideDeleteVectorProjId = (id, config, s2) => { throw new NotImplementedException(); };

		public Func<string, Embeddings> overrideGetEmbeddings = (s) => { throw new NotImplementedException(); };
		public Func<string, Embeddings> overrideGetVector = (s) => { throw new NotImplementedException(); };
		public Func<Embeddings, int, Embeddings[]> overrideQuery = (e, count) => { throw new NotImplementedException(); };
		public Func<Embeddings, int, TenantConfiguration, Embeddings[]> overrideQueryTenantConfig = (e, count, config) => { throw new NotImplementedException(); };
		public Func<Embeddings, int, TenantConfiguration, string, Embeddings[]> overrideQueryTenantConfigProjId = (e, count, config, s2) => { throw new NotImplementedException(); };

		public void AddVector(string id, Embeddings embeddings)
		{
			overrideAddVector(id, embeddings);
		}

		public void AddVector(string id, Embeddings embeddings, TenantConfiguration tenantConfiguration)
		{
			overrideAddVectorTenantConfig(id, embeddings, tenantConfiguration);
		}

		public void AddVector(string id, Embeddings embeddings, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideAddVectorTenantConfigProjId(id, embeddings, tenantConfiguration, projectId);
		}

		public void DeleteVector(string id, TenantConfiguration tenantConfiguration)
		{
			overrideDeleteVector(id, tenantConfiguration);
		}

		public void DeleteVector(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			overrideDeleteVectorProjId(id, tenantConfiguration, projectId);
		}

		public Embeddings GetEmbeddings(string issueMessage)
		{
			return overrideGetEmbeddings(issueMessage);
		}

		public Embeddings GetEmbeddings(string issueMessage, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public Embeddings GetEmbeddings(string issueMessage, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public Embeddings GetVector(string id)
		{
			return overrideGetVector(id);
		}

		public Embeddings GetVector(string id, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public Embeddings GetVector(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public float IdenticalIssueThreshold()
		{
			throw new NotImplementedException();
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public Embeddings[] Query(Embeddings embeddings, int count)
		{
			return overrideQuery(embeddings, count);
		}

		public Embeddings[] Query(Embeddings embeddings, int count, TenantConfiguration tenantConfiguration)
		{
			return overrideQueryTenantConfig(embeddings, count, tenantConfiguration);
		}

		public Embeddings[] Query(Embeddings embeddings, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			return overrideQueryTenantConfigProjId(embeddings, count, tenantConfiguration, projectId);
		}
	}
}