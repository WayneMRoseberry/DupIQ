namespace DupIQ.IssueIdentity.ConfigureServices
{
	internal class AppArguments
	{
		internal string Command { get; set; }
		internal string ServiceAdminName { get; set; }
		internal string ServiceAdminFirstName { get; set; }
		internal string ServiceAdminLastName { get; set; }
		internal string ServiceAdminEmail { get; set; }
		internal string ServiceAdminPassword { get; set; }

		public AppArguments(string[] args)
		{
			if(args.Length < 2)
			{
				throw new ArgumentException("Insufficient number of arguments.");
			}
			Command = args[1].ToLowerInvariant();

			switch (Command)
			{
				case "database":
					{
						break;
					}
				case "serviceadmin":
					{
						if(args.Length < 7)
						{
							throw new ArgumentException("Command serviceadmin requires at least five parameters.");
						}
						ServiceAdminName = args[2];
						ServiceAdminFirstName = args[3];
						ServiceAdminLastName = args[4];
						ServiceAdminEmail = args[5];
						ServiceAdminPassword = args[6];
						break;
					}
				default:
					{
						throw new ArgumentException($"Invalid command line argument: {Command}");
					}
			}
		}
	}
}
