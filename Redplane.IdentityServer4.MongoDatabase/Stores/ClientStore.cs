﻿using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Stores
{
    public class ClientStore : IClientStore
    {
        #region Properties

        private readonly IMongoCollection<Client> _clients;

        #endregion

        #region Constructors

        public ClientStore(IAuthenticationMongoContext context)
        {
            _clients = context.Collections.Clients;
        }

        #endregion

        #region Methods

        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            IQueryable<Client> clients = _clients.AsQueryable();
            clients = clients.Where(x => x.ClientId == clientId);

            var client = await ((IMongoQueryable<Client>) clients).FirstOrDefaultAsync();
            return client;
        }

        #endregion
    }
}