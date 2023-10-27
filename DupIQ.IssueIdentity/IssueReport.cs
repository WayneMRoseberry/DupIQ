namespace DupIQ.IssueIdentity
{
	/// <summary>
	/// Used to report issues to the repository. Represents an instance of a given issue having been
	/// observed before it has been identified and stored.
	/// </summary>
	public class IssueReport
	{
		/// <summary>
		/// Text of the issue message. Primary information used to determine if an issue
		/// is new or existing.
		/// </summary>
		public string IssueMessage { get; set; }

		/// <summary>
		/// Date and time this instance of the issue was observed.
		/// </summary>
		public DateTime IssueDate { get; set; }

		/// <summary>
		/// Unique identifier for this issue instance. Distinguishes it from other
		/// times when the same issue may have been observed. Caller is responsible for
		/// keeping the InstanceId unique. The issue database will interpret writes to
		/// existing InstanceId as an update.
		/// </summary>
		public string InstanceId { get; set; }

		/// <summary>
		/// Stores the Id of the issue once it has been identified. Any value in this field
		/// when submitted to ReportIssue will be overwritten once identified.
		/// </summary>
		public string IssueId { get; set; }
	}
}
