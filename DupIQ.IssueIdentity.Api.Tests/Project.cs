namespace DupIQ.IssueIdentity.Api.Tests
{
	public class Project
	{
		public string tenantId { get; set; }
		public string projectId { get; set; }
		public string name { get; set; }
		public string ownerId { get; set; }
		public float similarityThreshold { get; set; }
	}
}
