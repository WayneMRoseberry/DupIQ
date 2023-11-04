using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DupIQ.IssueIdentity.Api.Tests
{
	public class IssueProfile
	{
		public string issueId { get; set; }
		public bool isNew { get; set; }
		public DateTime firstReportedDate { get; set; }
		public string exampleMessage { get; set; }
		public string providerId { get; set; }
	}
}
