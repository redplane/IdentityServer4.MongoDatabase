using IdentityServer4.Models;
using MongoDB.Driver;

namespace IdentityServer4.Mongo.Interfaces
{
    public interface IAuthenticationMongoCollections
    {
        IMongoCollection<Client> Clients { get; }

        IMongoCollection<PersistedGrant> PersistedGrants { get; }

        IMongoCollection<ApiResource> ApiResources { get; }

        IMongoCollection<IdentityResource> IdentityResources { get; }
    }
}