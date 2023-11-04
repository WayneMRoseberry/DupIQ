using NuGet.Frameworks;
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
			CreateSharedTenant();
			CreateSharedProject();
		}

		[TestCleanup]
		public void Cleanup()
		{
			string deleteRequestUri = $"{UriBase}/Admin/allrecords?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";

			HttpWebRequest webRequest = CreateDeleteRequest(deleteRequestUri);
			WebResponse webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
				Console.WriteLine($"Response to delete all records: {responseBody}");
			}
		}

		[TestMethod]
		public void GET_ReportIssue()
		{
			string testMessage = "somethingnew";

			string requestUri = $"{UriBase}/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}&message={testMessage}";

			WebRequest webRequest = CreateGetRequest(requestUri);
			WebResponse webResponse = webRequest.GetResponse();
			using(var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				IssueProfile issueProfileResponse = JsonSerializer.Deserialize<IssueProfile>(response);
				Assert.AreEqual(testMessage, issueProfileResponse.exampleMessage, "Fail if the example message of the returned issue is not what was sent.");
				Assert.AreEqual(true, issueProfileResponse.isNew, "Fail if the issue was not reported as new.");
			}
		}

		[TestMethod]
		public void POST_IssueProfile()
		{
			Console.WriteLine("Get existing list of issue profiles.");
			string getIssueProfilesUri = $"{UriBase}/IssueProfiles?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			IssueProfile[] existingIssueProfiles;
			HttpWebRequest getIssueProfilesRequest = CreateGetRequest(getIssueProfilesUri);
			var webResponse = getIssueProfilesRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				existingIssueProfiles = JsonSerializer.Deserialize<IssueProfile[]>(response);
			}

			Console.WriteLine("Create a new issue profile.");
			string testIssueId = "testissueid";
			IssueProfile issueProfile = new IssueProfile()
			{
				exampleMessage = "test message",
				isNew = true,
				firstReportedDate = DateTime.Now,
				issueId = testIssueId,
				providerId = "test"
			};

			string postBody = JsonSerializer.Serialize(issueProfile);

			string postIssueProfileUri = $"{UriBase}/IssueProfiles/IssueProfile?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			HttpWebRequest request = CreatePostRequest(postBody, postIssueProfileUri);

			webResponse = request.GetResponse();

			Console.WriteLine("Check list of issue profiles after creating new issue profile.");

			IssueProfile[] newIssueProfilesList;
			getIssueProfilesRequest = CreateGetRequest(getIssueProfilesUri);
			webResponse = getIssueProfilesRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				newIssueProfilesList = JsonSerializer.Deserialize<IssueProfile[]>(response);
			}

			Assert.IsTrue(newIssueProfilesList.Count() > existingIssueProfiles.Count(), "Fail if there was not a new issueProfile added to the existing list of IssueProfiles.");

			Console.WriteLine("Check if getting issue profile returns what we expected.");
			var getIssueProfileUri = $"{UriBase}/IssueProfiles/IssueProfile?issueId={testIssueId}&tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			Console.WriteLine($"uri={getIssueProfileUri}");
			request = CreateGetRequest(getIssueProfileUri);
			webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				IssueProfile newProfile = JsonSerializer.Deserialize<IssueProfile>(response);
				Assert.AreEqual(testIssueId, newProfile.issueId, "fail if we did not get back the expected issue id from call to GET IssueProfile.");
				Assert.AreEqual("SqlIssueDbProvider", newProfile.providerId, "fail if the system did not override our non-existent provider with the default.");
				Assert.IsFalse(newProfile.isNew, "fail if if isNew was not overridden and set to false.");
			}
		}

		[TestMethod]
		public void POST_ReportIssue()
		{
			IssueReport issueReport = new IssueReport()
			{
				instanceId = "testinstance",
				issueId = "changes",
				issueDate = DateTime.Now,
				issueMessage = "this is a test"
			};
			string postBody = JsonSerializer.Serialize(issueReport);

			string requestUri = $"{UriBase}/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			HttpWebRequest request = CreatePostRequest(postBody, requestUri);

			var webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueProfileResponse = JsonSerializer.Deserialize<IssueProfile>(response);

				Assert.AreEqual("this is a test", issueProfileResponse.exampleMessage, "Fail if the example message does not match issue entered.");
				Assert.IsTrue(issueProfileResponse.isNew, "Fail if the issue is not reported as new.");
			}
		}

		[TestMethod]
		public void POST_IssueProfiles_Related()
		{
			IssueReport issueReport = new IssueReport()
			{
				instanceId = "testinstance",
				issueId = "changes",
				issueDate = DateTime.Now,
				issueMessage = "this is a test"
			};
			string postBody = JsonSerializer.Serialize(issueReport);

			Console.WriteLine("First post an issue report.");
			string reportIssuePostRequestUri = $"{UriBase}/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			HttpWebRequest request = CreatePostRequest(postBody, reportIssuePostRequestUri);

			var webResponse = request.GetResponse();
			string issueId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				issueId = JsonSerializer.Deserialize<IssueProfile>(response).issueId;
				Assert.AreNotEqual(string.Empty, issueId, "Fail if the issueId is still empty.");
			}

			Console.WriteLine("Then search for related issue profiles from that same issue report.");

			string requestUri = $"{UriBase}/IssueProfiles/Related?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			request = CreatePostRequest(postBody, requestUri);

			webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueProfileResponse = JsonSerializer.Deserialize<RelatedIssueProfile[]>(response);

				Console.WriteLine(issueProfileResponse);

				Assert.AreEqual(1, issueProfileResponse.Count(), "something to fail on.");
				Assert.AreEqual(issueId, issueProfileResponse[0].issueId, "fail if the first issueId is not the same one we got back reporting the issue.");
			}
		}

		[TestMethod]
		public void ReportMultipleIssuesAndSeeIfVisibleViaOtherApis()
		{
			IssueReport issueReport = new IssueReport()
			{
				instanceId = "testinstance",
				issueId = "changes",
				issueDate = DateTime.Now,
				issueMessage = "this is a test"
			};
			string postBody = JsonSerializer.Serialize(issueReport);

			string postIssueReportRequestUri = $"{UriBase}/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			HttpWebRequest request = CreatePostRequest(postBody, postIssueReportRequestUri);

			var webResponse = request.GetResponse();
			string issueId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				Console.WriteLine("read the response");
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueProfileResponse = JsonSerializer.Deserialize<IssueProfile>(response);
				issueId = issueProfileResponse.issueId;
				Assert.AreEqual("this is a test", issueProfileResponse.exampleMessage, "Fail if the example message does not match issue entered.");
				Assert.IsTrue(issueProfileResponse.isNew, "Fail if the issue is not reported as new.");
			}

			issueReport.instanceId = "testinstance2";
			postBody = JsonSerializer.Serialize(issueReport);
			request = CreatePostRequest(postBody, postIssueReportRequestUri);

			webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueProfileResponse = JsonSerializer.Deserialize<IssueProfile>(response);

				Assert.AreEqual("this is a test", issueProfileResponse.exampleMessage, "Fail if the example message does not match issue entered.");
				Assert.IsFalse(issueProfileResponse.isNew, "Fail if the issue is reported as new after posting report second time.");
			}

			string getIssueReportsRelatedToIssueIdUri = UriBase + $"/IssueReports/Related?tenantId={_sharedTenantId}&projectId={_sharedProjectId}&issueId={issueId}";
			request = CreateGetRequest(getIssueReportsRelatedToIssueIdUri);
			webResponse = request.GetResponse();
			using(var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				Console.WriteLine("read the response");
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueReportsResponse = JsonSerializer.Deserialize<IssueReport[]>(response);
				Assert.AreEqual(2, issueReportsResponse.Count(), "Fail if there are not two related issue reports returned for the issue profile.");
			}
		}

		[TestMethod]
		public void PostBatchOfIssueReports()
		{
			string similarHeadlineFile = AppContext.BaseDirectory + "\\similarheadlines.txt";
			string[] contentHeadlines = System.IO.File.ReadAllLines(similarHeadlineFile);

			Dictionary<string, int> issueProfileCounts = new Dictionary<string, int>();
			int lineCounter = 0;
			foreach(string line in contentHeadlines)
			{
				System.Console.WriteLine($" - posting line {lineCounter}.");
				lineCounter++;
				string reportIssuePostRequestUri = $"{UriBase}/IssueReports/Report?tenantId={_sharedTenantId}&projectId={_sharedProjectId}&message={Uri.EscapeDataString(line)}";
				HttpWebRequest req = CreateGetRequest(reportIssuePostRequestUri);
				var resp = req.GetResponse();
				using(var responseReader = new StreamReader(resp.GetResponseStream()))
				{
					string respContent = responseReader.ReadToEnd();
					IssueProfile issueProfile = JsonSerializer.Deserialize<IssueProfile>(respContent);
					if(!issueProfileCounts.ContainsKey(issueProfile.issueId))
					{
						issueProfileCounts.Add(issueProfile.issueId, 0);
					}
					issueProfileCounts[issueProfile.issueId]++;
				}
			}
			System.Console.WriteLine($"headlines={contentHeadlines.Count()}, issueprofiles={issueProfileCounts.Count}");

			string findRelatedIssuesUri = $"{UriBase}/IssueProfiles/Related?tenantId={_sharedTenantId}&projectId={_sharedProjectId}&message={Uri.EscapeUriString(contentHeadlines[0])}";
			WebRequest webRequest = CreateGetRequest(findRelatedIssuesUri);
			var response = webRequest.GetResponse();
			int relatedProfilesCount = 0;
			using(var responseReader = new StreamReader(response.GetResponseStream()))
			{
				string respContent = responseReader.ReadToEnd();
				RelatedIssueProfile[] relatedIssueProfiles = JsonSerializer.Deserialize<RelatedIssueProfile[]>(respContent);
				relatedProfilesCount = relatedIssueProfiles.Count();
				System.Console.WriteLine($"relatedIssueProfiles.Count={relatedProfilesCount}");
			}

			double ratio = ((double)issueProfileCounts.Count) / ((double)contentHeadlines.Count());
			Assert.IsTrue(ratio > 0.1, "Fail if the content provided did not yield more than a 0.1 ratio of issues to issue reports.");
			Assert.IsTrue(relatedProfilesCount > 1, "Fail if there is not more than 1 related issue profile returned.");
		}

		private HttpWebRequest CreateDeleteRequest(string requestUri)
		{
			var request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.Method = "DELETE";
			request.ContentType = "application/json";
			request.Headers.Add("Access-Control-Allow-Origin", "*");

			return request;
		}

		private HttpWebRequest CreateGetRequest(string requestUri)
		{
			var request = (HttpWebRequest)WebRequest.Create(requestUri);
			request.Method = "GET";
			request.ContentType = "application/json";
			request.Headers.Add("Access-Control-Allow-Origin", "*");

			return request;
		}

		private HttpWebRequest CreatePostRequest(string postBody, string requestUri)
		{
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

		private void CreateSharedProject()
		{
			Project testProject = new Project()
			{
				tenantId = _sharedTenantId,
				projectId = _sharedProjectId,
				name = _sharedProjectName,
				ownerId = _ownerId,
				similarityThreshold = 0.95f
			};
			string addProjectUriBase = $"{UriBase}/Tenant/Project?tenantId={_sharedTenantId}";
			string serializedProjectJson = JsonSerializer.Serialize(testProject);

			Console.WriteLine($" addProjectUriBase:{addProjectUriBase}");
			Console.WriteLine($" serializedProjectJson:{serializedProjectJson}");

			WebRequest webRequest = CreatePostRequest(serializedProjectJson, addProjectUriBase);
			WebResponse webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				string projectBody = sr.ReadToEnd();
				Console.WriteLine($"project response:{projectBody}");
			}
		}

		private void CreateSharedTenant()
		{
			string AddTenantURITemplate = $"{UriBase}/Tenant/Tenant?tenantName={_tenantName}&ownerId={_ownerId}&ownerEmail={_ownerEmail}&ownerName={_ownerName}";
			Console.WriteLine($"Create tenant Uri:{AddTenantURITemplate}");

			WebRequest webRequest = CreatePostRequest(string.Empty, AddTenantURITemplate);
			WebResponse webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				_sharedTenantId = sr.ReadToEnd().Replace("\"", string.Empty);
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