using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Core
{
    public class Persistence
    {
        private string _databaseId;
        private Uri _endpointUri;
        private string _primaryKey;
        private DocumentClient _client;

        public Persistence(Uri endpointUri, string primaryKey)
        {
            _databaseId = "QuoteServiceDB";
            _endpointUri = endpointUri;
            _primaryKey = primaryKey;
        }

        public async Task EnsureSetupAsync()
        {
            if (_client == null)
            {
                _client = new DocumentClient(_endpointUri, _primaryKey);
            }

            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId });
            var databaseUri = UriFactory.CreateDatabaseUri(_databaseId);

            // Samples
            await _client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection() { Id = "SamplesCollection" });
        }

        public async Task SaveSampleAsync(Sample sample)
        {
            await EnsureSetupAsync();

            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, "SamplesCollection");
            await _client.UpsertDocumentAsync(documentCollectionUri, sample);
        }

        public async Task<Sample> GetSampleAsync(string Id)
        {
            await EnsureSetupAsync();

            var documentUri = UriFactory.CreateDocumentUri(_databaseId, "SamplesCollection", Id);
            var result = await _client.ReadDocumentAsync<Sample>(documentUri);
            return result.Document;
        }

        public async Task<List<Sample>> GetSamplesAsync()
        {
            await EnsureSetupAsync();

            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, "SamplesCollection");

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
