using IdentityServer4.Models;
using IdentityServer4.MongoDbAdapter.Constants;
using IdentityServer4.MongoDbAdapter.Interfaces;
using IdentityServer4.MongoDbAdapter.Interfaces.Contexts;
using MongoDB.Driver;

namespace IdentityServer4.MongoDbAdapter.Models
{
    public class AuthenticationMongoContext : IAuthenticationMongoContext
    {
        #region Constructor

        public AuthenticationMongoContext(IMongoClient client, string databaseName,
            MongoDatabaseSettings mongoDatabaseSettings = null)
        {
            Client = client;
            Database = client.GetDatabase(databaseName, mongoDatabaseSettings);

            var clients = Database.GetCollection<Client>(AuthenticationCollectionNameConstants.Clients);
            var identityResources =
                Database.GetCollection<IdentityResource>(AuthenticationCollectionNameConstants.IdentityResources);
            var apiResources = Database.GetCollection<ApiResource>(AuthenticationCollectionNameConstants.ApiResources);
            var persistedGrants =
                Database.GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            Collections = new AuthenticationMongoCollections(clients, persistedGrants, apiResources, identityResources);
        }

        #endregion

        #region Properties

        public IMongoClient Client { get; }

        public IMongoDatabase Database { get; }

        public IAuthenticationMongoCollections Collections { get; }

        #endregion
    }
}