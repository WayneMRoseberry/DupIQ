namespace DupIQ.IssueIdentity
{
	public class IssueIdentityUser
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public IssueIdentityUserStatus Userstatus { get; set; }
	}

	public enum IssueIdentityUserStatus
	{
		Active
	}
}
