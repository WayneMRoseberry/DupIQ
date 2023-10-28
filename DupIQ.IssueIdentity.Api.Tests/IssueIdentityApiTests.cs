using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Configuration;

namespace DupIQ.IssueIdentity.Api.Tests
{
	[TestClass]
	public class IssueIdentityApiTests
	{
		private string UriBase = "http://localhost:5000";
		private string _sharedTenantId = string.Empty;

		string _tenantName = "test_tenant";
		string _ownerId = "user1";
		string _ownerEmail = "owneremail";
		string _ownerName = "user_one";
		string _sharedProjectId = "testproject";
		string _sharedProjectName = "test_project";

		[TestInitialize]
		public void InitializeDupIq()
		{
			InitializeFromConfig();
			HttpClient httpClient = CreateHttpClient();
			CreateSharedTenant(httpClient);
			CreateSharedProject(httpClient);
		}

		[TestCleanup]
		public void Cleanup()
		{
			HttpClient client = CreateHttpClient();
			HttpResponseMessage httpResponseMessage = client.DeleteAsync(UriBase + $"/Admin/allrecords?tenantId={_sharedTenantId}&projectId={_sharedProjectId}").Result;
			Console.WriteLine($"Response to delete all records: {httpResponseMessage}");
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode, "Failed to delete content.");
		}

		[TestMethod]
		public void GET_ReportIssue()
		{
			HttpClient httpClient = CreateHttpClient();
			string testMessage = "somethingnew";

			string requestUri = UriBase + $"/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}&message={testMessage}";
			Console.WriteLine($"Report Issue Uri: {requestUri}");
			var responseMessage = httpClient.GetAsync(requestUri).Result;
			Console.WriteLine($"responseMessage: {responseMessage}");
			Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode, "Fail if the request was not successful.");

			string body = responseMessage.Content.ReadAsStringAsync().Result;
			Console.WriteLine($"responseMessage.Content: {body}");

			var jsonResponse = JsonSerializer.Deserialize<JsonElement>(body);
			var returnedIsNew = jsonResponse.GetProperty("isNew").GetBoolean();
			var returnedExampleMessage = jsonResponse.GetProperty("exampleMessage").GetString();
			Console.WriteLine($"{returnedIssueId}:{returnedIsNew}:{returnedExampleMessage}");
			Assert.AreEqual(testMessage, returnedExampleMessage, "Fail if the example message of the returned issue is not what was sent.");
			Assert.AreEqual(true, returnedIsNew, "Fail if the issue was not reported as new.");
		}

		private HttpClient CreateHttpClient()
		{
			HttpClient httpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
			httpClient.BaseAddress = new Uri(UriBase);
			System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/json"));
			return httpClient;
		}

		private void CreateSharedProject(HttpClient httpClient)
		{
			using (StringContent jsonContent = new StringContent($"{{n\"projectId\": \"{_sharedProjectId}\",\"tenantId\": \"{_sharedTenantId}\",\"name\":\"{_sharedProjectName}\",\"ownerId\":\"{_ownerId}\",\"similarityThreshold\":0}}"))
			{
				var responseMessage = httpClient.PostAsync(UriBase + $"/Tenant/Project?tenantId={_sharedTenantId}", jsonContent).Result;
			}
		}

		private void CreateSharedTenant(HttpClient httpClient)
		{
			string AddTenantURITemplate = $"{UriBase}/Tenant/Tenant?tenantName={_tenantName}&ownerId={_ownerId}&ownerEmail={_ownerEmail}&ownerName={_ownerName}";
			Console.WriteLine($"Create tenant Uri:{AddTenantURITemplate}");

			using (StringContent jsonContent = new StringContent(string.Empty))
			{
				var responseMessage = httpClient.PostAsync(AddTenantURITemplate, jsonContent).Result;

				_sharedTenantId = responseMessage.Content.ReadAsStringAsync().Result.Replace("\"", string.Empty);
				Console.WriteLine($"Shared TenantId = {_sharedTenantId}");
			}
		}

		private void InitializeFromConfig()
		{
			string testConfigText = System.IO.File.ReadAllText("testconfig.json");
			var testConfigJson = JsonSerializer.Deserialize<JsonElement>(testConfigText);

			UriBase = testConfigJson.GetProperty("testserverbaseuri").GetString();
			Console.WriteLine($"UriBase:{UriBase}");
		}
	}
}