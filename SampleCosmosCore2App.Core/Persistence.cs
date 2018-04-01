using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using SampleCosmosCore2App.Core.Samples;
using SampleCosmosCore2App.Core.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Core
{
    public class Persistence : IDisposable
    {
        private string _databaseId;
        private Uri _endpointUri;
        private string _primaryKey;

        private DocumentClient _client;
        private bool _isDisposing;

        public Persistence(Uri endpointUri, string primaryKey, string databaseId)
        {
            _databaseId = databaseId;
            _endpointUri = endpointUri;
            _primaryKey = primaryKey;

            _client = new DocumentClient(endpointUri, primaryKey);
            _client.OpenAsync();

            Samples = new SamplePersistence(_client, _databaseId);
            Users = new UserPersistence(_client, _databaseId);
        }

        public UserPersistence Users { get; private set; }

        public SamplePersistence Samples { get; private set; }

        public async Task EnsureSetupAsync()
        {
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId });

            await Samples.EnsureSetupAsync();
            await Users.EnsureSetupAsync();
        }
        
        public void Dispose()
        {
            if (!_isDisposing)
            {
                _isDisposing = true;
                if (_client != null)
                {
                    _client.Dispose();
                }
            }
        }
    }
}
