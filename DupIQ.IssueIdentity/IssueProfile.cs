namespace DupIQ.IssueIdentity
{
	public class IssueProfile
	{
		/// <summary>
		/// Unique identifier for the issue.
		/// </summary>
		public string IssueId { get; set; }

		/// <summary>
		/// True if was the first time the issue had been fetched, False if issue had already exists.
		/// </summary>
		public bool IsNew { get; set; }

		/// <summary>
		/// Indicates an example issue message associated with the known issue.
		/// </summary>
		public string ExampleMessage { get; set; }

		/// <summary>
		/// Indicates first date when the issue profile was written to the issue database.
		/// </summary>
		public DateTime FirstReportedDate { get; set; }

		public string ProviderId { get; set; }
	}
}
