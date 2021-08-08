using System;
using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Models
{
    public class AuthenticationDatabaseContext : IAuthenticationDatabaseContext
    {
        #region Constructor

        public AuthenticationDatabaseContext(
            string contextName,
            IMongoCollection<Client> clients, IMongoCollection<PersistedGrant> persistedGrants,
            IMongoCollection<ApiResource> apiResources, IMongoCollection<IdentityResource> identityResources,
            IMongoCollection<ApiScope> apiScopes,
            Func<IClientSessionHandle> sessionBeginner
        )
        {
            if (string.IsNullOrWhiteSpace(contextName))
                throw new Exception($"{nameof(contextName)} cannot be empty or null.");

            if (sessionBeginner == null)
                throw new Exception($"{nameof(sessionBeginner)} cannot be null.");

            Name = contextName;

            _clients = clients;
            _persistedGrants = persistedGrants;
            _apiResources = apiResources;
            _identityResources = identityResources;
            _apiScopes = apiScopes;

            _sessionBeginner = sessionBeginner;
        }

        #endregion

        #region Accessors

        public string Name { get; }

        #endregion

        #region Properties

        private readonly IMongoCollection<Client> _clients;

        private readonly IMongoCollection<PersistedGrant> _persistedGrants;

        private readonly IMongoCollection<ApiResource> _apiResources;

        private readonly IMongoCollection<IdentityResource> _identityResources;

        private readonly IMongoCollection<ApiScope> _apiScopes;

        private readonly Func<IClientSessionHandle> _sessionBeginner;

        #endregion

        #region Methods

        public virtual IMongoCollection<Client> GetClients()
        {
            return _clients;
        }

        public virtual IMongoCollection<PersistedGrant> GetPersistedGrants()
        {
            return _persistedGrants;
        }

        public virtual IMongoCollection<ApiResource> GetApiResources()
        {
            return _apiResources;
        }

        public virtual IMongoCollection<IdentityResource> GetIdentityResources()
        {
            return _identityResources;
        }

        public virtual IMongoCollection<ApiScope> GetApiScopes()
        {
            return _apiScopes;
        }

        public IClientSessionHandle StartSession()
        {
            return _sessionBeginner.Invoke();
        }

        #endregion
    }
}