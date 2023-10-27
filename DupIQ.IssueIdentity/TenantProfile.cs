namespace DupIQ.IssueIdentity
{
	public class TenantProfile
	{
		public string Name { get; set; }
		public string TenantId { get; set; }
		public string OwnerId { get; set; }
		public TenantStatus Status { get; set; }
		public string ApiKey { get; set; }
	}
}
