namespace DupIQ.IssueIdentity
{
	public interface IVectorHelper
	{
		/// <summary>
		/// Returns embeddings for message provided.
		/// </summary>
		/// <param name="issueMessage">Message to get embeddings for.</param>
		/// <returns>Embedding object based on whichever embedding model is implemented.</returns>
		public Embeddings GetEmbeddings(string issueMessage);
		public Embeddings GetEmbeddings(string issueMessage, TenantConfiguration tenantConfiguration);
		public Embeddings GetEmbeddings(string issueMessage, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Adds the list of embeddings to the vector database.
		/// </summary>
		/// <param name="id">Identity of the embeddings to store under.</param>
		/// <param name="embeddings">Embeddings to store.</param>
		public void AddVector(string id, Embeddings embeddings);
		public void AddVector(string id, Embeddings embeddings, TenantConfiguration tenantConfiguration);
		public void AddVector(string id, Embeddings embeddings, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Deletes the specified vector from the vector database.
		/// </summary>
		/// <param name="id">Id of the vector to delete.</param>
		/// <param name="tenantConfiguration">Tenant where the vector resides.</param>
		public void DeleteVector(string id, TenantConfiguration tenantConfiguration);
		public void DeleteVector(string id, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Gets a specific vector from the vector database.
		/// </summary>
		/// <param name="id">Identify of vector to retrieve.</param>
		/// <returns>Embeddings object with vector in value.</returns>
		public Embeddings GetVector(string id);
		public Embeddings GetVector(string id, TenantConfiguration tenantConfiguration);
		public Embeddings GetVector(string id, TenantConfiguration tenantConfiguration, string projectId);

		/// <summary>
		/// Queries vector database for nearest embeddings to one specified.
		/// </summary>
		/// <param name="embeddings">The embeddings to search for.</param>
		/// <param name="count">Maximum number of vectors to return.</param>
		/// <returns></returns>
		public Embeddings[] Query(Embeddings embeddings, int count);
		public Embeddings[] Query(Embeddings embeddings, int count, TenantConfiguration tenantConfiguration);
		public Embeddings[] Query(Embeddings embeddings, int count, TenantConfiguration tenantConfiguration, string projectId);

		public float IdenticalIssueThreshold();
		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId);
	}

	public class Embeddings
	{
		/// <summary>
		/// Identity of this set of embeddings.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Array of floats indicating the value vector of the embeddings.
		/// </summary>
		public float[] Values { get; set; }

		/// <summary>
		/// When returned as a query, represenets the distance of this embeddings
		/// vector from the item queried for.
		/// </summary>
		public float Similarity { get; set; }
	}
}
