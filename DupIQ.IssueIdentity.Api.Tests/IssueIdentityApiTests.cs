using NuGet.Frameworks;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DupIQ.IssueIdentity.Api.Tests
{
	[TestClass]
	public class IssueIdentityApiTests
	{
		// this changes every time the database is paved or service admin reset
		// need to move it somewhere that is not compiled or shared
		private string _serviceadmin = string.Empty;
		private string UriBase = string.Empty;
		private string _sharedTenantId = string.Empty;
		private string _sharedTenantWriterId = string.Empty;
		private string _sharedTenantAdminId = string.Empty;
		private string _sharedTenantReaderId = string.Empty;
		private string _adminToken = string.Empty;
		private string _tenantAdminToken = string.Empty;
		private string _tenantWriterToken = string.Empty;
		private string _tenantReaderToken = string.Empty;

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
			GetAdminToken();
			CreateSharedUsers();
			GetSharedUsersTokens();
			CreateSharedTenant();
			CreateSharedProject();
		}

		[TestCleanup]
		public void Cleanup()
		{
			string deleteAllRecordsRequestUri = IssueIdentityApiTestsHelpers.BuildDeleteAllRecordsRequestUri(UriBase, _sharedTenantId, _sharedProjectId);

			HttpWebRequest webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteAllRecordsRequestUri, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
				Console.WriteLine($"Response to delete all records: {responseBody}");
			}
			string deleteAllTenantsRequestUri = IssueIdentityApiTestsHelpers.BuildDeleteAllTenantsRequestUri(UriBase);

			webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteAllTenantsRequestUri, _adminToken);
			webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
				Console.WriteLine($"Response to delete all tenants: {responseBody}");
			}

			string deleteUserRequestUriTemplate = $"{UriBase}/IssueIdentityUser?userId=";
			webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteUserRequestUriTemplate+_sharedTenantAdminId, _adminToken);
			webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
			}

			webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteUserRequestUriTemplate + _sharedTenantReaderId, _adminToken);
			webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
			}
			webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteUserRequestUriTemplate + _sharedTenantWriterId, _adminToken);
			webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
			}

		}

		[TestMethod]
		public void GET_ReportIssue()
		{
			string testMessage = "somethingnew";

			string requestUri = IssueIdentityApiTestsHelpers.BuildReportIssueGetRequestUri(testMessage, UriBase, _sharedTenantId, _sharedProjectId);

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(requestUri, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				IssueProfile issueProfileResponse = JsonSerializer.Deserialize<IssueProfile>(response);
				Assert.AreEqual(testMessage, issueProfileResponse.exampleMessage, "Fail if the example message of the returned issue is not what was sent.");
				Assert.AreEqual(true, issueProfileResponse.isNew, "Fail if the issue was not reported as new.");
			}
		}

		[TestMethod]
		public void GET_IssueReport()
		{
			string testMessage = "somethingnew";

			string issueReportGetRequestUri = IssueIdentityApiTestsHelpers.BuildReportIssueGetRequestUri(testMessage, UriBase, _sharedTenantId, _sharedProjectId);

			Console.WriteLine("First report the issue.");
			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(issueReportGetRequestUri, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			string issueId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				IssueProfile issueProfileResponse = JsonSerializer.Deserialize<IssueProfile>(response);
				issueId = issueProfileResponse.issueId;
				Assert.AreEqual(testMessage, issueProfileResponse.exampleMessage, "Fail if the example message of the returned issue is not what was sent.");
				Assert.AreEqual(true, issueProfileResponse.isNew, "Fail if the issue was not reported as new.");
			}

			string relatedIssueReportsGetRequestUri = IssueIdentityApiTestsHelpers.BuildIssueReportsGetRequestUri(issueId, UriBase, _sharedTenantId, _sharedProjectId);
			Console.WriteLine("Then get the related issue report id back from the issueId we got on the report.");
			webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(relatedIssueReportsGetRequestUri, _adminToken);
			webResponse = webRequest.GetResponse();
			string instanceId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var relatedIssueReportsResponse = JsonSerializer.Deserialize<IssueReport[]>(response);
				Assert.AreEqual(1, relatedIssueReportsResponse.Count(), "Fail if there is any other quantity than one IssueReport related to the issue profile.");
				instanceId = relatedIssueReportsResponse[0].instanceId;
			}

			Console.WriteLine("Use the instanceId to retrieve the IssueReport.");
			string issueReportsGetIssueReportRequestUri = IssueIdentityApiTestsHelpers.BuildIssueReportGetRequestUri(instanceId, UriBase, _sharedTenantId, _sharedProjectId);
			Console.WriteLine($"get issuereport uri: {issueReportsGetIssueReportRequestUri}");
			webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(issueReportsGetIssueReportRequestUri, _adminToken);
			webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var returnedIssueReport = JsonSerializer.Deserialize<IssueReport>(response);
				Assert.AreEqual(instanceId, returnedIssueReport.instanceId, "Fail if we did not get the IssueReport we were looking for.");
			}
		}

		[TestMethod]
		public void POST_IssueProfile()
		{
			Console.WriteLine("Get existing list of issue profiles.");
			string getIssueProfilesUri = $"{UriBase}/IssueProfiles?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			IssueProfile[] existingIssueProfiles;
			HttpWebRequest getIssueProfilesRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfilesUri, _adminToken);
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
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, postIssueProfileUri, _adminToken);

			webResponse = request.GetResponse();

			Console.WriteLine("Check list of issue profiles after creating new issue profile.");

			IssueProfile[] newIssueProfilesList;
			getIssueProfilesRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfilesUri, _adminToken);
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
			request = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfileUri, _adminToken);
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
		public void POST_IssueProfiles()
		{
			Console.WriteLine("Get existing list of issue profiles.");
			string getIssueProfilesUri = IssueIdentityApiTestsHelpers.BuildIssueProfilesGetRequestUri(UriBase, _sharedTenantId, _sharedProjectId);
			IssueProfile[] existingIssueProfiles;
			HttpWebRequest getIssueProfilesRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfilesUri, _adminToken);
			var webResponse = getIssueProfilesRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				existingIssueProfiles = JsonSerializer.Deserialize<IssueProfile[]>(response);
			}

			Console.WriteLine("Create new issue profiles from array.");
			string testIssueId = "testissueid";
			IssueProfile[] issueProfiles = new IssueProfile[]
			{
				new IssueProfile()
				{
					exampleMessage = "test message",
					isNew = true,
					firstReportedDate = DateTime.Now,
					issueId = testIssueId+"1",
					providerId = "test"
				},
				new IssueProfile()
				{
					exampleMessage = "test message",
					isNew = true,
					firstReportedDate = DateTime.Now,
					issueId = testIssueId+"2",
					providerId = "test"
				}
			};

			string postBody = JsonSerializer.Serialize(issueProfiles);

			string postIssueProfileUri = IssueIdentityApiTestsHelpers.BuildIssueProfilesPostRequestUri(UriBase, _sharedTenantId, _sharedProjectId);
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, postIssueProfileUri, _adminToken);

			webResponse = request.GetResponse();

			Console.WriteLine("Check list of issue profiles after creating new issue profile.");

			IssueProfile[] newIssueProfilesList;
			getIssueProfilesRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfilesUri, _adminToken);
			webResponse = getIssueProfilesRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine($"GET issue profiles response={response}");
				newIssueProfilesList = JsonSerializer.Deserialize<IssueProfile[]>(response);
			}

			Assert.IsTrue(newIssueProfilesList.Count() > existingIssueProfiles.Count(), "Fail if there was not a new issueProfile added to the existing list of IssueProfiles.");

			Console.WriteLine("Check if getting issue profile returns what we expected.");
			var getIssueProfileUri = IssueIdentityApiTestsHelpers.BuildIssueProfileGetRequestUri(UriBase, testIssueId + "1", _sharedTenantId, _sharedProjectId);
			Console.WriteLine($"uri={getIssueProfileUri}");
			request = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfileUri, _adminToken);
			webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				IssueProfile newProfile = JsonSerializer.Deserialize<IssueProfile>(response);
				Assert.AreEqual(testIssueId + "1", newProfile.issueId, "fail if we did not get back the expected issue id from call to GET IssueProfile.");
				Assert.AreEqual("SqlIssueDbProvider", newProfile.providerId, "fail if the system did not override our non-existent provider with the default.");
				Assert.IsFalse(newProfile.isNew, "fail if if isNew was not overridden and set to false.");
			}
		}

		[TestMethod]
		public void POST_ReportIssues()
		{
			IssueReport[] issueReports = new IssueReport[] {
				new IssueReport()
				{
					instanceId = "testinstance1",
					issueId = "changes",
					issueDate = DateTime.Now,
					issueMessage = "this is a test"
				},
				new IssueReport()
				{
					instanceId = "testinstance2",
					issueId = "changes",
					issueDate = DateTime.Now,
					issueMessage = "this is a test"
				}
			};
			string postBody = JsonSerializer.Serialize(issueReports);

			string uriBase = UriBase;
			string _sharedTenantId1 = _sharedTenantId;
			string _sharedProjectId1 = _sharedProjectId;
			string requestUri = IssueIdentityApiTestsHelpers.BuildIssueReportsPostRequestUri(uriBase, _sharedTenantId1, _sharedProjectId1);
			Console.WriteLine("Create array of issueReports.");
			Console.WriteLine($"POST to: {requestUri}");
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, requestUri, _adminToken);

			var webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueProfileResponse = JsonSerializer.Deserialize<IssueProfile[]>(response);

				Assert.AreEqual(2, issueProfileResponse.Count(), "Fail if we don't get two profiles returned.");
				Assert.AreEqual(issueProfileResponse[0].issueId, issueProfileResponse[1].issueId, "Fail if both issue profiles do not have the same issueId.");
				Assert.AreNotEqual(issueProfileResponse[0].isNew, issueProfileResponse[1].isNew, "Fail if both issue profiles have the same isNew value.");
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

			string reportIssuePostRequestUri = IssueIdentityApiTestsHelpers.BuildReportIssuePostRequestUri(UriBase, _sharedTenantId, _sharedProjectId);
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);

			var webResponse = request.GetResponse();
			string issueId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				issueId = JsonSerializer.Deserialize<IssueProfile>(response).issueId;
				Assert.AreNotEqual(string.Empty, issueId, "Fail if the issueId is still empty.");
			}

			Console.WriteLine("Then search for related issue profiles from that same issue report.");

			string requestUri = IssueIdentityApiTestsHelpers.BuildIssueProfilesRelatedPostRequestUri(UriBase, _sharedTenantId, _sharedProjectId);
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, requestUri, _adminToken);

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
		public void GET_IssueReports_Related_multiplereports()
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

			string reportIssuePostRequestUri = IssueIdentityApiTestsHelpers.BuildReportIssuePostRequestUri(UriBase, _sharedTenantId, _sharedProjectId);
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);

			var webResponse = request.GetResponse();
			string issueId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				issueId = JsonSerializer.Deserialize<IssueProfile>(response).issueId;
				Assert.AreNotEqual(string.Empty, issueId, "Fail if the issueId is still empty.");
			}

			Console.WriteLine("Post the IssueReport three more times.");
			issueReport.instanceId = "instance2";
			postBody = JsonSerializer.Serialize(issueReport);
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);
			request.GetResponse();
			issueReport.instanceId = "instance3";
			postBody = JsonSerializer.Serialize(issueReport);
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);
			request.GetResponse();
			issueReport.instanceId = "instance4";
			postBody = JsonSerializer.Serialize(issueReport);
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);
			request.GetResponse();

			Console.WriteLine("Then search for related issue profiles from that same issue report.");

			string relatedIssueReportsGetUri = IssueIdentityApiTestsHelpers.BuildIssueProfileRelatedGetRequestUri(issueId, UriBase, _sharedTenantId, _sharedProjectId);
			request = IssueIdentityApiTestsHelpers.CreateGetRequest(relatedIssueReportsGetUri, _adminToken);

			webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueReportsResponse = JsonSerializer.Deserialize<IssueReport[]>(response);

				Console.WriteLine(issueReportsResponse);

				Assert.AreEqual(4, issueReportsResponse.Count(), "We reported four times, so fail if that is not how many reports we got back.");
			}
		}

		[TestMethod]
		public void GET_IssueReports_Related_reportsameinstanceidoverandover()
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
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);

			var webResponse = request.GetResponse();
			string issueId = string.Empty;
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				issueId = JsonSerializer.Deserialize<IssueProfile>(response).issueId;
				Assert.AreNotEqual(string.Empty, issueId, "Fail if the issueId is still empty.");
			}

			Console.WriteLine("Post the IssueReport three more times.");
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);
			request.GetResponse();
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);
			request.GetResponse();
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, reportIssuePostRequestUri, _adminToken);
			request.GetResponse();

			Console.WriteLine("Then search for related issue profiles from that same issue report.");

			string relatedIssueReportsGetUri = $"{UriBase}/IssueReports/Related?tenantId={_sharedTenantId}&projectId={_sharedProjectId}&issueId={issueId}";
			request = IssueIdentityApiTestsHelpers.CreateGetRequest(relatedIssueReportsGetUri, _adminToken);

			webResponse = request.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				var issueReportsResponse = JsonSerializer.Deserialize<IssueReport[]>(response);

				Console.WriteLine(issueReportsResponse);

				Assert.AreEqual(1, issueReportsResponse.Count(), "We reported four times, but instance id was the same, so fail if there is more than 1 report.");
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
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, postIssueReportRequestUri, _adminToken);

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
			request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, postIssueReportRequestUri, _adminToken);

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
			request = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueReportsRelatedToIssueIdUri, _adminToken);
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
				HttpWebRequest req = IssueIdentityApiTestsHelpers.CreateGetRequest(reportIssuePostRequestUri, _adminToken);
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
			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(findRelatedIssuesUri, _adminToken);
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

		private void CreateSharedProject()
		{
			Project testProject = new Project()
			{
				tenantId = _sharedTenantId,
				projectId = _sharedProjectId,
				name = _sharedProjectName,
				ownerId = _sharedTenantAdminId,
				similarityThreshold = 0.95f
			};
			string addProjectUriBase = $"{UriBase}/Tenant/Project?tenantId={_sharedTenantId}";
			string serializedProjectJson = JsonSerializer.Serialize(testProject);

			Console.WriteLine($" addProjectUriBase:{addProjectUriBase}");
			Console.WriteLine($" serializedProjectJson:{serializedProjectJson}");

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(serializedProjectJson, addProjectUriBase, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				string projectBody = sr.ReadToEnd();
				Console.WriteLine($"project response:{projectBody}");
			}
		}

		private void CreateSharedUsers()
		{
			string AddUserURITemplate = $"{UriBase}/IssueIdentityUser";
			string SetUserPasswordTemplat = $"{UriBase}/IssueIdentityUser/password?password=password";
			Console.WriteLine($"Create user Uri:{AddUserURITemplate}");

			IssueIdentityUser user = new IssueIdentityUser
			{
				id = "temp",
				name = "testtenantwriter",
				firstname = "writer",
				lastname = "writer",
				email = "tenantwriter@email.com",
				userstatus = 0
			};

			string userJson = JsonSerializer.Serialize(user);
			_sharedTenantWriterId = CreateAndPasswordAndReturnId(AddUserURITemplate, SetUserPasswordTemplat, userJson);
			Console.WriteLine($"Shared TenantWriterId = {_sharedTenantWriterId}");

			user = new IssueIdentityUser
			{
				id = "temp",
				name = "testtenantadmin",
				firstname = "admin",
				lastname = "admin",
				email = "tenantadmin@email.com",
				userstatus = 0
			};
			userJson = JsonSerializer.Serialize(user);
			_sharedTenantAdminId = CreateAndPasswordAndReturnId(AddUserURITemplate, SetUserPasswordTemplat, userJson);
			Console.WriteLine($"Shared TenantAdminId = {_sharedTenantAdminId}");

			user = new IssueIdentityUser
			{
				id = "temp",
				name = "testtenantreader",
				firstname = "reader",
				lastname = "reader",
				email = "tenantreader@email.com",
				userstatus = 0
			};
			userJson = JsonSerializer.Serialize(user);
			_sharedTenantReaderId = CreateAndPasswordAndReturnId(AddUserURITemplate, SetUserPasswordTemplat, userJson);
			Console.WriteLine($"Shared TenantAdminId = {_sharedTenantReaderId}");

		}

		private string CreateAndPasswordAndReturnId(string AddUserURITemplate, string SetUserPasswordTemplat, string userJson)
		{
			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(userJson, AddUserURITemplate, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			string userId = string.Empty;
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				userId = sr.ReadToEnd().Replace("\"", string.Empty);
			}

			webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(string.Empty, SetUserPasswordTemplat + $"&userId={userId}", _adminToken);
			webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				Console.WriteLine($"Set password returned = {new StreamReader(stream).ReadToEnd()}");
			}

			return userId;
		}

		private void CreateSharedTenant()
		{
			string AddTenantURITemplate = $"{UriBase}/Tenant/Tenant?tenantName={_tenantName}&ownerId={_sharedTenantAdminId}&ownerEmail={_ownerEmail}&ownerName={_ownerName}";
			Console.WriteLine($"Create tenant Uri:{AddTenantURITemplate}");

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(string.Empty, AddTenantURITemplate, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				_sharedTenantId = sr.ReadToEnd().Replace("\"", string.Empty);
				Console.WriteLine($"Shared TenantId = {_sharedTenantId}");

			}
		}

		private void GetAdminToken()
		{
			string GetTokenUriTemplate = $"{UriBase}/token?password=password";
			Console.WriteLine($"Get token Uri:{GetTokenUriTemplate}");

			IssueIdentityUser adminUser = new IssueIdentityUser
			{
				id= _serviceadmin,
				name="string",
				firstname="string",
				lastname="string",
				email="string",
				userstatus=0
			};
			string userJson = JsonSerializer.Serialize(adminUser);

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(userJson, GetTokenUriTemplate, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				string tempToken = sr.ReadToEnd().Replace("\"", string.Empty);
				_adminToken = tempToken;
				Console.WriteLine($"Shared admin token = {_adminToken}");

			}
		}

		private void GetSharedUsersTokens()
		{

			_tenantAdminToken = GetClaimForUserId(_sharedTenantAdminId);
			Console.WriteLine($"Shared tenant admin token = {_tenantAdminToken}");
			_tenantWriterToken = GetClaimForUserId(_sharedTenantWriterId);
			Console.WriteLine($"Shared tenant writer token = {_tenantWriterToken}");
			_tenantReaderToken = GetClaimForUserId(_sharedTenantReaderId);
			Console.WriteLine($"Shared tenant reader token = {_tenantReaderToken}");
		}

		private string GetClaimForUserId(string userId)
		{
			string GetTokenUriTemplate = $"{UriBase}/token?password=password";
			Console.WriteLine($"Get token Uri:{GetTokenUriTemplate}");
			IssueIdentityUser user = new IssueIdentityUser
			{
				id = userId,
				name = "string",
				firstname = "string",
				lastname = "string",
				email = "string",
				userstatus = 0
			};
			string userJson = JsonSerializer.Serialize(user);

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(userJson, GetTokenUriTemplate, _adminToken);
			WebResponse webResponse = webRequest.GetResponse();
			string token = string.Empty;
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				string tempToken = sr.ReadToEnd().Replace("\"", string.Empty);
				token = tempToken;

			}

			return token;
		}

		private void InitializeFromConfig()
		{
			string testConfigText = System.IO.File.ReadAllText("testconfig.json");
			var testConfigJson = JsonSerializer.Deserialize<JsonElement>(testConfigText);

			UriBase = testConfigJson.GetProperty("testserverbaseuri").GetString(); 
			_serviceadmin = testConfigJson.GetProperty("serviceadminid").GetString();
			Console.WriteLine($"UriBase:{UriBase}, ServiceAdmin: {_serviceadmin}");
		}
	}
}