using DupIQ.IssueIdentity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public class SqlTenantDatabaseHelper : ISqlTenantDatabaseHelper
	{
		private SqlIOHelperConfig config;

		public SqlTenantDatabaseHelper(SqlIOHelperConfig config)
		{
			this.config = config;
		}

		public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void AddOrUpdateProject(string tenantId, Project project)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateProject", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", project.ProjectId);
					command.Parameters.AddWithValue("@OwnerId", project.OwnerId);
					command.Parameters.AddWithValue("@Name", project.Name);
					command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
					command.Parameters.AddWithValue("@SimilarityThreshold", project.SimilarityThreshold);

					command.ExecuteNonQuery();
				}
			};
			AddOrUpdateProjectExtendedProperties(tenantId, project.ProjectId, new PropStuffer());
			AddOrUpdateProjectForUser(tenantId, project.ProjectId, project.OwnerId);
		}

		public void AddOrUpdateProjectExtendedProperties(string tenantId, string projectId, PropStuffer properties)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateProjectExtendedProperties", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);
					command.Parameters.AddWithValue("@PropertyJson", JsonSerializer.Serialize(properties));
					command.ExecuteNonQuery();
				}
			}
		}

		public void AddOrUpdateProjectForUser(string tenantId, string projectId, string userId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateTenantUserProject", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@ProjectId", projectId);
					command.Parameters.AddWithValue("@UserId", userId);
					command.Parameters.AddWithValue("@Role", UserTenantAuthorization.Admin.ToString());
					command.ExecuteNonQuery();
				}
			}
		}

		public void AddOrUpdateTenantConfiguration(TenantConfiguration tenantConfiguration)
		{
			string configurationJson = JsonSerializer.Serialize(tenantConfiguration.Configuration);
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateTenantConfigurations", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantConfiguration.TenantId);
					command.Parameters.AddWithValue("@PropertyJson", configurationJson);
					command.ExecuteNonQuery();
				}
			};
		}

		public void AddOrUpdateTenantProfile(TenantProfile tenantProfile)
		{
			if (tenantProfile == null) { throw new ArgumentNullException(); }
			if (string.IsNullOrEmpty(tenantProfile.TenantId)) { throw new ArgumentNullException(); }
			if (string.IsNullOrEmpty(tenantProfile.Name)) { throw new ArgumentNullException(); }
			if (string.IsNullOrEmpty(tenantProfile.OwnerId)) { throw new ArgumentNullException(); }
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateTenantProfile", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantProfile.TenantId);
					command.Parameters.AddWithValue("@Name", tenantProfile.Name);
					command.Parameters.AddWithValue("@OwnerId", tenantProfile.OwnerId);
					command.Parameters.AddWithValue("@BackupOwnerId", tenantProfile.OwnerId);
					command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
					command.ExecuteNonQuery();
				}
			};
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = tenantProfile.TenantId, Configuration = new Dictionary<string, string>() };
			AddOrUpdateTenantConfiguration(tenantConfiguration);
			AddOrUpdateTenantProfileToUserProfileList(tenantProfile.TenantId, tenantProfile.OwnerId, string.Empty, string.Empty, UserTenantAuthorization.Admin);
		}

		public void AddOrUpdateTenantProfileToUserProfileList(string tenantId, string userId, string userName, string email, UserTenantAuthorization auth)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateTenantUser", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@UserId", userId);
					command.Parameters.AddWithValue("@Role", auth.ToString());
					command.ExecuteNonQuery();
				}
			};
		}

		public void AddOrUpdateUserServiceAuthorization(string userId, UserServiceAuthorization authorization)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateServiceUserAuth", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@UserId", userId);
					command.Parameters.AddWithValue("@Role", authorization.ToString());
					command.ExecuteNonQuery();
				}
			}
		}

		public void AddOrUpdateUserTenantAuthorizaation(string tenantId, string userId, UserTenantAuthorization authorization)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateTenantUser", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@UserId", userId);
					command.Parameters.AddWithValue("@Role", authorization.ToString());
					command.ExecuteNonQuery();
				}
			}
		}

		public void ConfigureTenantDatabase()
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				CreateTable_Projects(connection);
				CreateTable_ProjectsExtendedProperties(connection);
				CreateTable_ServiceUserAuth(connection);
				CreateTable_Tenants(connection);
				CreateTable_TenantsConfigurations(connection);
				CreateTable_TenantUsers(connection);
				CreateTable_TenantUsersProjects(connection);

				CreateSproc_AddOrUpdateTenantUser(connection);
				CreateSproc_AddOrUpdateTenantUserProject(connection);
				CreateSproc_AddOrUpdateProject(connection);
				CreateSproc_AddOrUpdateProjectExtendedProperties(connection);
				CreateSproc_AddOrUpdateServiceUserAuth(connection);
				CreateSproc_AddOrUpdateTenantConfigurations(connection);
				CreateSproc_AddOrUpdateTenantProfile(connection);
			}
		}

		public void DeleteProjects(string tenantId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(@"
DELETE FROM Projects WHERE TenantId = @TenantId
DELETE FROM ProjectsExtendedProperties WHERE TenantId = @TenantId
DELETE FROM TenantsUsersProjects WHERE TenantId = @TenantId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);

					command.ExecuteNonQuery();
				}
			};
		}

		public void DeleteTenantConfiguration(string tenantId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("DELETE FROM TenantsConfigurations WHERE TenantId = @TenantId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);

					command.ExecuteNonQuery();
				}
			};
		}

		public void DeleteTenantProfile(string tenantId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(@"
DELETE FROM Tenants WHERE TenantId = @TenantId
DELETE FROM TenantUsers Where TenantId = @TenantId
DELETE FROM TenantsConfigurations WHERE TenantId = @TenantId
DELETE FROM TenantUsersProjects WHERE TenantId = @TenantId
DELETE FROM Projects WHERE TenantId = @TenantId
DELETE FROM ProjectsExtendedProperties WHERE TenantId = @TenantId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);

					command.ExecuteNonQuery();
				}
			};
		}

		public Project GetProject(string tenantId, string projectId)
		{
			Project result = new Project();
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM Projects WHERE TenantId = @TenantId AND ProjectId = @ProjectId", connection))
			{
				command.Parameters.AddWithValue("@TenantId", tenantId);
				command.Parameters.AddWithValue("@ProjectId", projectId);
				SqlDataReader sqlDataReader = command.ExecuteReader();
				while (sqlDataReader.Read())
				{
					result.TenantId = sqlDataReader["TenantId"].ToString().Trim();
					result.Name = sqlDataReader["Name"].ToString().Trim();
					result.ProjectId = sqlDataReader["ProjectId"].ToString().Trim();
					result.OwnerId = sqlDataReader["OwnerId"].ToString().Trim();
					float sim = 0.0f;
					if (float.TryParse(sqlDataReader["SimilarityThreshold"].ToString().Trim(), out sim))
					{
						result.SimilarityThreshold = sim;
					};
				}
				connection.Close();
				return result;
			}
		}

		public PropStuffer GetProjectExtendedProperties(string tenantId, string projectId)
		{
			PropStuffer result = null;
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM ProjectsExtendedProperties WHERE TenantId = @tenantId AND ProjectId = @projectId", connection))
			{
				command.Parameters.AddWithValue("@tenantId", tenantId);
				command.Parameters.AddWithValue("@projectId", projectId);
				SqlDataReader sqlDataReader = command.ExecuteReader();
				while (sqlDataReader.Read())
				{
					string propStuffBlob = sqlDataReader["PropertyJson"].ToString().Trim();
					result = JsonSerializer.Deserialize<PropStuffer>(propStuffBlob);
				}
				connection.Close();
				return result;
			}
		}

		public DbDataReader GetProjects(string tenantId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{ 
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM Projects WHERE TenantId = @TenantId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);

					return ExecuteProjectQueryAndReturnDataReader(command);
				}
			}

		}

		public DbDataReader GetProjects(string tenantId, string userId)
		{
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand(@"
SELECT 
  TUP.TenantId AS TenantId,
  TUP.ProjectId AS ProjectId,
  TUP.UserId AS UserId,
  TUP.Role AS Role,
  P.OwnerId AS OwnerId,
  P.Name AS Name
FROM TenantUsersProjects AS TUP 
INNER JOIN Projects AS P on TUP.TenantId = P.TenantId AND TUP.ProjectId = P.ProjectId 
WHERE TUP.TenantId = @TenantId AND TUP.UserId = @UserId", connection))
			{
				command.Parameters.AddWithValue("@TenantId", tenantId);
				command.Parameters.AddWithValue("@UserId", userId);
				return ExecuteProjectUsersQueryAndReturnDataReader(command);
			}
		}

		private static DbDataReader ExecuteProjectUsersQueryAndReturnDataReader(SqlCommand command)
		{
			SqlDataReader sqlDataReader = command.ExecuteReader();
			DataTable dataTable = CreateProjectUsersDataTabe();

			AddReaderRowToProjectUsersTable(sqlDataReader, dataTable);

			return dataTable.CreateDataReader();
		}

		private static DbDataReader ExecuteProjectQueryAndReturnDataReader(SqlCommand command)
		{
			SqlDataReader sqlDataReader = command.ExecuteReader();
			DataTable dataTable = CreateProjectDataTabe();

			AddReaderRowToProjectTable(sqlDataReader, dataTable);

			return dataTable.CreateDataReader();
		}

		private static DbDataReader ExecuteTenantQueryAndReturnDataReader(SqlCommand command)
		{
			SqlDataReader sqlDataReader = command.ExecuteReader();
			DataTable dataTable = CreateTenantDataTabe();

			AddReaderRowToTenantTable(sqlDataReader, dataTable);

			return dataTable.CreateDataReader();
		}

		private static DbDataReader ExecuteTenantUsersQueryAndReturnDataReader(SqlCommand command)
		{
			SqlDataReader sqlDataReader = command.ExecuteReader();
			DataTable dataTable = CreateTenantUsersDataTabe();

			AddReaderRowToTenantUsersTable(sqlDataReader, dataTable);

			return dataTable.CreateDataReader();
		}

		private static void AddReaderRowToProjectTable(SqlDataReader sqlDataReader, DataTable dataTable)
		{
			while (sqlDataReader.Read())
			{
				dataTable.Rows.Add(
					sqlDataReader["TenantId"].ToString().Trim(),
					sqlDataReader["ProjectId"].ToString().Trim(),
					sqlDataReader["OwnerId"].ToString().Trim(),
					sqlDataReader["Name"].ToString().Trim()
					);
			}
		}

		private static void AddReaderRowToProjectUsersTable(SqlDataReader sqlDataReader, DataTable dataTable)
		{
			while (sqlDataReader.Read())
			{
				dataTable.Rows.Add(
					sqlDataReader["TenantId"].ToString().Trim(),
					sqlDataReader["ProjectId"].ToString().Trim(),
					sqlDataReader["Name"].ToString().Trim(),
					sqlDataReader["UserId"].ToString().Trim(),
					sqlDataReader["OwnerId"].ToString().Trim(),
					sqlDataReader["Role"].ToString().Trim()
					);
			}
		}

		private static void AddReaderRowToTenantTable(SqlDataReader sqlDataReader, DataTable dataTable)
		{
			while (sqlDataReader.Read())
			{
				dataTable.Rows.Add(
					sqlDataReader["TenantId"].ToString().Trim(),
					sqlDataReader["OwnerId"].ToString().Trim(),
					sqlDataReader["Name"].ToString().Trim()
					);
			}
		}

		private static void AddReaderRowToTenantUsersTable(SqlDataReader sqlDataReader, DataTable dataTable)
		{
			while (sqlDataReader.Read())
			{
				dataTable.Rows.Add(
					sqlDataReader["TenantId"].ToString().Trim(),
					sqlDataReader["UserId"].ToString().Trim(),
					sqlDataReader["Role"].ToString().Trim()
					);
			}
		}

		private static DataTable CreateProjectDataTabe()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("TenantId", typeof(string));
			dataTable.Columns.Add("ProjectId", typeof(string));
			dataTable.Columns.Add("OwnerId", typeof(string));
			dataTable.Columns.Add("Name", typeof(string));
			return dataTable;
		}

		private static DataTable CreateProjectUsersDataTabe()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("TenantId", typeof(string));
			dataTable.Columns.Add("ProjectId", typeof(string));
			dataTable.Columns.Add("Name", typeof(string));
			dataTable.Columns.Add("UserId", typeof(string));
			dataTable.Columns.Add("OwnerId", typeof(string));
			dataTable.Columns.Add("Role", typeof(string));
			return dataTable;
		}
		private static DataTable CreateTenantDataTabe()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("TenantId", typeof(string));
			dataTable.Columns.Add("OwnerId", typeof(string));
			dataTable.Columns.Add("Name", typeof(string));
			return dataTable;
		}
		private static DataTable CreateTenantUsersDataTabe()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("TenantId", typeof(string));
			dataTable.Columns.Add("UserId", typeof(string));
			dataTable.Columns.Add("Role", typeof(string));
			return dataTable;
		}

		public DbDataReader GetTenantConfiguration(string tenantId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{ 
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM TenantsConfigurations WHERE TenantId = @TenantId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					SqlDataReader sqlDataReader = command.ExecuteReader();
					DataTable dataTable = new DataTable();
					dataTable.Columns.Add("TenantId", typeof(string));
					dataTable.Columns.Add("PropertyJson", typeof(string));

					while(sqlDataReader.Read())
					{
						dataTable.Rows.Add(sqlDataReader["TenantId"].ToString().Trim(), sqlDataReader["PropertyJson"].ToString().Trim());
					}

					return dataTable.CreateDataReader();
				}			
			}

		}

		public DbDataReader GetTenantProfile(string tenantId)
		{
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM Tenants WHERE TenantId = @TenantId", connection))
			{
				command.Parameters.AddWithValue("@TenantId", tenantId);
				return ExecuteTenantQueryAndReturnDataReader(command);
			}
		}

		public DbDataReader GetTenants()
		{
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM Tenants", connection))
			{
				return ExecuteTenantQueryAndReturnDataReader(command);
			}
		}

		public DbDataReader GetTenants(string userId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{ 
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT * FROM TenantUsers WHERE UserId = @UserId", connection))
				{
					command.Parameters.AddWithValue("@UserId", userId);
					return ExecuteTenantUsersQueryAndReturnDataReader(command);
				}			
			}

		}

		public DbDataReader GetUserServiceAuthorization(string userId)
		{
			using(SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using(SqlCommand command = new SqlCommand("SELECT Role FROM ServiceUserAuth WHERE UserId = @UserId", connection))
				{
					command.Parameters.AddWithValue("@UserId", userId);
					DataTable dataTable = new DataTable();
					dataTable.Columns.Add("Role", typeof(String));

					SqlDataReader sqlDataReader = command.ExecuteReader();
					while(sqlDataReader.Read())
					{
						dataTable.Rows.Add(sqlDataReader["Role"].ToString().Trim());
					}
					return dataTable.CreateDataReader();
				}
			}
		}

		public DbDataReader GetUserTenantAuthorization(string tenantId, string userId)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("SELECT Role FROM TenantUsers WHERE TenantId = @TenantId AND UserId = @UserId", connection))
				{
					command.Parameters.AddWithValue("@TenantId", tenantId);
					command.Parameters.AddWithValue("@UserId", userId);
					DataTable dataTable = new DataTable();
					dataTable.Columns.Add("Role", typeof(string));

					SqlDataReader sqlDataReader = command.ExecuteReader();
					while(sqlDataReader.Read())
					{
						dataTable.Rows.Add(sqlDataReader["Role"].ToString().Trim());
					}

					return dataTable.CreateDataReader();
				}
			}

		}

		public void PurgeTenants(bool purgeDependencies)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(@"
DELETE FROM Tenants WHERE TenantId IS NOT NULL
DELETE FROM TenantsConfigurations WHERE TenantId IS NOT NULL
DELETE FROM TenantUsers WHERE TenantId IS NOT NULL
DELETE FROM TenantUsersProjects WHERE TenantId IS NOT NULL
DELETE FROM Projects WHERE TenantId IS NOT NULL
DELETE FROM ProjectsExtendedProperties WHERE TenantId IS NOT NULL
DELETE FROM ServiceUserAuth WHERE UserId IS NOT NULL
", connection))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		#region create_sprocs

		private void CreateSproc_AddOrUpdateProjectExtendedProperties(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateProjectExtendedProperties') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateProjectExtendedProperties]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateProjectExtendedProperties]
    @TenantId nchar(50),
	@ProjectId nchar(50),
	@PropertyJson nvarchar(2048)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ProjectsExtendedProperties WHERE TenantId = @TenantId AND ProjectId = @ProjectId)
    BEGIN
        -- The User already exists, update the row
        UPDATE dbo.ProjectsExtendedProperties
        SET PropertyJson = @PropertyJson
        WHERE TenantId = @TenantId AND ProjectId = @ProjectId;
    END
    ELSE
    BEGIN
        -- The User does not exist, insert a new row
        INSERT INTO dbo.ProjectsExtendedProperties(TenantId, ProjectId, PropertyJson)
        VALUES (@TenantId, @ProjectId, @PropertyJson);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateServiceUserAuth(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateServiceUserAuth') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateServiceUserAuth]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateServiceUserAuth]
	@UserId nchar(50),
	@Role nchar(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.ServiceUserAuth WHERE UserId = @UserId)
    BEGIN
        -- The User already exists, update the row
        UPDATE dbo.ServiceUserAuth
        SET Role = @Role
        WHERE UserId = @UserId;
    END
    ELSE
    BEGIN
        -- The User does not exist, insert a new row
        INSERT INTO dbo.ServiceUserAuth(UserId, Role)
        VALUES (@UserId, @Role);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateTenantUser(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateTenantUser') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateTenantUser]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateTenantUser]
    @TenantId nchar(50),
	@UserId nchar(50),
	@Role nchar(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.TenantUsers WHERE TenantId = @TenantId AND UserId = @UserId)
    BEGIN
        -- The User already exists, update the row
        UPDATE dbo.TenantUsers
        SET Role = @Role
        WHERE TenantId = @TenantId AND UserId = @UserId;
    END
    ELSE
    BEGIN
        -- The User does not exist, insert a new row
        INSERT INTO dbo.TenantUsers(TenantId, UserId, Role)
        VALUES (@TenantId, @UserId, @Role);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateTenantUserProject(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateTenantUserProject') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateTenantUserProject]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateTenantUserProject]
    @TenantId nchar(50),
	@UserId nchar(50),
	@ProjectId nchar(50),
	@Role nchar(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.TenantUsersProjects WHERE TenantId = @TenantId AND UserId = @UserId AND ProjectId = @ProjectId)
    BEGIN
        -- The User already exists, update the row
        UPDATE dbo.TenantUsersProjects
        SET Role = @Role
        WHERE TenantId = @TenantId;
    END
    ELSE
    BEGIN
        -- The User does not exist, insert a new row
        INSERT INTO dbo.TenantUsersProjects(TenantId, UserId, ProjectId, Role)
        VALUES (@TenantId, @UserId, @ProjectId, @Role);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateTenantConfigurations(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateTenantConfigurations') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateTenantConfigurations]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateTenantConfigurations]
    @TenantId nchar(50),
	@PropertyJson nvarchar(2048) 
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.TenantsConfigurations WHERE TenantId = @TenantId)
    BEGIN
        -- The Tenant already exists, update the row
        UPDATE dbo.TenantsConfigurations
        SET PropertyJson = @PropertyJson
        WHERE TenantId = @TenantId;
    END
    ELSE
    BEGIN
        -- The Tenant does not exist, insert a new row
        INSERT INTO dbo.TenantsConfigurations(TenantId, PropertyJson)
        VALUES (@TenantId, @PropertyJson);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateTenantProfile(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateTenantProfile') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateTenantProfile]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateTenantProfile]
    @TenantId nchar(50),
	@Name nchar(50),
	@OwnerId nchar(50),
	@BackupOwnerId nchar(50),
	@CreatedDate datetime 
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Tenants WHERE TenantId = @TenantId)
    BEGIN
        -- The Tenant already exists, update the row
        UPDATE dbo.Tenants
        SET Name = @Name,
            OwnerId = @OwnerId,
            BackupOwnerId = @BackupOwnerId,
			CreatedDate = @CreatedDate
        WHERE TenantId = @TenantId;
    END
    ELSE
    BEGIN
        -- The Tenant does not exist, insert a new row
        INSERT INTO dbo.Tenants(TenantId, Name, OwnerId, BackupOwnerId, CreatedDate)
        VALUES (@TenantId, @Name, @OwnerId, @BackupOwnerId, @CreatedDate);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateProject(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateProject') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateProject]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateProject]
    @TenantId nchar(50),
    @ProjectId nchar(50),
	@Name nchar(50),
	@OwnerId nchar(50),
	@CreatedDate datetime,
	@SimilarityThreshold float
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Projects WHERE TenantId = @TenantId AND ProjectId = @ProjectId)
    BEGIN
        -- The IssueProfile already exists, update the row
        UPDATE dbo.Projects
        SET Name = @Name,
            OwnerId = @OwnerId,
			CreatedDate = @CreatedDate,
			SimilarityThreshold = @SimilarityThreshold
        WHERE TenantId = @TenantId AND ProjectId = @ProjectId;
    END
    ELSE
    BEGIN
        -- The IssueProfile does not exist, insert a new row
        INSERT INTO dbo.Projects(TenantId, ProjectId, Name, OwnerId, CreatedDate, SimilarityThreshold)
        VALUES (@TenantId, @ProjectId, @Name, @OwnerId, @CreatedDate, @SimilarityThreshold);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		#endregion

		#region create_tables

		private void CreateTable_Projects(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Projects', N'U') IS NOT NULL  
   DROP TABLE [dbo].[Projects];  

IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
CREATE TABLE [dbo].[Projects](
	[TenantId] [nchar](50) NOT NULL,
	[ProjectId] [nchar](50) NOT NULL,
	[Name] [nchar](50) NOT NULL,
	[OwnerId] [nchar](50) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[SimilarityThreshold] [float] NULL
 CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC, [ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateTable_ProjectsExtendedProperties(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.ProjectsExtendedProperties', N'U') IS NOT NULL  
   DROP TABLE [dbo].[ProjectsExtendedProperties];  

IF OBJECT_ID(N'dbo.ProjectsExtendedProperties', N'U') IS NULL
CREATE TABLE [dbo].[ProjectsExtendedProperties](
	[TenantId] [nchar](50) NOT NULL,
	[ProjectId] [nchar](50) NOT NULL,
	[PropertyJson] [nvarchar](2048) NOT NULL
 CONSTRAINT [PK_ProjectsExtendedProperties] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC, [ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateTable_ServiceUserAuth(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.ServiceUserAuth', N'U') IS NOT NULL  
   DROP TABLE [dbo].[ServiceUserAuth];  

IF OBJECT_ID(N'dbo.ServiceUserAuth', N'U') IS NULL
CREATE TABLE [dbo].[ServiceUserAuth](
	[UserId] [nchar](50) NOT NULL,
	[Role] [nchar](50) NOT NULL
 CONSTRAINT [PK_ServiceUserAuth] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateTable_Tenants(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Tenants', N'U') IS NOT NULL  
   DROP TABLE [dbo].[Tenants];  

IF OBJECT_ID(N'dbo.Tenants', N'U') IS NULL
CREATE TABLE [dbo].[Tenants](
	[TenantId] [nchar](50) NOT NULL,
	[Name] [nchar](50) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[OwnerId] [nchar](50) NOT NULL,
	[BackupOwnerId] [nchar](50) NOT NULL
 CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateTable_TenantsConfigurations(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.TenantsConfigurations', N'U') IS NOT NULL  
   DROP TABLE [dbo].[TenantsConfigurations];  

IF OBJECT_ID(N'dbo.TenantsConfigurations', N'U') IS NULL
CREATE TABLE [dbo].[TenantsConfigurations](
	[TenantId] [nchar](50) NOT NULL,
	[PropertyJson] [nvarchar](2048) NOT NULL
 CONSTRAINT [PK_TenantsConfigurations] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateTable_TenantUsers(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.TenantUsers', N'U') IS NOT NULL  
   DROP TABLE [dbo].[TenantUsers];  

IF OBJECT_ID(N'dbo.TenantUsers', N'U') IS NULL
CREATE TABLE [dbo].[TenantUsers](
	[TenantId] [nchar](50) NOT NULL,
	[UserId] [nchar](50) NOT NULL,
	[Role] [nchar](50) NOT NULL
 CONSTRAINT [PK_TenantUsers] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC, [UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private void CreateTable_TenantUsersProjects(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.TenantUsersProjects', N'U') IS NOT NULL  
   DROP TABLE [dbo].[TenantUsersProjects];  

IF OBJECT_ID(N'dbo.TenantUsersProjects', N'U') IS NULL
CREATE TABLE [dbo].[TenantUsersProjects](
	[TenantId] [nchar](50) NOT NULL,
	[UserId] [nchar](50) NOT NULL,
	[ProjectId] [nchar](50) NOT NULL,
	[Role] [nchar](50) NOT NULL
 CONSTRAINT [PK_TenantUsersProjects] PRIMARY KEY CLUSTERED 
(
	[TenantId] ASC, [UserId] ASC, [ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		#endregion

		private SqlConnection GetTenantDBConnection()
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
			builder.DataSource = config.ServerName;
			builder.InitialCatalog = config.DatabaseName;
			builder.UserID = config.AccountName;
			builder.Password = config.Password;
			builder.Authentication = SqlAuthenticationMethod.SqlPassword;
			builder.Encrypt = false;
			builder.TrustServerCertificate = true;
			builder.Pooling = false;

			SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);
			return sqlConnection;
		}
	}
}
