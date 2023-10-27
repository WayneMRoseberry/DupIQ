namespace DupIQ.IssueIdentity
{
	/// <summary>
	/// Describes a tenant's configuration information.
	/// The properties are intended to vary per platform such that platform specific
	/// providers may expect configuration settings specific to that platform only.
	/// </summary>
	public class TenantConfiguration
	{
		/// <summary>
		/// Unique Id that identifies the tenant in the system.
		/// </summary>
		public string TenantId { get; set; }

		/// <summary>
		/// A set of key value pairs that defines the tenant configuration.
		/// Expected key names and their meaning are up to the provider.
		/// </summary>
		public Dictionary<string, string> Configuration { get; set; } = new Dictionary<string, string>();
	}
}
