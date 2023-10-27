using DupIQ.IssueIdentity;
using DupIQ.IssueIdentityProviders.Word2Vec_Pinecone;

namespace DupIQ.IssueIdentity.Tests
{
	[TestClass]
	public class PineconeIdentityProvider_unittests
	{
		[TestMethod]
		public void AddIssueProfile()
		{
			string passedInMessage = string.Empty;
			string passedInId = string.Empty;
			Embeddings passedInEmbeddings = null;

			MockVectorHelper mockVectorHelper = new MockVectorHelper();
			mockVectorHelper.overrideGetEmbeddings = (s) => { passedInMessage = s; return new Embeddings() { Values = new float[] { 1.0f, 2.0f } }; };
			mockVectorHelper.overrideAddVectorTenantConfigProjId = (s, e, t, s2) => { passedInId = s; passedInEmbeddings = e; };

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			pineconeIdentityProvider.AddIssueProfile(new IssueProfile() { IssueId = "testid", ExampleMessage = "test message" }, null, string.Empty);
			Assert.AreEqual("test message", passedInMessage, "Fail if the message passed in to GetEmbeddings was not expected.");
			Assert.AreEqual("testid", passedInId, "Fail if the id passed in to AddVector was not expected.");
			Assert.IsNotNull(passedInEmbeddings, "Fail if passedInEmbeddings is still null, means AddVector was never called.");
			Assert.AreEqual(2, passedInEmbeddings.Values.Count(), "Fail if the number of values in the embedding passed in to AddVector are not expected.");
			Assert.AreEqual(1.0f, passedInEmbeddings.Values[0], "Fail if the first embedding value passed in to AddVector was not expected.");
		}

		[TestMethod]
		public void DeleteIssueProfile()
		{
			string passedInMessage = string.Empty;
			string passedInId = string.Empty;
			Embeddings passedInEmbeddings = null;

			MockVectorHelper mockVectorHelper = new MockVectorHelper();
			mockVectorHelper.overrideGetEmbeddings = (s) => { passedInMessage = s; return new Embeddings() { Values = new float[] { 1.0f, 2.0f } }; };
			mockVectorHelper.overrideAddVectorTenantConfigProjId = (s, e, t, s2) => { passedInId = s; passedInEmbeddings = e; };
			mockVectorHelper.overrideDeleteVectorProjId = (i, t, s2) => { passedInId = i; };

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			TenantConfiguration tenantConfiguration = new TenantConfiguration() { TenantId = "test tenant" };
			pineconeIdentityProvider.AddIssueProfile(new IssueProfile() { IssueId = "testid", ExampleMessage = "test message" }, tenantConfiguration, string.Empty);
			Assert.AreEqual("testid", passedInId, "Fail if the id passed in to AddVector was not expected.");

			passedInId = string.Empty;
			pineconeIdentityProvider.DeleteIssueProfile("testid", tenantConfiguration, string.Empty);
			Assert.AreEqual("testid", passedInId, "Fail if the id passed in to DeleteVector was not expected.");

		}

		[TestMethod]
		public void AddIssueProfile_idisnull()
		{
			MockVectorHelper mockVectorHelper = new MockVectorHelper();

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			try
			{
				pineconeIdentityProvider.AddIssueProfile(new IssueProfile() { ExampleMessage = "test message" }, null, string.Empty);
				Assert.Fail("Should not get this far because it should have thrown.", string.Empty);
			}
			catch (ArgumentNullException e)
			{
				Assert.AreEqual("IssueId", e.ParamName, "Fail if exception does not mention correct parameter.");
			}
		}

		[TestMethod]
		public void AddIssueProfile_issueMessageisnull()
		{
			MockVectorHelper mockVectorHelper = new MockVectorHelper();

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);

			try
			{
				pineconeIdentityProvider.AddIssueProfile(new IssueProfile() { IssueId = "testid" }, null, string.Empty);
				Assert.Fail("Should not get this far because it should have thrown.", string.Empty);
			}
			catch (ArgumentNullException e)
			{
				Assert.AreEqual("ExampleMessage", e.ParamName, "Fail if exception does not mention correct parameter.");
			}
		}

		[TestMethod]
		public void GetIssueProfile()
		{
			string passedInId = string.Empty;

			MockVectorHelper mockVectorHelper = new MockVectorHelper();
			mockVectorHelper.overrideGetVector = (s) => { passedInId = s; return new Embeddings() { Id = s, Similarity = 0.0f, Values = new float[] { 0.9f, 0.8f } }; };

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			IssueProfile issueProfile = pineconeIdentityProvider.GetIssueProfile("testid", null, string.Empty);
			Assert.AreEqual("testid", issueProfile.IssueId, "Fail if the returned issueProfile.Id is not as expected.");
			Assert.AreEqual("testid", passedInId, "Fail if passedInId was not set, indicating GetVector was never called.");
		}

		[TestMethod]
		public void GetIssueProfile_idisnull()
		{
			MockVectorHelper mockVectorHelper = new MockVectorHelper();

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			try
			{
				IssueProfile issueProfile = pineconeIdentityProvider.GetIssueProfile(null, null, string.Empty);
				Assert.Fail("Should fail if got here because it should have thrown.", string.Empty);
			}
			catch (ArgumentNullException e)
			{
				Assert.AreEqual("id", e.ParamName, "Fail if exception does not mention expected parameter name.");
			}
		}

		[TestMethod]
		public void GetRelatedIssueProfiles()
		{
			string passedInId = string.Empty;

			MockVectorHelper mockVectorHelper = new MockVectorHelper();
			mockVectorHelper.overrideGetEmbeddings = (s) => { return new Embeddings() { Values = new float[] { 1.1f, 2.2f } }; };
			mockVectorHelper.overrideQueryTenantConfigProjId = (e, c, t, s2) =>
			{
				return new Embeddings[]
				{
					new Embeddings() { Id="testid1", Values=new float[] { 1.1f, 2.2f }, Similarity=1.0f },
					new Embeddings() { Id="testid2", Values=new float[] { 1.2f, 2.2f }, Similarity=0.9f }
				};
			};

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			RelatedIssueProfile[] relatedIssueProfiles = pineconeIdentityProvider.GetRelatedIssueProfiles("some issues I saw", 5, null, string.Empty);
			Assert.AreEqual(2, relatedIssueProfiles.Count(), "Fail if there are not 2 related profiles returned.");
			Assert.AreEqual(1.0f, relatedIssueProfiles[0].Similarity, "Fail if the similarity of first item is not expected.");
			Assert.AreEqual("testid1", relatedIssueProfiles[0].IssueId, "Fail if the id of first item is not expected.");
			Assert.AreEqual(0.9f, relatedIssueProfiles[1].Similarity, "Fail if the similarity of second item is not expected.");
			Assert.AreEqual("testid2", relatedIssueProfiles[1].IssueId, "Fail if the id of second item is not expected.");
		}

		[TestMethod]
		public void GetRelatedIssueProfiles_norelatedissues()
		{
			string passedInId = string.Empty;

			MockVectorHelper mockVectorHelper = new MockVectorHelper();
			mockVectorHelper.overrideGetEmbeddings = (s) => { return new Embeddings() { Values = new float[] { 1.1f, 2.2f } }; };
			mockVectorHelper.overrideQueryTenantConfigProjId = (e, c, t, s2) => { return new Embeddings[] { }; };

			PineconeIdentityProvider pineconeIdentityProvider = new PineconeIdentityProvider(mockVectorHelper);
			RelatedIssueProfile[] relatedIssueProfiles = pineconeIdentityProvider.GetRelatedIssueProfiles("some issues I saw", 5, null, string.Empty);
			Assert.AreEqual(0, relatedIssueProfiles.Count(), "Fail if there are any related profiles returned.");
		}
	}
}