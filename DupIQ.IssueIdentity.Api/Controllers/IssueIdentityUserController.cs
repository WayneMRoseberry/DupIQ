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
		public IActionResult GetToken([FromBody]IssueIdentityUser user)
		{
			var tokenDescriptor = new SecurityTokenDescriptor
			{
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
	}
}
