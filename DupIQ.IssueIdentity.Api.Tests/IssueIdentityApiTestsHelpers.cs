using DupIQ.IssueIdentity.Api.Tests;
using DupIQ.IssueIdentity.Api.Tests;
using DupIQ.IssueIdentity.Api.Tests;
using NuGet.Frameworks;
using NuGet.Frameworks;
using NuGet.Frameworks;
using System.Net;
using System.Net;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json;
using System.Text.Json;
internal static class IssueIdentityApiTestsHelpers
{

	internal static string BuildDeleteAllRecordsRequestUri(string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/Admin/allrecords?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}";
	}

	internal static string BuildDeleteAllTenantsRequestUri(string uriBase)
	{
		return $"{uriBase}/Admin/alltenants";
	}

	internal static string BuildIssueProfileGetRequestUri(string uriBase2, string issueId, string _sharedTenantId3, string _sharedProjectId3)
	{
		return $"{uriBase2}/IssueProfiles/IssueProfile?issueId={issueId}&tenantId={_sharedTenantId3}&projectId={_sharedProjectId3}";
	}

	internal static string BuildIssueProfileRelatedGetRequestUri(string issueId, string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueReports/Related?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}&issueId={issueId}";
	}

	internal static string BuildIssueProfilesGetRequestUri(string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueProfiles?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}";
	}

	internal static string BuildIssueProfilesPostRequestUri(string uriBase1, string _sharedTenantId2, string _sharedProjectId2)
	{
		return $"{uriBase1}/IssueProfiles/IssueProfiles?tenantId={_sharedTenantId2}&projectId={_sharedProjectId2}";
	}

	internal static string BuildIssueProfilesRelatedPostRequestUri(string uriBase1, string _sharedTenantId2, string _sharedProjectId2)
	{
		return $"{uriBase1}/IssueProfiles/Related?tenantId={_sharedTenantId2}&projectId={_sharedProjectId2}";
	}

	internal static string BuildIssueReportGetRequestUri(string instanceId, string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueReports/IssueReport?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}&instanceId={instanceId}";
	}

	internal static string BuildIssueReportsGetRequestUri(string issueId, string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueReports/Related?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}&issueId={issueId}";
	}

	internal static string BuildIssueReportsPostRequestUri(string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueReports/Reports?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}";
	}

	internal static string BuildReportIssueGetRequestUri(string testMessage, string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueReports/Report?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}&message={testMessage}";
	}

	internal static string BuildReportIssuePostRequestUri(string uriBase, string _sharedTenantId1, string _sharedProjectId1)
	{
		return $"{uriBase}/IssueReports/Report?tenantId={_sharedTenantId1}&projectId={_sharedProjectId1}";
	}

		static internal HttpWebRequest CreateDeleteRequest(string requestUri, string token="")
		{
			var request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.Method = "DELETE";
			request.PreAuthenticate = true;
			request.ContentType = "application/json";
			request.Headers.Add("Access-Control-Allow-Origin", "*");
			request.Headers.Add("Authorization", "Bearer " + token);

		return request;
		}

		static internal HttpWebRequest CreateGetRequest(string requestUri, string token="")
		{
			var request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.Method = "GET"; 
			request.PreAuthenticate = true;
			request.ContentType = "application/json";
			request.Headers.Add("Access-Control-Allow-Origin", "*");
			request.Headers.Add("Authorization", "Bearer " + token);

		return request;
		}

		static internal HttpWebRequest CreatePostRequest(string postBody, string requestUri, string token="")
		{
			var request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.Method = "POST";
			request.PreAuthenticate = true;
			request.ContentType = "application/json";
			request.Headers.Add("Access-Control-Allow-Origin", "*");
			request.Headers.Add("Authorization", "Bearer " + token);
			Console.WriteLine("writing to request stream");
			using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(postBody);
			}

			return request;
		}
}