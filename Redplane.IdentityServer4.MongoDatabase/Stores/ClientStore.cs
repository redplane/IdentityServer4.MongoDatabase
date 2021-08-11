using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;
using Redplane.IdentityServer4.Cores.Models.Entities;

namespace Redplane.IdentityServer4.MongoDatabase.Stores
{
    public class ClientStore : IClientStore
    {
        #region Properties

        private readonly IMongoCollection<ApplicationClient> _clients;

        #endregion

        #region Constructors

        public ClientStore(IMongoCollection<ApplicationClient> clients)
        {
            _clients = clients;
        }

        #endregion

        #region Methods

        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            var conditions = new LinkedList<FilterDefinition<ApplicationClient>>();

            // Filter by id.
            conditions.AddLast(Builders<ApplicationClient>.Filter.Eq(x => x.ClientId, clientId));

            // Find the enabled one only.
            conditions.AddLast(Builders<ApplicationClient>.Filter.Eq(x => x.Enabled, true));

            var client = await _clients.Find(Builders<ApplicationClient>.Filter.And(conditions))
                .FirstOrDefaultAsync();
            return client.ToClient();
        }

        #endregion
    }
}