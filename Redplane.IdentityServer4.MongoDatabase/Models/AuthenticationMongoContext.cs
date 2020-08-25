using System;
using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Models
{
    public class AuthenticationMongoContext : IAuthenticationMongoContext
    {
        #region Constructor

        public AuthenticationMongoContext(IMongoDatabase database,
            string contextName, string clientsCollectionName, string identityResourcesCollectionName,
            string apiResourcesCollectionName, string persistedGrantsCollectionName)
        {
            Name = contextName;
            Database = database;
            Client = database.Client;

            if (string.IsNullOrWhiteSpace(clientsCollectionName))
                throw new ArgumentException($"{nameof(clientsCollectionName)} cannot be either null or empty.");

            if (string.IsNullOrWhiteSpace(identityResourcesCollectionName))
                throw new ArgumentException($"{nameof(identityResourcesCollectionName)} cannot be either null or empty.");

            if (string.IsNullOrWhiteSpace(apiResourcesCollectionName))
                throw new ArgumentException($"{nameof(apiResourcesCollectionName)} cannot be either null or empty.");

            if (string.IsNullOrWhiteSpace(persistedGrantsCollectionName))
                throw new ArgumentException($"{nameof(persistedGrantsCollectionName)} cannot be either null or empty.");

            var clients = Database.GetCollection<Client>(clientsCollectionName);

            var identityResources =
                Database.GetCollection<IdentityResource>(identityResourcesCollectionName);

            var apiResources = Database.GetCollection<ApiResource>(apiResourcesCollectionName);

            var persistedGrants =
                Database.GetCollection<PersistedGrant>(persistedGrantsCollectionName);

            Collections = new AuthenticationDbCollections(clients, persistedGrants, apiResources, identityResources);
        }

        #endregion

        #region Accessors

        public string Name { get; }

        public IMongoClient Client { get; }

        public IMongoDatabase Database { get; }

        public IAuthenticationMongoCollections Collections { get; }

        #endregion
    }
}