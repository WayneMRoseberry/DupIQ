namespace DupIQ.IssueIdentity
{
	public class UnableToMatchIssueException : Exception
	{
		public string IssueMessage { get; set; }
		public UnableToMatchIssueException(string issueMessage)
		{
			IssueMessage = issueMessage;
		}
	}

	public class IssueDoesNotExistException : Exception
	{
		public string IssueId { get; set; }

		public IssueDoesNotExistException(string issueId)
		{
			IssueId = issueId;
		}

		public IssueDoesNotExistException(string issueId, Exception exception) : base("inner method threw exception", exception)
		{
			IssueId = issueId;
		}
	}

	public class IssueReportDoesNotExistException : Exception
	{
		public string InstanceId { get; set; }

		public IssueReportDoesNotExistException(string instanceId, Exception exception) : base("inner method threw exception", exception)
		{
			InstanceId = instanceId;
		}
	}
}
