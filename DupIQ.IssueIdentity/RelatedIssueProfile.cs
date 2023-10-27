using System.Text.Json.Serialization;

namespace DupIQ.IssueIdentity
{
	[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
	public class RelatedIssueProfile : IssueProfile
	{
		/// <summary>
		/// Indicates how similar this profile is to the one it is related to.
		/// Range varies based on the provider.
		/// </summary>
		public float Similarity { get; set; }
	}
}
