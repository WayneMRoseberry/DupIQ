using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

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
			var returnedIssueId = jsonResponse.GetProperty("issueId").GetString();
			var returnedIsNew = jsonResponse.GetProperty("isNew").GetBoolean();
			var returnedExampleMessage = jsonResponse.GetProperty("exampleMessage").GetString();
			Console.WriteLine($"{returnedIssueId}:{returnedIsNew}:{returnedExampleMessage}");
			Assert.AreEqual(testMessage, returnedExampleMessage, "Fail if the example message of the returned issue is not what was sent.");
			Assert.AreEqual(true, returnedIsNew, "Fail if the issue was not reported as new.");
		}

		[TestMethod]
		public void Post_ReportIssue()
		{
			const string postBody = @"{""instanceId"":""testinstance"",""issueId"":""replaced"",""isNew"":""true"",""issueDate"":""2023-10-28T17:42:40.837Z"",""issueMessage"":""this is a test""}";

			string requestUri = UriBase + $"/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			string testMessage = "somethingnew";

			HttpWebRequest request = CreatePostRequest(postBody, requestUri);

			var webResponse = request.GetResponse();
			Console.WriteLine("get response stream");
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				Console.WriteLine("read the response");
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var responseJson = JsonSerializer.Deserialize<JsonElement>(response);

				string returnedExampleMessage = responseJson.GetProperty("exampleMessage").GetString();
				bool returnedIsNew = responseJson.GetProperty("isNew").GetBoolean();
				Assert.AreEqual("this is a test", returnedExampleMessage, "Fail if did not get back expected message.");
				Assert.IsTrue(returnedIsNew, "Fail if the reported issue was not new.");
			}
		}

		private HttpClient CreateHttpClient()
		{
			HttpClient httpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
			httpClient.BaseAddress = new Uri(UriBase);
			System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("text/plain"));
			httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
			return httpClient;
		}

		private HttpWebRequest CreatePostRequest(string postBody, string requestUri)
		{
			HttpClient httpClient = CreateHttpClient();
			var request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.Headers.Add("Access-Control-Allow-Origin", "*");
			Console.WriteLine("writing to request stream");
			using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(postBody);
			}

			return request;
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