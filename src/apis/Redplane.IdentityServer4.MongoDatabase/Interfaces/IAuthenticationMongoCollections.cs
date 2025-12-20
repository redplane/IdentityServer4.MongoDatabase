using IdentityServer4.Models;
using MongoDB.Driver;

namespace Redplane.IdentityServer4.MongoDatabase.Interfaces
{
    public interface IAuthenticationMongoCollections
    {
        IMongoCollection<Client> Clients { get; }

        IMongoCollection<PersistedGrant> PersistedGrants { get; }

        IMongoCollection<ApiResource> ApiResources { get; }

        IMongoCollection<IdentityResource> IdentityResources { get; }
    }
}