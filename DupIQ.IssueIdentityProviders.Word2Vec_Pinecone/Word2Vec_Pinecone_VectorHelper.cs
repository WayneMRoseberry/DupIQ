using DupIQ.IssueIdentity;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using Pinecone;
using Pinecone.Grpc;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DupIQ.IssueIdentityProviders.Word2Vec_Pinecone
{
	public class Word2Vec_Pinecone_VectorHelper : IVectorHelper
	{
		private const string FAILUREIDVECTORS = "failureidvectors";
		private const int VECTORSIZE = 150; // <- changed to glove50, which I think is 150 features <- gl0ve300d, sentimentspecific embedding was 150;
		private Word2Vec_Pinecone_Config config;
		private PredictionEngine<TextData, TransformedTextData> predictionEngine;
		private PineconeClient pinecone;
		private Index<GrpcTransport> index;
		private ILogger _logger;

		private string apiKey;
		private string environment;
		public float isdenticalIssueThreshold = 1.0f;

		private Index<GrpcTransport> Index
		{
			get
			{
				if (index == null)
				{
					index = GetIndex(FAILUREIDVECTORS);
				}
				return index;
			}
		}

		public Word2Vec_Pinecone_VectorHelper(string configJson)
		{
			ILogger temp = LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger<Word2Vec_Pinecone_VectorHelper>();
			Initialize(configJson, temp);
		}

		public Word2Vec_Pinecone_VectorHelper(string configJson, ILogger logger)
		{
			Initialize(configJson, logger);
		}

		private void Initialize(string configJson, ILogger temp)
		{
			_logger = temp;
			_logger.LogInformation(SharedEvents.ProviderStartup, "Word2Vec_Pinecone_Config Json:{ConfigJson}", configJson);
			Word2Vec_Pinecone_Config word2Vec_Pinecone_Config = JsonSerializer.Deserialize<Word2Vec_Pinecone_Config>(configJson);
			Initialize(word2Vec_Pinecone_Config);
		}

		public Word2Vec_Pinecone_VectorHelper(Word2Vec_Pinecone_Config config)
		{
			_logger = LoggerFactory.Create(config2 => { config2.AddConsole(); }).CreateLogger("Word2Vec_Pinecone_VectorHelper");
			Initialize(config);
		}

		private void Initialize(Word2Vec_Pinecone_Config config)
		{
			this.config = config;
			predictionEngine = GetPredictionEngine(_logger);
			apiKey = config.ApiKey;
			environment = config.Environment;
			isdenticalIssueThreshold = config.Threshold;
			pinecone = new PineconeClient(apiKey, environment);
		}

		public void AddVector(string id, Embeddings embeddings)
		{
			List<Vector> v = new List<Vector>();
			v.Add(new Vector() { Id = id, Values = embeddings.Values });
			var t = Index.Upsert(v);
			t.Wait();
		}

		public void AddVector(string id, Embeddings embeddings, TenantConfiguration tenantConfiguration)
		{
			AddVector(id, embeddings, tenantConfiguration, string.Empty);
		}

		public void AddVector(string id, Embeddings embeddings, TenantConfiguration tenantConfiguration, string projectId)
		{
			List<Vector> v = new List<Vector>();
			v.Add(new Vector() { Id = id, Values = embeddings.Values });
			string namespacePartition = CreateNameSpacePartitionFromTenantIdAndProjectId(tenantConfiguration, projectId);
			var t = Index.Upsert(v, indexNamespace: namespacePartition);
			t.Wait();
		}

		public void DeleteIndex(string indexName)
		{
			var t = pinecone.DeleteIndex(indexName);
			t.Wait();
		}

		public void DeleteVector(string id, TenantConfiguration tenantConfiguration)
		{
			DeleteVector(id, tenantConfiguration, string.Empty);
		}
		public void DeleteVector(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			string[] deleteThese = new string[] { id };
			Index.Delete(deleteThese, indexNamespace: CreateNameSpacePartitionFromTenantIdAndProjectId(tenantConfiguration, projectId));
		}

		public void EmptyIndex(string tenantId, string projectId)
		{
			var t = Index.DeleteAll(indexNamespace: CreateNameSpacePartitionFromTenantIdAndProjectId(tenantId, projectId));
			t.Wait();
		}

		public void EmptyIndex()
		{
			var t = Index.DeleteAll();
			t.Wait();
		}

		public Embeddings GetEmbeddings(string issueMessage)
		{
			return new Embeddings
			{
				Values = GetPredictionFromFailureMessage(predictionEngine, issueMessage).Features
			};
		}

		public Embeddings GetEmbeddings(string issueMessage, TenantConfiguration tenantConfiguration)
		{
			throw new NotImplementedException();
		}

		public Embeddings GetEmbeddings(string issueMessage, TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}
		public static PredictionEngine<TextData, TransformedTextData> GetPredictionEngine()
		{
			ILogger logger = LoggerFactory.Create(config => { config.AddConsole(); }).CreateLogger("Word2Vec_Pinecone_VectorHelper");
			return GetPredictionEngine(logger);
		}

		private static PredictionEngine<TextData, TransformedTextData> GetPredictionEngine(ILogger logger)
		{
			logger.LogInformation("GetPredictionEngine.");
			// Create a new ML context, for ML.NET operations. It can be used for
			// exception tracking and logging, as well as the source of randomness.
			var mlContext = new MLContext();

			// Create an empty list as the dataset. The 'ApplyWordEmbedding' does
			// not require training data as the estimator ('WordEmbeddingEstimator')
			// created by 'ApplyWordEmbedding' API is not a trainable estimator.
			// The empty list is only needed to pass input schema to the pipeline.
			var emptySamples = new List<TextData>();

			// Convert sample list to an empty IDataView.
			var emptyDataView = mlContext.Data.LoadFromEnumerable(emptySamples);

			// A pipeline for converting text into a 150-dimension embedding vector
			// using pretrained 'SentimentSpecificWordEmbedding' model. The
			// 'ApplyWordEmbedding' computes the minimum, average and maximum values
			// for each token's embedding vector. Tokens in 
			// 'SentimentSpecificWordEmbedding' model are represented as
			// 50 -dimension vector. Therefore, the output is of 150-dimension [min,
			// avg, max].
			//
			// The 'ApplyWordEmbedding' API requires vector of text as input.
			// The pipeline first normalizes and tokenizes text then applies word
			// embedding transformation.
			//
			// I changed it to glove300d instead of sentimentspecificwordembedding to see
			// what performance I get.
			// ...then FastTextWikipedia300d
			var textPipeline = mlContext.Transforms.Text.NormalizeText("Text")
				.Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens",
					"Text"))
				.Append(mlContext.Transforms.Text.ApplyWordEmbedding("Features",
					"Tokens", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D
			));

			logger.LogInformation("Fit data to prediction engine model.");
			// Fit to data.
			var textTransformer = textPipeline.Fit(emptyDataView);

			logger.LogInformation("Create the engine from the fit data.");
			// Create the prediction engine to get the embedding vector from the
			// input text/string.
			var predictionEngine = mlContext.Model.CreatePredictionEngine<TextData,
				TransformedTextData>(textTransformer);

			logger.LogInformation("Engine created, return to caller.");
			return predictionEngine;
		}

		public static TransformedTextData GetPredictionFromFailureMessage(PredictionEngine<TextData, TransformedTextData> p, string failureMessage)
		{
			TextData t = new TextData() { Text = failureMessage };
			return p.Predict(t);
		}

		public Embeddings GetVector(string id)
		{
			Dictionary<string, Vector> v = Index.Fetch(new string[] { id }).Result;

			if (v.ContainsKey(id))
			{
				Vector item = v[id];
				Embeddings result = new Embeddings() { Id = id, Values = item.Values, Similarity = 1.0f };
				return result;
			}
			throw new IssueDoesNotExistException(id);
		}

		public Embeddings GetVector(string id, TenantConfiguration tenantConfiguration)
		{
			return GetVector(id, tenantConfiguration, string.Empty);
		}

		public Embeddings GetVector(string id, TenantConfiguration tenantConfiguration, string projectId)
		{
			Dictionary<string, Vector> v = Index.Fetch(new string[] { id }, indexNamespace: CreateNameSpacePartitionFromTenantIdAndProjectId(tenantConfiguration, projectId)).Result;

			if (v.ContainsKey(id))
			{
				Vector item = v[id];
				Embeddings result = new Embeddings() { Id = id, Values = item.Values, Similarity = 1.0f };
				return result;
			}
			throw new IssueDoesNotExistException(id);
		}


		public float IdenticalIssueThreshold()
		{
			return config.Threshold;
		}

		public float IdenticalIssueThreshold(TenantConfiguration tenantConfiguration, string projectId)
		{
			throw new NotImplementedException();
		}

		public Embeddings[] Query(Embeddings embeddings, int count)
		{
			List<Embeddings> result = new List<Embeddings>();
			float[] stuff = embeddings.Values.Take(VECTORSIZE).ToArray();
			ScoredVector[] scoredVectors = Index.Query(stuff, (uint)count).Result;
			foreach (ScoredVector sv in scoredVectors)
			{
				result.Add(new Embeddings() { Id = sv.Id, Similarity = (float)sv.Score, Values = sv.Values });
			}
			return result.ToArray();
		}

		public Embeddings[] Query(Embeddings embeddings, int count, TenantConfiguration tenantConfiguration)
		{
			return Query(embeddings, count, tenantConfiguration, string.Empty);
		}

		public Embeddings[] Query(Embeddings embeddings, int count, TenantConfiguration tenantConfiguration, string projectId)
		{
			List<Embeddings> result = new List<Embeddings>();
			float[] stuff = embeddings.Values.Take(VECTORSIZE).ToArray();
			ScoredVector[] scoredVectors = Index.Query(stuff, (uint)count, indexNamespace: CreateNameSpacePartitionFromTenantIdAndProjectId(tenantConfiguration, projectId)).Result;
			foreach (ScoredVector sv in scoredVectors)
			{
				result.Add(new Embeddings() { Id = sv.Id, Similarity = (float)sv.Score, Values = sv.Values });
			}
			return result.ToArray();
		}

		private static string CreateNameSpacePartitionFromTenantIdAndProjectId(TenantConfiguration tenantConfiguration, string projectId)
		{
			string tenantId = tenantConfiguration.TenantId;
			return CreateNameSpacePartitionFromTenantIdAndProjectId(projectId, tenantId);
		}

		private static string CreateNameSpacePartitionFromTenantIdAndProjectId(string projectId, string tenantId)
		{
			return string.IsNullOrEmpty(projectId) ? tenantId : $"{tenantId}.{projectId}";
		}

		private Index<GrpcTransport> GetIndex(string indexName)
		{
			// List all indexes
			var indexes = pinecone.ListIndexes().Result;

			// Create a new index if it doesn't exist

			if (!indexes.Contains(indexName))
			{
				Task t = pinecone.CreateIndex(indexName, 150, Metric.Cosine);
				t.Wait();
			}

			// Get an index by name
			Index<GrpcTransport> fetchedIndex = pinecone.GetIndex(indexName).Result;
			return fetchedIndex;
		}
	}

	public class Word2Vec_Pinecone_Config
	{
		public string ApiKey { get; set; }
		public string Environment { get; set; }
		public string IdenticalIssueThreshold { get; set; }

		public float Threshold
		{
			get
			{
				float output = 0;
				float.TryParse(IdenticalIssueThreshold, out output);
				return output;
			}
		}
	}

	public class TextData
	{
		public string Text { get; set; }
	}

	public class TransformedTextData
	{
		public float[] Features { get; set; }
	}
}
