using DupIQ.IssueIdentity;
using DupIQ.IssueIdentity.Providers;
using DupIQ.IssueIdentityProviders.Sql;
using DupIQ.IssueIdentityProviders.Word2Vec_Pinecone;

namespace DupIQ.IssueIdentityAPI
{
	public static class GlobalConfiguration
	{
		public static IssueRepository Repository { get; set; }
		public static IIssueDbIOHelper dbIOHelper { get; set; }
		public static Word2Vec_Pinecone_VectorHelper word2Vec_Pinecone_VectorHelper { get; set; }
		public static TaggedIssueProvider taggedIssueProvider { get; set; }

		public static ITenantManager TenantManager { get; set; }

		public static SqlIssueIdentityUserManager UserManager { get; set; }

		public static void InitializeConfiguration(IConfiguration configuration, ILogger logger)
		{
			IssueIdentityAPIConfig issueIdentityAPIConfig = new IssueIdentityAPIConfig()
			{
				pineconeconfigjson = configuration.GetConnectionString("pineconeconfigjson"),
			};

			SqlIOHelperConfig sqlConfig = new SqlIOHelperConfig()
			{
				ServerName = configuration["SqlIOHelperConfig:ServerName"],
				DatabaseName = configuration["SqlIOHelperConfig:DatabaseName"],
				AccountName = configuration["SqlIOHelperConfig:AccountName"],
				Password = configuration["SqlIOHelperConfig:Password"],
			};

			InitializeConfiguration(issueIdentityAPIConfig, logger, sqlConfig);
		}

		public static void InitializeConfiguration(IssueIdentityAPIConfig config, ILogger logger, SqlIOHelperConfig sqlConfig)
		{
			if (Repository == null)
			{
				SqlTenantDatabaseHelper sqlTenantDatabaseHelper = new SqlTenantDatabaseHelper(sqlConfig);
				TenantManager = new SqlTenantManager(sqlTenantDatabaseHelper);

				dbIOHelper = new SqlIssueDbIOHelper(sqlConfig);
				SqlIssueDbProvider sqlDb = new SqlIssueDbProvider(dbIOHelper);

				SqlUserDbHelper sqlUserDbHelper = new SqlUserDbHelper(sqlConfig);
				UserManager = new SqlIssueIdentityUserManager(sqlUserDbHelper);			


				taggedIssueProvider = new TaggedIssueProvider(dbIOHelper, logger);

				word2Vec_Pinecone_VectorHelper = new Word2Vec_Pinecone_VectorHelper(config.pineconeconfigjson, logger);
				PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(word2Vec_Pinecone_VectorHelper, logger);
				IssueIdentityManager issueIdentityManager = new IssueIdentityManager(new IIssueIdentityProvider[] { taggedIssueProvider, pineconeIdentityProvider }, TenantManager, logger, pineconeIdentityProvider.GetType().Name);

				Repository = new IssueRepository(sqlDb, issueIdentityManager, logger, TenantManager);
			}
		}
	}
}
