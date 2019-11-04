using IdentityServer4.Models;
using IdentityServer4.Mongo.Interfaces;
using MongoDB.Driver;

namespace IdentityServer4.Mongo.Models
{
    public class AuthenticationMongoCollections : IAuthenticationMongoCollections
    {
        #region Constructor

        public AuthenticationMongoCollections(IMongoCollection<Client> clients,
            IMongoCollection<PersistedGrant> persistedGrants,
            IMongoCollection<ApiResource> apiResources,
            IMongoCollection<IdentityResource> identityResources)
        {
            Clients = clients;
            PersistedGrants = persistedGrants;
            ApiResources = apiResources;
            IdentityResources = identityResources;
        }

        #endregion

        #region Properties

        public IMongoCollection<Client> Clients { get; }

        public IMongoCollection<PersistedGrant> PersistedGrants { get; }

        public IMongoCollection<ApiResource> ApiResources { get; }

        public IMongoCollection<IdentityResource> IdentityResources { get; }

        #endregion
    }
}