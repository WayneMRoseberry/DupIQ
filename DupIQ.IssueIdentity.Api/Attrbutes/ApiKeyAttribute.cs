using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DupIQ.IssueIdentityAPI.Attrbutes
{
	[AttributeUsage(validOn: AttributeTargets.Class)]
	public class ApiKeyAttribute : Attribute, IAsyncActionFilter
	{
		private const string APIKEYNAME = "ApiKey";

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
			{
				context.Result = new ContentResult()
				{
					StatusCode = 401,
					Content = "Api Key was not provided"
				};
				return;
			}
			await next();
		}
	}
}
