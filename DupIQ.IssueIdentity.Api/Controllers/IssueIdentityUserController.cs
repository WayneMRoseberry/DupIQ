using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace DupIQ.IssueIdentityAPI.Controllers
{
	[Microsoft.AspNetCore.Mvc.Route("[controller]")]
	[ApiController]
	public class IssueIdentityUserController : IssueIdentityControllerBaseClass
	{
		ILogger<IssueIdentityUserController> logger;
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
			return GlobalConfiguration.UserManager.GetUserById(userId);
		}

		[Microsoft.AspNetCore.Mvc.HttpPost]
		public string AddUser(IssueIdentityUser user)
		{
			return GlobalConfiguration.UserManager.AddOrUpdateUser(user);

		}

		[Microsoft.AspNetCore.Mvc.HttpDelete]
		public void DeleteUser(string userId)
		{
			GlobalConfiguration.UserManager.DeleteUser(userId);
		}

		[Microsoft.AspNetCore.Mvc.HttpPost("/token")]
		public string GetToken(IssueIdentityUser user)
		{
			var issuer = _configuration["Jwt:Issuer"];
			var audience = _configuration["Jwt:Audience"];
			var expiry = DateTime.Now.AddMinutes(120);
			var securityKey = new SymmetricSecurityKey
		(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var credentials = new SigningCredentials
		(securityKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(issuer: issuer,
		audience: audience,
		expires: expiry,
		signingCredentials: credentials);

			var tokenHandler = new JwtSecurityTokenHandler();
			var stringToken = tokenHandler.WriteToken(token);
			return stringToken;
		}
	}
}
