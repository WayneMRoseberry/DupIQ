using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DupIQ.IssueIdentityAPI.Controllers
{
	[Microsoft.AspNetCore.Authorization.Authorize]
	[Microsoft.AspNetCore.Mvc.Route("[controller]")]
	[ApiController]
	public class IssueIdentityUserController : IssueIdentityControllerBaseClass
	{
		IConfiguration _configuration;
		public IssueIdentityUserController(IConfiguration configuration, ILogger<IssueIdentityUserController> logger)
		{
			this.logger = logger;
			this._configuration = configuration;
			GlobalConfiguration.InitializeConfiguration(_configuration, logger);
		}

		[Microsoft.AspNetCore.Mvc.HttpGet]
		public IssueIdentityUser Get(string userId)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				return GlobalConfiguration.UserManager.GetUserById(userId);
			}
			return null;
		}

		[Microsoft.AspNetCore.Mvc.HttpPost]
		public string AddUser(IssueIdentityUser user)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				return GlobalConfiguration.UserManager.AddOrUpdateUser(user);
			}
			return string.Empty;

		}

		[Microsoft.AspNetCore.Mvc.HttpPost("password")]
		public void SetPassword(string userId,string password)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				IssueIdentityUser user = GlobalConfiguration.UserManager.GetUserById(userId);
				PasswordHasher<IssueIdentityUser> passwordHasher = new PasswordHasher<IssueIdentityUser>();
				string hashedPassword = passwordHasher.HashPassword(user, password);

				GlobalConfiguration.UserManager.AddOrUpdateUserPasswordHash(userId, hashedPassword);
			}
		}

		[Microsoft.AspNetCore.Mvc.HttpDelete]
		public void DeleteUser(string userId)
		{
			if (CheckIfBelowServiceAuthorizationLevel(GetUserServiceAuthorizationFromIdentityClaim(HttpContext.User.Identity as ClaimsIdentity), UserServiceAuthorization.Guest))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			}
			else
			{
				GlobalConfiguration.UserManager.DeleteUser(userId);
			}
		}

		[AllowAnonymous]
		[Microsoft.AspNetCore.Mvc.HttpPost("/token")]
		public IActionResult GetToken([FromBody]IssueIdentityUser user, string password)
		{
			string storedHash = GlobalConfiguration.UserManager.GetUserPasswordHash(user.Id);

			PasswordHasher<IssueIdentityUser> passwordHasher = new PasswordHasher<IssueIdentityUser>();
			PasswordVerificationResult passwordVerification = passwordHasher.VerifyHashedPassword(user, storedHash, password);

			if (passwordVerification.Equals(PasswordVerificationResult.Failed))
			{
				return Unauthorized();
			}

			ClaimsIdentity claimsIdentity = BuildUserClaimsList(user);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = claimsIdentity,
				Expires = DateTime.UtcNow.AddMinutes(120),
				Issuer = _configuration["Jwt:Issuer"],
				Audience = _configuration["Jwt:Audience"],
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])), SecurityAlgorithms.HmacSha256)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var stringToken = tokenHandler.WriteToken(token);
			return Ok(stringToken);
		}

		private static ClaimsIdentity BuildUserClaimsList(IssueIdentityUser user)
		{
			List<Claim> claimList = new List<Claim>();
			Dictionary<string, string> userTenantRoles = GetUserTenantClaims(user);
			foreach (string claimName in userTenantRoles.Keys)
			{
				claimList.Add(new Claim(claimName, userTenantRoles[claimName]));
			}
			AddUserServiceRoleToClaimList(user, claimList);
			claimList.Add(new Claim("userId", user.Id));

			ClaimsIdentity claimsIdentity = new ClaimsIdentity(claimList.ToArray());
			return claimsIdentity;
		}

		private static void AddUserServiceRoleToClaimList(IssueIdentityUser user, List<Claim> claimList)
		{
			UserServiceAuthorization serviceAuthorization = GlobalConfiguration.TenantManager.GetUserServiceAuthorization(user.Id);
			claimList.Add(new Claim("ServiceRole", serviceAuthorization.ToString()));
		}

		private static Dictionary<string, string> GetUserTenantClaims(IssueIdentityUser user)
		{
			Dictionary<string, string> userTenantRoles = new Dictionary<string, string>();
			TenantProfile[] userTenants = GlobalConfiguration.TenantManager.GetTenants(user.Id);
			foreach (TenantProfile tenant in userTenants)
			{
				UserTenantAuthorization userTenantAuthorization = GlobalConfiguration.TenantManager.GetUserTenantAuthorization(tenant.TenantId, user.Id);
				string tenantKey = $"TenantRole_{userTenantAuthorization.ToString()}";
				if (!userTenantRoles.ContainsKey(tenantKey))
				{
					userTenantRoles.Add(tenantKey, tenant.TenantId);
				}
				else
				{
					userTenantRoles[tenantKey] = userTenantRoles[tenantKey] + $":{tenant.TenantId}";
				}
			}

			return userTenantRoles;
		}
	}
}
