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

namespace SampleCosmosCore2App.Core.Users
{
    public class UserPersistence
    {
        private const string USERS_DOCUMENT_COLLECTION_ID = "Users";
        private const string AUTHS_DOCUMENT_COLLECTION_ID = "UserAuthentications";
        private const string SESSIONS_DOCUMENT_COLLECTION_ID = "Sessions";
        private DocumentClient _client;
        private string _databaseId;

        public UserPersistence(DocumentClient client, string databaseId)
        {
            _client = client;
            _databaseId = databaseId;
        }

        public async Task EnsureSetupAsync()
        {
            var databaseUri = UriFactory.CreateDatabaseUri(_databaseId);

            // Collections
            await _client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection() { Id = SESSIONS_DOCUMENT_COLLECTION_ID });
            await _client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection() { Id = AUTHS_DOCUMENT_COLLECTION_ID });
            var users = new DocumentCollection();
            users.Id = USERS_DOCUMENT_COLLECTION_ID;
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
            var result = await _client.CreateDocumentAsync(GetUsersCollectionUri(), user, new RequestOptions() { });
            return JsonConvert.DeserializeObject<LoginUser>(result.Resource.ToString());
        }

        public async Task<LoginUser> GetUserAsync(string userId)
        {
            var result = await _client.ReadDocumentAsync<LoginUser>(UriFactory.CreateDocumentUri(_databaseId, USERS_DOCUMENT_COLLECTION_ID, userId));
            return result.Document;
        }

        public async Task<LoginUser> GetUserByUsernameAsync(string userName)
        {
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
        
        public async Task<LoginUser> GetUserByAuthenticationAsync(AuthenticationScheme authenticationScheme, string identity)
        {
            var query = _client.CreateDocumentQuery<LoginUserAuthentication>(GetAuthenticationsCollectionUri(), new SqlQuerySpec()
            {
                QueryText = "SELECT * FROM UserAuthentications UA WHERE UA.Scheme = @scheme AND UA.Identity = @identity",
                Parameters = new SqlParameterCollection()
               {
                    new SqlParameter("@scheme", authenticationScheme),
                    new SqlParameter("@identity", identity)
               }
            });
            var results = await query.AsDocumentQuery()
                                     .ExecuteNextAsync<LoginUserAuthentication>();
            if (results.Count == 0)
            {
                return null;
            }
            else
            {
                return await GetUserAsync(results.First().UserId);
            }
        }

        public async Task<bool> IsUsernameRegisteredAsync(string username)
        {
            var query = _client.CreateDocumentQuery<int>(GetUsersCollectionUri(), new SqlQuerySpec()
            {
                QueryText = "SELECT VALUE COUNT(1) FROM Users WHERE U.Username = @username",
                Parameters = new SqlParameterCollection()
               {
                   new SqlParameter("@username", username)
               }
            });
            var result = await query.AsDocumentQuery()
                                    .ExecuteNextAsync<int>();
            return result.Single() == 1;
        }

        public async Task DeleteUserAsync(LoginUser user)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, USERS_DOCUMENT_COLLECTION_ID, user.Id));
        }

        #endregion

        #region Additional Authentication Methods

        public async Task<LoginUserAuthentication> CreateUserAuthenticationAsync(LoginUserAuthentication userAuth)
        {
            var result = await _client.CreateDocumentAsync(GetAuthenticationsCollectionUri(), userAuth, new RequestOptions() { });
            return JsonConvert.DeserializeObject<LoginUserAuthentication>(result.Resource.ToString());
        }


        public async Task<bool> IsIdentityRegisteredAsync(AuthenticationScheme authenticationScheme, string identity)
        {
            var query = _client.CreateDocumentQuery<int>(GetAuthenticationsCollectionUri(), new SqlQuerySpec()
            {
                QueryText = "SELECT VALUE COUNT(1) FROM UserAuthentications  UA WHERE UA.Scheme = @scheme AND UA.Identity = @identity",
                Parameters = new SqlParameterCollection()
               {
                    new SqlParameter("@scheme", authenticationScheme),
                    new SqlParameter("@identity", identity)
               }
            });
            var result = await query.AsDocumentQuery()
                                    .ExecuteNextAsync<int>();
            return result.Single() == 1;
        }

        #endregion

        #region Sessions

        public async Task<LoginSession> CreateSessionAsync(LoginSession session)
        {
            var result = await _client.CreateDocumentAsync(GetSessionsCollectionUri(), session);
            return JsonConvert.DeserializeObject<LoginSession>(result.Resource.ToString());
        }

        public async Task<LoginSession> GetSessionAsync(string sessionId)
        {
            var result = await _client.ReadDocumentAsync<LoginSession>(UriFactory.CreateDocumentUri(_databaseId, SESSIONS_DOCUMENT_COLLECTION_ID, sessionId));
            return result.Document;
        }

        public async Task UpdateSessionAsync(LoginSession session)
        {
            await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, SESSIONS_DOCUMENT_COLLECTION_ID, session.Id), session);
        }

        #endregion

        private Uri GetUsersCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, USERS_DOCUMENT_COLLECTION_ID);
        }

        private Uri GetAuthenticationsCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, AUTHS_DOCUMENT_COLLECTION_ID);
        }

        private Uri GetSessionsCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, SESSIONS_DOCUMENT_COLLECTION_ID);
        }

    }
}
