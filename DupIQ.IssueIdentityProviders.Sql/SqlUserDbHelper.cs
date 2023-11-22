using DupIQ.IssueIdentity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public class SqlUserDbHelper : ISqlUserDbHelper
	{
		private SqlIOHelperConfig _sqlConfig;

		public SqlUserDbHelper(SqlIOHelperConfig sqlIOHelperConfig)
		{
			_sqlConfig = sqlIOHelperConfig;
		}

		public void AddOrUpdateUser(IssueIdentityUser user)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateUser", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@UserId", user.Id);
					command.Parameters.AddWithValue("@UserName", user.Name);
					command.Parameters.AddWithValue("@FirstName", user.FirstName);
					command.Parameters.AddWithValue("@LastName", user.LastName);
					command.Parameters.AddWithValue("@Email", user.Email);
					command.Parameters.AddWithValue("@Status", IssueIdentityUserStatus.Active.ToString());
					command.Parameters.AddWithValue("@CreateDate", DateTime.Now);
					command.ExecuteNonQuery();
				}
			}
		}

		public void AddOrUpdateUserPasswordHash(string userId, string passwordHash)
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("AddOrUpdateUserPasswordHashes", connection) { CommandType = CommandType.StoredProcedure })
				{
					command.Parameters.AddWithValue("@UserId", userId);
					command.Parameters.AddWithValue("@PasswordHash", passwordHash);
					command.ExecuteNonQuery();
				}
			}
		}

		public void ConfigureDatabase()
		{
			using (SqlConnection connection = GetTenantDBConnection())
			{
				connection.Open();
				CreateTable_PasswordHashes(connection);
				CreateTable_Users(connection);
				CreateSproc_AddOrUpdateUser(connection);
				CreateSproc_AddOrUpdateUserPasswordHashes(connection);
			}
		}

		public void DeleteUser(string userId)
		{
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("DELETE FROM IssueIdentityUsers WHERE UserId=@UserId", connection))
			{
				command.Parameters.AddWithValue("@UserId", userId);
				command.ExecuteNonQuery();
			}
		}

		public DbDataReader GetUser(string userId)
		{
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM IssueIdentityUsers WHERE UserId=@UserId", connection))
			{
				command.Parameters.AddWithValue("@UserId", userId);
				SqlDataReader reader = command.ExecuteReader();
				return reader;
			}
		}

		public string GetUserPasswordHash(string userId)
		{
			string result = string.Empty;
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM IssueIdentityUserPasswordHashes WHERE UserId=@UserId", connection))
			{
				command.Parameters.AddWithValue("@UserId", userId);
				SqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					result = reader["PasswordHash"].ToString().Trim();
				}
				return result;
			}
		}

		public bool UserExists(string userId)
		{
			using (DbDataReader reader = GetUser(userId))
			{
				while (reader.Read())
				{
					if (userId.Equals(reader["UserId"].ToString().Trim()))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool UserNameAvailable(string userName)
		{
			SqlConnection connection = GetTenantDBConnection();
			connection.Open();
			using (SqlCommand command = new SqlCommand("SELECT * FROM IssueIdentityUsers WHERE UserName=@UserName", connection))
			{
				command.Parameters.AddWithValue("@UserName", userName);
				SqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					if (userName.Equals(reader["UserName"].ToString().Trim()))
					{
						return false;
					}
				}
				return true;
			}
		}

		private void CreateSproc_AddOrUpdateUserPasswordHashes(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateUserPasswordHashes') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateUserPasswordHashes]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateUserPasswordHashes]
    @UserId nchar(50),
    @PasswordHash nchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.IssueIdentityUserPasswordHashes WHERE UserId = @UserId)
    BEGIN
        -- The User already exists, update the row
        UPDATE dbo.IssueIdentityUserPasswordHashes
        SET PasswordHash = @PasswordHash
        WHERE UserId = @UserId;
    END
    ELSE
    BEGIN
        -- The User does not exist, insert a new row
        INSERT INTO dbo.IssueIdentityUserPasswordHashes(UserId, PasswordHash)
        VALUES (@UserId, @PasswordHash);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}

		private void CreateSproc_AddOrUpdateUser(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(
@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'AddOrUpdateUser') AND type in (N'P', N'PC'))
  DROP PROCEDURE [dbo].[AddOrUpdateUser]

exec('CREATE PROCEDURE [dbo].[AddOrUpdateUser]
    @UserId nchar(50),
    @UserName nchar(50),
	@FirstName nchar(50),
	@LastName nchar(50),
	@Email nchar(50),
	@Status nchar(50),
	@CreateDate datetime
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.IssueIdentityUsers WHERE UserId = @UserId)
    BEGIN
        -- The User already exists, update the row
        UPDATE dbo.IssueIdentityUsers
        SET UserName = @UserName,
            FirstName = @FirstName,
            LastName = @LastName,
            Email = @Email,
            Status = @Status,
			CreateDate = @CreateDate
        WHERE UserId = @UserId;
    END
    ELSE
    BEGIN
        -- The User does not exist, insert a new row
        INSERT INTO dbo.IssueIdentityUsers(UserId, UserName, FirstName, LastName, Email, Status, CreateDate)
        VALUES (@UserId, @UserName, @FirstName, @LastName, @Email, @Status, @CreateDate);
    END
END')
"
,
		connection))
			{
				command.ExecuteNonQuery();
			}

		}


		private void CreateTable_PasswordHashes(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.IssueIdentityUserPasswordHashes', N'U') IS NOT NULL  
   DROP TABLE [dbo].[IssueIdentityUserPasswordHashes];  

CREATE TABLE [dbo].[IssueIdentityUserPasswordHashes](
	[UserId] [nchar](50) NOT NULL,
	[PasswordHash] [nchar](100) NOT NULL
 CONSTRAINT [PK_IssueIdentityUserPasswordHashes] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}
		private void CreateTable_Users(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.IssueIdentityUsers', N'U') IS NOT NULL  
   DROP TABLE [dbo].[IssueIdentityUsers];  

CREATE TABLE [dbo].[IssueIdentityUsers](
	[UserId] [nchar](50) NOT NULL,
	[UserName] [nchar](50) NOT NULL,
	[FirstName] [nchar](50) NOT NULL,
	[LastName] [nchar](50) NOT NULL,
	[Email] [nchar](50) NOT NULL,
	[Status] [nchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_IssueIdentityUsers] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [AK_IssueIdentityUserNames] UNIQUE NONCLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
",
					connection))
			{
				command.ExecuteNonQuery();
			}
		}

		private SqlConnection GetTenantDBConnection()
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
			builder.DataSource = _sqlConfig.ServerName;
			builder.InitialCatalog = _sqlConfig.DatabaseName;
			builder.UserID = _sqlConfig.AccountName;
			builder.Password = _sqlConfig.Password;
			builder.Authentication = SqlAuthenticationMethod.SqlPassword;
			builder.Encrypt = false;
			builder.TrustServerCertificate = true;

			SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);
			return sqlConnection;
		}
	}
}
