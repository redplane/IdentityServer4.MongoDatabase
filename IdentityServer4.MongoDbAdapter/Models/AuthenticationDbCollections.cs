using IdentityServer4.Models;
using IdentityServer4.MongoDbAdapter.Interfaces;
using MongoDB.Driver;

namespace IdentityServer4.MongoDbAdapter.Models
{
    public class AuthenticationDbCollections : IAuthenticationMongoCollections
    {
        #region Constructor

        public AuthenticationDbCollections(IMongoCollection<Client> clients,
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