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
		private string _adminToken = string.Empty;
		private string _sharedTenantId = string.Empty;
		private string _sharedTenantWriterId = string.Empty;
		private string _sharedTenantAdminId = string.Empty;
		private string _sharedTenantReaderId = string.Empty;
		private string _tenantAdminToken = string.Empty;
		private string _tenantWriterToken = string.Empty;
		private string _tenantReaderToken = string.Empty;
		private string _sharedTenantId2 = string.Empty;
		private string _sharedTenantWriterId2 = string.Empty;
		private string _sharedTenantAdminId2 = string.Empty;
		private string _sharedTenantReaderId2 = string.Empty;
		private string _tenantAdminToken2 = string.Empty;
		private string _tenantWriterToken2 = string.Empty;
		private string _tenantReaderToken2 = string.Empty;

		string _tenantName = "test_tenant1"; 
		string _tenantName2 = "test_tenant2";
		string _ownerId = "user1";
		string _ownerEmail = "owneremail";
		string _ownerName = "user_one";
		string _sharedProjectId = "testproject";
		string _sharedProjectName = "test_project";
		string _sharedProjectId2 = "testproject2";
		string _sharedProjectName2 = "test_project2";

		[TestInitialize]
		public void InitializeDupIq()
		{
			InitializeFromConfig();
			GetAdminToken();
			CreateSharedUsers();
			CreateSharedTenant();
			CreateSharedProject();
			GetSharedUsersTokens();
		}

		[TestCleanup]
		public void Cleanup()
		{
			string deleteAllRecordsRequestUri = IssueIdentityApiTestsHelpers.BuildDeleteAllRecordsRequestUri(UriBase, _sharedTenantId, _sharedProjectId);

			string userToken = _adminToken;
			HttpWebRequest webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteAllRecordsRequestUri, userToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
				Console.WriteLine($"Response to delete all records: {responseBody}");
			}
			string deleteAllTenantsRequestUri = IssueIdentityApiTestsHelpers.BuildDeleteAllTenantsRequestUri(UriBase);

			webRequest = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteAllTenantsRequestUri, userToken);
			webResponse = webRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
				Console.WriteLine($"Response to delete all tenants: {responseBody}");
			}
			DeleteUser(userToken, _sharedTenantAdminId);
			DeleteUser(userToken, _sharedTenantReaderId);
			DeleteUser(userToken, _sharedTenantWriterId);
			DeleteUser(userToken, _sharedTenantAdminId2);
			DeleteUser(userToken, _sharedTenantReaderId2);
			DeleteUser(userToken, _sharedTenantWriterId2);
		}

		private void DeleteUser(string userToken, string userId)
		{
			string deleteUserRequestUriTemplate1 = $"{UriBase}/IssueIdentityUser?userId=";

			HttpWebRequest webRequest2 = IssueIdentityApiTestsHelpers.CreateDeleteRequest(deleteUserRequestUriTemplate1 + userId, userToken);
			WebResponse webResponse2 = webRequest2.GetResponse();
			using (var responseReader = new StreamReader(webResponse2.GetResponseStream()))
			{
				string responseBody = responseReader.ReadToEnd();
			}
		}

		[TestMethod]
		public void GET_ReportIssue()
		{
			string userToken = _adminToken;
			GetReportIssueForUserToken(userToken);
		}

		[TestMethod]
		public void GET_ReportIssue_allroles()
		{
			Console.WriteLine("Check with tenant admin token.");
			GetReportIssueForUserToken(_tenantAdminToken);
			Console.WriteLine("Check with tenant writer token.");
			GetReportIssueForUserToken(_tenantWriterToken);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void GET_ReportIssue_reader_shouldthrow()
		{
			Console.WriteLine("Check with tenant reader token.");
			GetReportIssueForUserToken(_tenantReaderToken);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void GET_ReportIssue_tenantadmin2_tenant1_shouldthrow()
		{
			Console.WriteLine("Check with tenantadmin2 token.");
			GetReportIssueForUserToken(_tenantAdminToken2);
		}

		[TestMethod]
		public void GET_ReportIssue_tenantadmin2_tenant2()
		{
			Console.WriteLine("Check with tenantadmin2 token.");
			GetReportIssueForUserTokenAndTenant(_tenantAdminToken2, "tenantadmin 2 reporting new issue", _sharedTenantId2, _sharedProjectId2);
		}

		private void GetReportIssueForUserToken(string userToken)
		{
			string testMessage = "somethingnew";

			GetReportIssueForUserTokenAndTenant(userToken, testMessage, _sharedTenantId, _sharedProjectId);
		}

		private void GetReportIssueForUserTokenAndTenant(string userToken, string testMessage, string tenantId, string projectId)
		{
			string requestUri = IssueIdentityApiTestsHelpers.BuildReportIssueGetRequestUri(testMessage, UriBase, tenantId, projectId);

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(requestUri, userToken);
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
		public void GET_IssueReport_eachwritecapablerole()
		{
			Console.WriteLine("Check with tenant admin token.");
			CheckIssueReportWithUserToken(_tenantAdminToken); 
			Console.WriteLine("Check with tenant writer token.");
			CheckIssueReportWithUserToken(_tenantWriterToken);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void GET_IssueReport_asreader_shouldthrow()
		{
			Console.WriteLine("Check with tenant reader token.");
			CheckIssueReportWithUserToken(_tenantReaderToken);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void GET_IssueReport_tenantadmin2_againsttenant1_shouldthrow()
		{
			Console.WriteLine("Check with tenant2 admin token.");
			CheckIssueReportWithUserToken(_tenantAdminToken2);
		}

		[TestMethod]
		public void GET_IssueReport_tenantadmin2_againsttenant2()
		{
			Console.WriteLine("Check with tenant2 admin token.");
			CheckIssueReportWithUserTokenAndTenant(_tenantAdminToken2,"different tenant message", _sharedTenantId2, _sharedProjectId2);
		}

		private void CheckIssueReportWithUserToken(string userToken)
		{
			string testMessage = "somethingnew";

			CheckIssueReportWithUserTokenAndTenant(userToken, testMessage, _sharedTenantId, _sharedProjectId);
		}

		private void CheckIssueReportWithUserTokenAndTenant(string userToken, string testMessage, string tenantId, string projectId)
		{
			string issueReportGetRequestUri = IssueIdentityApiTestsHelpers.BuildReportIssueGetRequestUri(testMessage, UriBase, tenantId, projectId);

			Console.WriteLine("First report the issue.");
			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(issueReportGetRequestUri, userToken);
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

			string relatedIssueReportsGetRequestUri = IssueIdentityApiTestsHelpers.BuildIssueReportsGetRequestUri(issueId, UriBase, tenantId, projectId);
			Console.WriteLine("Then get the related issue report id back from the issueId we got on the report.");
			webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(relatedIssueReportsGetRequestUri, userToken);
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
			string issueReportsGetIssueReportRequestUri = IssueIdentityApiTestsHelpers.BuildIssueReportGetRequestUri(instanceId, UriBase, tenantId, projectId);
			Console.WriteLine($"get issuereport uri: {issueReportsGetIssueReportRequestUri}");
			webRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(issueReportsGetIssueReportRequestUri, userToken);
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
			CheckPostIssueProfileWithUserToken(_adminToken);
		}

		[TestMethod]
		public void POST_IssueProfile_allwriteableroles()
		{
			Console.WriteLine("Check with tenant admin token.");
			CheckPostIssueProfileWithUserToken(_tenantAdminToken);
			Console.WriteLine("Check with tenant writer token.");
			CheckPostIssueProfileWithUserToken(_tenantWriterToken);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void POST_IssueProfile_reader_shouldtrown()
		{

			Console.WriteLine("Check with tenant reader token.");
			CheckPostIssueProfileWithUserToken(_tenantReaderToken);
		}

		private void CheckPostIssueProfileWithUserToken(string userToken)
		{
			Console.WriteLine("Get existing list of issue profiles.");
			string getIssueProfilesUri = $"{UriBase}/IssueProfiles?tenantId={_sharedTenantId}&projectId={_sharedProjectId}";
			IssueProfile[] existingIssueProfiles;
			HttpWebRequest getIssueProfilesRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfilesUri, userToken);
			var webResponse = getIssueProfilesRequest.GetResponse();
			using (var responseReader = new StreamReader(webResponse.GetResponseStream()))
			{
				string response = responseReader.ReadToEnd();
				Console.WriteLine(response);
				existingIssueProfiles = JsonSerializer.Deserialize<IssueProfile[]>(response);
			}

			Console.WriteLine("Create a new issue profile.");
			string testIssueId = "testissueid_" + System.Guid.NewGuid().ToString();
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
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, postIssueProfileUri, userToken);

			webResponse = request.GetResponse();

			Console.WriteLine("Check list of issue profiles after creating new issue profile.");

			IssueProfile[] newIssueProfilesList;
			getIssueProfilesRequest = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfilesUri, userToken);
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
			request = IssueIdentityApiTestsHelpers.CreateGetRequest(getIssueProfileUri, userToken);
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
			CheckPostReportIssuesForUserToken(_adminToken);
		}

		[TestMethod]
		public void POST_ReportIssues_allroles()
		{
			Console.WriteLine("Check with tenant admin token.");
			CheckPostReportIssuesForUserToken(_tenantAdminToken, "a new string for admin tenants to post");
			Console.WriteLine("Check with tenant writer token.");
			CheckPostReportIssuesForUserToken(_tenantWriterToken, "writers on a given tenant might post this string");
		}

		[TestMethod]
		[ExpectedException(typeof(System.Net.WebException))]
		public void POST_ReportIssues_reader()
		{
			Console.WriteLine("Check with tenant reader token.");
			CheckPostReportIssuesForUserToken(_tenantReaderToken, "really a reader should not be able to post anything");
		}

		private void CheckPostReportIssuesForUserToken(string userToken)
		{
			const string issueString = "this is a test";
			CheckPostReportIssuesForUserToken(userToken, issueString);
		}

		private void CheckPostReportIssuesForUserToken(string userToken, string issueString)
		{
			IssueReport[] issueReports = new IssueReport[] {
				new IssueReport()
				{
					instanceId = "testinstance1",
					issueId = "changes",
					issueDate = DateTime.Now,
					issueMessage = issueString
				},
				new IssueReport()
				{
					instanceId = "testinstance2",
					issueId = "changes",
					issueDate = DateTime.Now,
					issueMessage = issueString
				}
			};
			string postBody = JsonSerializer.Serialize(issueReports);

			string uriBase = UriBase;
			string _sharedTenantId1 = _sharedTenantId;
			string _sharedProjectId1 = _sharedProjectId;
			string requestUri = IssueIdentityApiTestsHelpers.BuildIssueReportsPostRequestUri(uriBase, _sharedTenantId1, _sharedProjectId1);
			Console.WriteLine("Create array of issueReports.");
			Console.WriteLine($"POST to: {requestUri}");
			HttpWebRequest request = IssueIdentityApiTestsHelpers.CreatePostRequest(postBody, requestUri, userToken);

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
			CreateProject(_adminToken, _sharedTenantId, _sharedProjectId, _sharedProjectName, _sharedTenantAdminId);
			CreateProject(_adminToken, _sharedTenantId2, _sharedProjectId2, _sharedProjectName2, _sharedTenantAdminId2);
		}

		private void CreateProject(string userToken, string tenantId, string projectId, string projectName, string projectOwnerId)
		{
			Project testProject = new Project()
			{
				tenantId = tenantId,
				projectId = projectId,
				name = projectName,
				ownerId = projectOwnerId,
				similarityThreshold = 0.95f
			};
			string addProjectUriBase = $"{UriBase}/Tenant/Project?tenantId={tenantId}";
			string serializedProjectJson = JsonSerializer.Serialize(testProject);

			Console.WriteLine($" addProjectUriBase:{addProjectUriBase}");
			Console.WriteLine($" serializedProjectJson:{serializedProjectJson}");

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(serializedProjectJson, addProjectUriBase, userToken);
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
			_sharedTenantWriterId = CreateSharedUserWithName("testtenantwriter1_"+System.Guid.NewGuid().ToString());
			Console.WriteLine($"Shared TenantWriterId = {_sharedTenantWriterId}");
			_sharedTenantAdminId = CreateSharedUserWithName("testtenantadmin1_" + System.Guid.NewGuid().ToString());
			Console.WriteLine($"Shared TenantAdminId = {_sharedTenantAdminId}");
			_sharedTenantReaderId = CreateSharedUserWithName("testtenantreader1_" + System.Guid.NewGuid().ToString());
			Console.WriteLine($"Shared TenantReaderId = {_sharedTenantReaderId}");

			_sharedTenantWriterId2 = CreateSharedUserWithName("testtenantwriter2_" + System.Guid.NewGuid().ToString());
			Console.WriteLine($"Shared TenantWriterId = {_sharedTenantWriterId}");
			_sharedTenantAdminId2 = CreateSharedUserWithName("testtenantadmin2_" + System.Guid.NewGuid().ToString());
			Console.WriteLine($"Shared TenantAdminId = {_sharedTenantAdminId}");
			_sharedTenantReaderId2 = CreateSharedUserWithName("testtenantreader2_" + System.Guid.NewGuid().ToString());
			Console.WriteLine($"Shared TenantReaderId = {_sharedTenantReaderId}");
		}

		private string CreateSharedUserWithName(string userName)
		{
			string AddUserURITemplate1 = $"{UriBase}/IssueIdentityUser";
			string SetUserPasswordTemplat1 = $"{UriBase}/IssueIdentityUser/password?password=password";
			Console.WriteLine($"Create user Uri:{AddUserURITemplate1}");
			IssueIdentityUser user1 = new IssueIdentityUser
			{
				id = "temp",
				name = userName,
				firstname = "writer",
				lastname = "writer",
				email = "tenantwriter@email.com",
				userstatus = 0
			};

			string userJson1 = JsonSerializer.Serialize(user1);
			string userId = CreateAndPasswordAndReturnId(AddUserURITemplate1, SetUserPasswordTemplat1, userJson1);
			return userId;
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
			_sharedTenantId = CreateTenantAndSetWriterAndReader(_sharedTenantAdminId, _tenantName, _adminToken, _sharedTenantWriterId, _sharedTenantReaderId);
			_sharedTenantId2 = CreateTenantAndSetWriterAndReader(_sharedTenantAdminId2, _tenantName2, _adminToken, _sharedTenantWriterId2, _sharedTenantReaderId2);
		}

		private string CreateTenantAndSetWriterAndReader(string tenantAdminId, string tenantName, string userToken, string tenantWriterId, string tenantReaderId)
		{
			string tenantId;
			string AddTenantURITemplate = $"{UriBase}/Tenant/Tenant?tenantName={tenantName}&ownerId={tenantAdminId}&ownerEmail={_ownerEmail}&ownerName={_ownerName}";
			Console.WriteLine($"Create tenant Uri:{AddTenantURITemplate}");

			WebRequest webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(string.Empty, AddTenantURITemplate, userToken);
			WebResponse webResponse = webRequest.GetResponse();
			using (var stream = webResponse.GetResponseStream())
			{
				StreamReader sr = new StreamReader(stream);
				tenantId = sr.ReadToEnd().Replace("\"", string.Empty);
				Console.WriteLine($"Shared TenantId = {tenantId}");

			}

			Console.WriteLine($"Set tenant roles for users.");
			string setUserRoleUriTemplate = $"{UriBase}/Tenant/Tenant/UserAuthorization?tenantId={tenantId}";
			webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(string.Empty, setUserRoleUriTemplate + $"&userId={tenantWriterId}&userTenantAuthorization=2", userToken);
			webResponse = webRequest.GetResponse();
			webRequest = IssueIdentityApiTestsHelpers.CreatePostRequest(string.Empty, setUserRoleUriTemplate + $"&userId={tenantReaderId}&userTenantAuthorization=3", userToken);
			webResponse = webRequest.GetResponse();
			return tenantId;
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
			_tenantAdminToken2 = GetClaimForUserId(_sharedTenantAdminId2);
			Console.WriteLine($"Shared tenant admin token = {_tenantAdminToken}");
			_tenantWriterToken2 = GetClaimForUserId(_sharedTenantWriterId2);
			Console.WriteLine($"Shared tenant writer token = {_tenantWriterToken}");
			_tenantReaderToken2 = GetClaimForUserId(_sharedTenantReaderId2);
			Console.WriteLine($"Shared tenant reader token = {_tenantReaderToken}");
		}

		private string GetClaimForUserId(string userId)
		{
			string GetTokenUriTemplate = $"{UriBase}/token?password=password";
			Console.WriteLine($"Get token Uri:{GetTokenUriTemplate}");
			Console.WriteLine($"Get claim for userid:{userId}");
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