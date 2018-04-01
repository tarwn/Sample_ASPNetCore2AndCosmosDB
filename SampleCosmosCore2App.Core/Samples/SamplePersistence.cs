using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Core.Samples
{
    public class SamplePersistence
    {
        private const string DOCUMENT_COLLECTION_ID = "Samples";
        private DocumentClient _client;
        private string _databaseId;

        public SamplePersistence(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task EnsureSetupAsync()
        {
            var databaseUri = UriFactory.CreateDatabaseUri(_databaseId);

            // Collections
            await _client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection() { Id = DOCUMENT_COLLECTION_ID });
        }

        public async Task SaveSampleAsync(Sample sample)
        {
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, DOCUMENT_COLLECTION_ID);
            await _client.UpsertDocumentAsync(documentCollectionUri, sample);
        }

        public async Task<Sample> GetSampleAsync(string Id)
        {
            var documentUri = UriFactory.CreateDocumentUri(_databaseId, DOCUMENT_COLLECTION_ID, Id);
            var result = await _client.ReadDocumentAsync<Sample>(documentUri);
            return result.Document;
        }

        public async Task<List<Sample>> GetSamplesAsync()
        {
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, DOCUMENT_COLLECTION_ID);

            // build the query
            var feedOptions = new FeedOptions() { MaxItemCount = -1 };
            var query = _client.CreateDocumentQuery<Sample>(documentCollectionUri, "SELECT * FROM Sample", feedOptions);
            var queryAll = query.AsDocumentQuery();

            // combine the results
            var results = new List<Sample>();
            while (queryAll.HasMoreResults)
            {
                results.AddRange(await queryAll.ExecuteNextAsync<Sample>());
            }

            return results;
        }
    }
}
