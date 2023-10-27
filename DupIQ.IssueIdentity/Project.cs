namespace DupIQ.IssueIdentity
{
    public class Project
	{
		public string ProjectId { get; set; }
		public string TenantId { get; set; }
		public string Name { get; set; }
		public string OwnerId { get; set; }
		public float SimilarityThreshold { get; set; }
	}
}
