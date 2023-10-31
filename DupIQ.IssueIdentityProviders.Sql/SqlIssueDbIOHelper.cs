using DupIQ.IssueIdentity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public class SqlIssueDbIOHelper : IIssueDbIOHelper
	{
		private DateTime mintime = new DateTime(1753, 1, 1, 0, 0, 0);
		private ILogger _logger;

		SqlIOHelperConfig config;

		public SqlIssueDbIOHelper(SqlIOHelperConfig conf) : this(conf, (ILogger?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<SqlIssueDbIOHelper>())
		{ }

		public SqlIssueDbIOHelper(SqlIOHelperConfig config, ILogger logger)
		{
			this.config = config;
			_logger = logger;
		}

		public SqlIssueDbIOHelper(string configJson)
		{
			InitializeHelper(configJson, (ILogger?)LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<SqlIssueDbIOHelper>());
		}

		public SqlIssueDbIOHelper(string configJson, ILogger logger)
		{

			InitializeHelper(configJson, logger);
		}

		public void AddIssueProfile(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (issueProfile == null || tenantConfiguration == null)
			{
				throw new ArgumentNullException();
			}
			AddIssueProfile(issueProfile, tenantConfiguration.TenantId, projectId);
		}

		public void AddIssueReport(IssueReport issueReport, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (tenantConfiguration == null || issueReport == null)
			{
				throw new ArgumentNullException();
			}
			if (string.IsNullOrEmpty(issueReport.IssueId)
				|| string.IsNullOrEmpty(issueReport.IssueMessage)
				|| string.IsNullOrEmpty(issueReport.InstanceId)
				|| string.IsNullOrEmpty(projectId)
				|| string.IsNullOrEmpty(tenantConfiguration.TenantId))
			{
				throw new ArgumentException();
			}
			if (issueReport.IssueDate < mintime)
			{
				throw new ArgumentException($"IssueDate must be less than Sql min date time {mintime}.", "issueReport.IssueDate");
			}


			using (SqlConnection connection = CreateSqlConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("InsertOrUpdateIssueReport", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantConfiguration.TenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);
					command.Parameters.AddWithValue("@InstanceId", issueReport.InstanceId);
					command.Parameters.AddWithValue("@IssueId", issueReport.IssueId);
					command.Parameters.AddWithValue("@IssueMessage", issueReport.IssueMessage);
					command.Parameters.AddWithValue("@IssueDate", issueReport.IssueDate);

					command.ExecuteNonQuery();
				}
			}
		}

		public void ConfigureDatabaseServer()
		{
			using (SqlConnection connect = CreateSqlConnection())
			{
				connect.Open();
				CreateIssueProfilesTable(connect);
				CreateIssueReportsTable(connect);
				CreateInsertOrUpdateIssueReportStoredProcedure(connect);
				CreateInsertOrUpdateIssueProfileStoredProcedure(connect);
			}
		}

		public void DeleteIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			using (SqlConnection connection = CreateSqlConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(@"DELETE FROM IssueProfiles WHERE TenantID = @TenantId AND ProjectId = @ProjectId AND ID = @Id", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantConfiguration.TenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);
					command.Parameters.AddWithValue("@Id", issueId);
					command.ExecuteNonQuery();
				}
			}
		}

		public void DeleteIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			using (SqlConnection connection = CreateSqlConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(@"DELETE FROM IssueReports WHERE TenantID = @TenantId AND ProjectId = @ProjectId AND InstanceId = @InstanceId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantConfiguration.TenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);
					command.Parameters.AddWithValue("@InstanceId", instanceId);
					command.ExecuteNonQuery();
				}
			}
		}

		public DbDataReader GetIssueProfile(string issueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (tenantConfiguration == null)
			{
				throw new ArgumentNullException();
			}
			return GetIssueProfile(issueId, tenantConfiguration.TenantId, projectId);
		}

		public DbDataReader GetIssueProfiles(TenantConfiguration tenantConfiguration, string projectId)
		{
			if (tenantConfiguration == null)
			{
				throw new ArgumentNullException();
			}
			return GetIssueProfiles(tenantConfiguration.TenantId, projectId);
		}

		public DbDataReader GetIssueReport(string instanceId, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (tenantConfiguration == null)
			{
				throw new ArgumentNullException();
			}
			return GetIssueReport(instanceId, tenantConfiguration.TenantId, projectId);
		}

		public DbDataReader GetIssueReports(IssueProfile issueProfile, TenantConfiguration tenantConfiguration, string projectId)
		{
			if (issueProfile == null || tenantConfiguration == null)
			{
				throw new ArgumentNullException();
			}
			return GetIssueReports(issueProfile, tenantConfiguration.TenantId, projectId);
		}

		public void PurgeIssueProfiles(TenantConfiguration tenantConfiguration, string projectId)
		{
			PurgeIssueProfiles(tenantConfiguration.TenantId, projectId);
		}

		public void PurgeIssueReports(TenantConfiguration tenantConfiguration, string projectId)
		{
			PurgeIssueReports(tenantConfiguration.TenantId, projectId);
		}

		public void PurgeIssueProfiles(string tenantId, string projectId)
		{
			using (SqlConnection connection = CreateSqlConnection())
			{
				using (SqlCommand command = new SqlCommand("DELETE FROM IssueProfiles WHERE TenantId = @TenantId AND ProjectId = @ProjectId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					connection.Open();
					command.ExecuteNonQuery();
				}
			}
		}

		public void PurgeIssueReports(string tenantId, string projectId)
		{
			using (SqlConnection connection = CreateSqlConnection())
			{
				using (SqlCommand command = new SqlCommand("DELETE FROM IssueReports WHERE TenantId = @TenantId AND ProjectId = @ProjectId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					connection.Open();
					command.ExecuteNonQuery();
				}
			}
		}

		public void UpdateIssueReportIssueId(string instanceId, string IssueId, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		private void AddIssueProfile(IssueProfile issueProfile, string tenantId, string projectId)
		{
			if (string.IsNullOrEmpty(issueProfile.ExampleMessage) || string.IsNullOrEmpty(issueProfile.IssueId) || string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentException();
			}
			if (issueProfile.FirstReportedDate < mintime)
			{
				throw new ArgumentException($"FirstReportedDate must be greater than Sql min date time {mintime}.", "issueProfile.FirstReportedDate");
			}

			using (SqlConnection connection = CreateSqlConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("InsertOrUpdateIssueProfile", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@Id", issueProfile.IssueId);
					command.Parameters.AddWithValue("@ExampleMessage", issueProfile.ExampleMessage);
					command.Parameters.AddWithValue("@FirstReportedDate", issueProfile.FirstReportedDate);
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					command.ExecuteNonQuery();
				}
			}
		}

		private static void CreateInsertOrUpdateIssueReportStoredProcedure(SqlConnection connect)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'InsertOrUpdateIssueReport') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[InsertOrUpdateIssueReport]

exec('CREATE PROCEDURE [dbo].[InsertOrUpdateIssueReport]
    @TenantId nchar(50),
    @ProjectId nchar(50),
    @InstanceId nchar(50),
	@IssueId nchar(50),
    @IssueMessage nvarchar(2048),
    @IssueDate datetime
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.IssueReports WHERE InstanceId = @InstanceId AND TenantId = @TenantId AND ProjectId = @ProjectId)
    BEGIN
        -- The IssueProfile already exists, update the row
        UPDATE dbo.IssueReports
        SET IssueMessage = @IssueMessage,
            IssueDate = @IssueDate,
			IssueId = @IssueId
        WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND InstanceId = @InstanceId;
    END
    ELSE
    BEGIN
        -- The IssueProfile does not exist, insert a new row
        INSERT INTO dbo.IssueReports(TenantId, ProjectId, InstanceId, IssueId, IssueMessage, IssueDate)
        VALUES (@TenantId, @ProjectId, @InstanceId, @IssueId, @IssueMessage, @IssueDate);
    END
END')
"
,
					connect))
			{
				command.ExecuteNonQuery();
			}
		}

		private static void CreateInsertOrUpdateIssueProfileStoredProcedure(SqlConnection connect)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'InsertOrUpdateIssueProfile') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[InsertOrUpdateIssueProfile]

exec('CREATE PROCEDURE [dbo].[InsertOrUpdateIssueProfile]
    @TenantId nchar(50),
    @ProjectId nchar(50),
    @Id nchar(50),
    @ExampleMessage nvarchar(2048),
    @FirstReportedDate datetime
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.IssueProfiles WHERE Id = @Id AND TenantId = @TenantId AND ProjectId = @ProjectId)
    BEGIN
        -- The IssueProfile already exists, update the row
        UPDATE dbo.IssueProfiles
        SET ExampleMessage = @ExampleMessage,
            FirstReportedDate = @FirstReportedDate
        WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Id = @Id;
    END
    ELSE
    BEGIN
        -- The IssueProfile does not exist, insert a new row
        INSERT INTO dbo.IssueProfiles (TenantId, ProjectId, Id, ExampleMessage, FirstReportedDate)
        VALUES (@TenantId, @ProjectId, @Id, @ExampleMessage, @FirstReportedDate);
    END
END')
"
,
					connect))
			{
				command.ExecuteNonQuery();
			}
		}

		private static void CreateIssueReportsTable(SqlConnection connect)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.IssueReports', N'U') IS NOT NULL  
   DROP TABLE [dbo].[IssueReports];  

IF OBJECT_ID(N'dbo.IssueReports', N'U') IS NULL
CREATE TABLE [dbo].[IssueReports](
	[TenantId] [nchar](50) NOT NULL,
	[ProjectId] [nchar](50) NOT NULL,
	[InstanceId] [nchar](50) NOT NULL,
	[IssueId] [nchar](50) NULL,
	[IssueMessage] [nvarchar](2048) NULL,
	[IssueDate] [datetime] NULL, 
 CONSTRAINT [PK_IssueReports] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC, [ProjectId] ASC,[InstanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connect))
			{
				command.ExecuteNonQuery();
			}
		}

		private static void CreateIssueProfilesTable(SqlConnection connect)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.IssueProfiles', N'U') IS NOT NULL  
   DROP TABLE [dbo].[IssueProfiles];  

IF OBJECT_ID(N'dbo.IssueProfiles', N'U') IS NULL
CREATE TABLE [dbo].[IssueProfiles](
	[TenantId] [nchar](50) NOT NULL,
	[ProjectId] [nchar](50) NOT NULL,
	[Id] [nchar](50) NOT NULL,
	[ExampleMessage] [nvarchar](2048) NULL,
	[FirstReportedDate] [datetime] NULL,
 CONSTRAINT [PK_IssueProfiles] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC, [ProjectId] ASC, [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connect))
			{
				command.ExecuteNonQuery();
			}
		}

		private SqlConnection CreateSqlConnection()
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
			builder.DataSource = config.ServerName;
			builder.InitialCatalog = config.DatabaseName;
			builder.UserID = config.AccountName;
			builder.Password = config.Password;
			builder.Authentication = SqlAuthenticationMethod.SqlPassword;
			builder.Encrypt = false;
			builder.Pooling = false;

			SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);
			return sqlConnection;
		}

		private DbDataReader GetIssueProfile(string issueId, string tenantId, string projectId)
		{
			if (string.IsNullOrEmpty(issueId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentException();
			}
			using (SqlConnection connection = CreateSqlConnection())
			{ 
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM IssueProfiles WHERE Id = @Id AND TenantId = @TenantId AND ProjectId = @ProjectId", connection))
				{

					command.Parameters.AddWithValue("@Id", issueId);
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					return RunIssueProfileQueryAndReturnDataReader(command);
				}
			}
		}

		private static DbDataReader RunIssueProfileQueryAndReturnDataReader(SqlCommand command)
		{
			SqlDataReader sqlDataReader = command.ExecuteReader();
			DataTable dataTable = CreateIssueProfileDataTable();
			while (sqlDataReader.Read())
			{
				AddSqlDataReaderRowToIssueProfileTable(sqlDataReader, dataTable);
			}
			return dataTable.CreateDataReader();
		}

		private static DbDataReader RunIssueReportsQueryAndReturnDataReader(SqlCommand command)
		{
			SqlDataReader sqlDataReader = command.ExecuteReader();
			DataTable dataTable = CreateIssueReportsDataTable();
			while (sqlDataReader.Read())
			{
				AddSqlDataReaderRowToIssueReportsTable(sqlDataReader, dataTable);
			}
			return dataTable.CreateDataReader();
		}

		private static DataRow AddSqlDataReaderRowToIssueProfileTable(SqlDataReader sqlDataReader, DataTable dataTable)
		{
			return dataTable.Rows.Add(
										sqlDataReader["Id"].ToString().Trim(),
										sqlDataReader["ExampleMessage"].ToString().Trim(),
										Convert.ToDateTime(sqlDataReader["FirstReportedDate"].ToString().Trim())
										);
		}

		private static DataRow AddSqlDataReaderRowToIssueReportsTable(SqlDataReader sqlDataReader, DataTable dataTable)
		{
			return dataTable.Rows.Add(
										sqlDataReader["InstanceId"].ToString().Trim(),
										sqlDataReader["IssueId"].ToString().Trim(),
										sqlDataReader["IssueMessage"].ToString().Trim(),
										Convert.ToDateTime(sqlDataReader["IssueDate"].ToString().Trim())
										);
		}

		private static DataTable CreateIssueProfileDataTable()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("Id", typeof(string));
			dataTable.Columns.Add("ExampleMessage", typeof(string));
			dataTable.Columns.Add("FirstReportedDate", typeof(DateTime));
			return dataTable;
		}
		private static DataTable CreateIssueReportsDataTable()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("InstanceId", typeof(string));
			dataTable.Columns.Add("IssueId", typeof(string));
			dataTable.Columns.Add("IssueMessage", typeof(string));
			dataTable.Columns.Add("IssueDate", typeof(DateTime));
			return dataTable;
		}

		private DbDataReader GetIssueProfiles(string tenantId, string projectId)
		{
			if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentException();
			}
			using (SqlConnection connection = CreateSqlConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM IssueProfiles WHERE TenantId = @TenantId AND ProjectId = @ProjectId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					return RunIssueProfileQueryAndReturnDataReader(command);
				}
			}
		}

		private DbDataReader GetIssueReport(string instanceId, string tenantId, string projectId)
		{
			if (string.IsNullOrEmpty(instanceId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentException();
			}
			using (SqlConnection connection = CreateSqlConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM IssueReports WHERE InstanceId = @InstanceId AND TenantId = @TenantId AND ProjectId = @ProjectId", connection))
				{

					command.Parameters.AddWithValue("@InstanceId", instanceId);
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					return RunIssueReportsQueryAndReturnDataReader(command);
				}
			}

		}

		private DbDataReader GetIssueReports(IssueProfile issueProfile, string tenantId, string projectId)
		{
			if (issueProfile == null)
			{
				throw new ArgumentNullException();
			}
			if (string.IsNullOrEmpty(issueProfile.IssueId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(projectId))
			{
				throw new ArgumentException();
			}
			using (SqlConnection connection = CreateSqlConnection()) 
			{ 
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM IssueReports WHERE IssueId = @IssueId AND TenantId = @TenantId AND ProjectId = @ProjectId", connection))
				{

					command.Parameters.AddWithValue("@IssueId", issueProfile.IssueId);
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);

					return RunIssueReportsQueryAndReturnDataReader(command);
				}
			}
		}

		private void InitializeHelper(string configJson, ILogger logger)
		{
			_logger = logger;
			_logger.LogInformation(SharedEvents.ProviderStartup, " sql config json: {ConfigJson}", configJson);
			config = JsonSerializer.Deserialize<SqlIOHelperConfig>(configJson);
		}
	}

	public class SqlIOHelperConfig
	{
		public string ConnectionString { get; set; }
		public string ServerName { get; set; }
		public string DatabaseName { get; set; }
		public string AccountName { get; set; }
		public string Password { get; set; }

	}
}
