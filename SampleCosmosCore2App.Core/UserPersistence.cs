using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleCosmosCore2App.Core
{
    public class UserPersistence : IDisposable
    {
        private const string USERS_DOCUMENT_ID = "Users";
        private const string SESSIONS_DOCUMENT_ID = "Sessions";
        private string _databaseId;
        private Uri _endpointUri;
        private string _primaryKey;

        private DocumentClient _client;
        private bool _isDisposing;

        public UserPersistence(Uri endpointUri, string primaryKey, string databaseId)
        {
            _databaseId = databaseId;
            _endpointUri = endpointUri;
            _primaryKey = primaryKey;
        }

        private async Task EnsureSetupAsync()
        {
            if (_client == null)
            {
                _client = new DocumentClient(_endpointUri, _primaryKey);
            }

            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId });
            var databaseUri = UriFactory.CreateDatabaseUri(_databaseId);

            // Collections
            await _client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection() { Id = SESSIONS_DOCUMENT_ID });
            var users = new DocumentCollection();
            users.Id = USERS_DOCUMENT_ID;
            //users.PartitionKey.Paths.Add("/Username");
            users.UniqueKeyPolicy = new UniqueKeyPolicy()
            {
                UniqueKeys = new Collection<UniqueKey>()
                {
                    new UniqueKey{ Paths = new Collection<string>{ "/Username" } }
                }
            };
            await _client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, users);
        }


        #region Users

        public async Task<LoginUser> CreateUserAsync(LoginUser user)
        {
            await EnsureSetupAsync();

            var result = await _client.CreateDocumentAsync(GetUsersCollectionUri(), user, new RequestOptions() { });
            return JsonConvert.DeserializeObject<LoginUser>(result.Resource.ToString());
        }

        public async Task<LoginUser> GetUserAsync(string userId)
        {
            await EnsureSetupAsync();

            var result = await _client.ReadDocumentAsync<LoginUser>(UriFactory.CreateDocumentUri(_databaseId, USERS_DOCUMENT_ID, userId));
            return result.Document;
        }

        public async Task<LoginUser> GetUserByUsernameAsync(string userName)
        {
            await EnsureSetupAsync();

            var query = _client.CreateDocumentQuery<LoginUser>(GetUsersCollectionUri(), new SqlQuerySpec()
            {
                QueryText = "SELECT * FROM Users U WHERE U.Username = @username",
                Parameters = new SqlParameterCollection()
               {
                   new SqlParameter("@username", userName)
               }
            });
            var results = await query.AsDocumentQuery()
                                     .ExecuteNextAsync<LoginUser>();
            return results.FirstOrDefault();
        }

        #endregion

        #region Sessions

        public async Task<LoginSession> CreateSessionAsync(LoginSession session)
        {
            await EnsureSetupAsync();

            var result = await _client.CreateDocumentAsync(GetSessionsCollectionUri(), session);
            return JsonConvert.DeserializeObject<LoginSession>(result.Resource.ToString());
        }

        public async Task<LoginSession> GetSessionAsync(string sessionId)
        {
            await EnsureSetupAsync();

            var result = await _client.ReadDocumentAsync<LoginSession>(UriFactory.CreateDocumentUri(_databaseId, SESSIONS_DOCUMENT_ID, sessionId));
            return result.Document;
        }

        public async Task UpdateSessionAsync(LoginSession session)
        {
            await EnsureSetupAsync();

            await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, SESSIONS_DOCUMENT_ID, session.Id), session);
        }

        #endregion

        private Uri GetUsersCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, USERS_DOCUMENT_ID);
        }

        private Uri GetSessionsCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, SESSIONS_DOCUMENT_ID);
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
