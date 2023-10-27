using DupIQ.IssueIdentity;
using System.Data.Common;
using System.Text.Json;

namespace DupIQ.IssueIdentityProviders.Sql
{
	public class SqlTenantManager : ITenantManager
	{
		public const string DefaultTenantId = "DEFAULTTENANT";
		private ISqlTenantDatabaseHelper _databaseHelper;


		public SqlTenantManager(ISqlTenantDatabaseHelper databaseHelper)
		{
			_databaseHelper = databaseHelper;
		}

		public void AddOrUpdateProjectExtendedProperties<T>(string tenantId, string projectId, T properties)
		{
			PropStuffer props = new PropStuffer();
			string propTypeName = typeof(T).Name;
			if (props.PropertyBag.ContainsKey(propTypeName))
			{
				props.PropertyBag[propTypeName] = properties;
			}
			else
			{
				props.AddProperties(properties);
			}
			_databaseHelper.AddOrUpdateProjectExtendedProperties(tenantId, projectId, props);
		}

		public void AddProject(string tenantId, Project project)
		{
			project.ProjectId = project.ProjectId.Trim();
			_databaseHelper.AddOrUpdateProject(tenantId, project);
		}

		public string AddTenant(TenantProfile tenantProfile)
		{
			tenantProfile.TenantId = tenantProfile.TenantId.Trim();
			_databaseHelper.AddOrUpdateTenantProfile(tenantProfile);
			return tenantProfile.TenantId;
		}

		public void AddTenantProfileToUserIdentityTenantList(string tenantId, string userId, string userName, string email, UserTenantAuthorization auth)
		{
			_databaseHelper.AddOrUpdateTenantProfileToUserProfileList(tenantId, userId, userName, email, auth);
		}

		public void AddUserTenantAuthorization(string tenantId, string userId, UserTenantAuthorization authorization)
		{
			_databaseHelper.AddOrUpdateUserTenantAuthorizaation(tenantId, userId, authorization);
		}

		public string DefaultTenant()
		{
			return DefaultTenantId;
		}

		public Project GetProject(string tenantId, string projectId)
		{
			return _databaseHelper.GetProject(tenantId, projectId);
		}

		public TenantConfiguration GetTenantConfiguration(string tenantId)
		{
			TenantConfiguration tenantConfiguration = new TenantConfiguration();
			using (DbDataReader reader = _databaseHelper.GetTenantConfiguration(tenantId))
			{
				while (reader.Read())
				{
					tenantConfiguration.TenantId = reader["TenantId"].ToString().Trim();
					tenantConfiguration.Configuration = JsonSerializer.Deserialize<Dictionary<string, string>>(reader["PropertyJson"].ToString().Trim());
					break;
				}
			}
			return tenantConfiguration;
		}

		public TenantProfile GetTenantProfile(string tenantId)
		{
			TenantProfile tenantProfile = new TenantProfile();
			using (DbDataReader reader = _databaseHelper.GetTenantProfile(tenantId))
			{
				while (reader.Read())
				{
					PopulateTenantProfileFromReader(tenantProfile, reader);
				}
			}
			return tenantProfile;
		}

		public string[] GetTenants()
		{
			List<string> tenants = new List<string>();
			using (DbDataReader reader = _databaseHelper.GetTenants())
			{
				while (reader.Read())
				{
					tenants.Add(reader["TenantId"].ToString().Trim());
				}
				return tenants.ToArray();
			}
		}

		public UserTenantAuthorization GetUserTenantAuthorization(string tenantId, string userId)
		{
			UserTenantAuthorization result = UserTenantAuthorization.None;
			using (DbDataReader reader = _databaseHelper.GetUserTenantAuthorization(tenantId, userId))
			{
				while (reader.Read())
				{
					string role = reader["Role"].ToString().Trim().ToLowerInvariant();
					switch (role)
					{
						case "admin":
							{
								result = UserTenantAuthorization.Admin;
								break;
							}
						case "developer":
							{
								result = UserTenantAuthorization.Developer;
								break;
							}
						case "writer":
							{
								result = UserTenantAuthorization.Writer;
								break;
							}
						case "reader":
							{
								result = UserTenantAuthorization.Reader;
								break;
							}
					}
				}
			}
			return result;
		}

		public void UpdateTenantConfiguration(TenantConfiguration tenantConfiguration)
		{
			_databaseHelper.AddOrUpdateTenantConfiguration(tenantConfiguration);
		}

		public void UpdateTenantProfile(TenantProfile tenantProfile)
		{
			AddTenant(tenantProfile);
		}

		private static void PopulateTenantProfileFromReader(TenantProfile tenantProfile, DbDataReader reader)
		{
			tenantProfile.TenantId = reader["TenantId"].ToString().Trim();
			tenantProfile.Name = reader["Name"].ToString().Trim();
			tenantProfile.OwnerId = reader["OwnerId"].ToString().Trim();
		}

		public void AddProjectForUser(string tenantId, string userId, string projectID)
		{
			_databaseHelper.AddOrUpdateProjectForUser(tenantId, projectID, userId);
		}

		public void AddTenantProfileToUserTenantList(string userId, string tenantId, UserTenantAuthorization authorization)
		{
			throw new NotImplementedException();
		}

		public void DeleteTenant(string tenantId)
		{
			_databaseHelper.DeleteTenantProfile(tenantId);
		}

		public string GenerateApiKey(string tenantId)
		{
			return GenerateApiKey(tenantId, string.Empty);
		}

		public string GenerateApiKey(string tenantId, string projectId)
		{
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())); ;
		}

		public T GetProjectExtendedProperties<T>(string tenantId, string projectId)
		{
			PropStuffer props = _databaseHelper.GetProjectExtendedProperties(tenantId, projectId);
			return props.GetThingFromProperties<T>();
		}

		public string[] GetProjectExtendedPropertyTypeNames(string tenantId, string projectId)
		{
			PropStuffer props = _databaseHelper.GetProjectExtendedProperties(tenantId, projectId);
			return props.PropertyBag.Keys.ToArray();
		}

		public Project[] GetProjects(string tenantId)
		{
			List<Project> projects = new List<Project>();
			using (DbDataReader reader = _databaseHelper.GetProjects(tenantId))
			{
				while (reader.Read())
				{
					Project project = new Project()
					{
						TenantId = reader["TenantId"].ToString().Trim(),
						ProjectId = reader["ProjectId"].ToString().Trim(),
						OwnerId = reader["OwnerId"].ToString().Trim(),
						Name = reader["Name"].ToString().Trim(),
					};
					projects.Add(project);
				}
			};
			return projects.ToArray();
		}

		public Project[] GetProjects(string tenantId, string userId)
		{
			List<Project> projects = new List<Project>();
			using (DbDataReader reader = _databaseHelper.GetProjects(tenantId, userId))
			{
				while (reader.Read())
				{
					Project project = new Project()
					{
						TenantId = reader["TenantId"].ToString().Trim(),
						ProjectId = reader["ProjectId"].ToString().Trim(),
						OwnerId = reader["OwnerId"].ToString().Trim(),
						Name = reader["Name"].ToString().Trim(),
					};
					projects.Add(project);
				}
			};
			return projects.ToArray();
		}

		public TenantProfile[] GetTenants(string userId)
		{
			List<TenantProfile> tenants = new List<TenantProfile>();
			using (DbDataReader reader = _databaseHelper.GetTenants(userId))
			{
				while (reader.Read())
				{
					TenantProfile tenantProfile = new TenantProfile();
					PopulateTenantProfileFromReader(tenantProfile, reader);
					tenants.Add(tenantProfile);
				}
				return tenants.ToArray();
			}
		}

		public void UpdateProject(string tenantId, Project project)
		{
			throw new NotImplementedException();
		}

		public T GetProjectExtendedProperties<T>(string projectId)
		{
			throw new NotImplementedException();
		}

		public string[] GetProjectExtendedPropertyTypeNames(string projectId)
		{
			throw new NotImplementedException();
		}

		TenantProfile ITenantManager.GetTenantProfile(string tenantId)
		{
			TenantProfile result = new TenantProfile();
			using (DbDataReader reader = _databaseHelper.GetTenantProfile(tenantId))
			{
				while (reader.Read())
				{
					result.TenantId = reader["TenantId"].ToString().Trim();
					result.Name = reader["Name"].ToString().Trim();
					break;
				}
				return result;
			}
		}

	}
}
