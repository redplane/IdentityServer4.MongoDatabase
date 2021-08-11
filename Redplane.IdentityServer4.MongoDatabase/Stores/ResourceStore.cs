using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Stores
{
    public class ResourceStore : IResourceStore
    {
        #region Constructor

        public ResourceStore(IAuthenticationDatabaseContext context)
        {
            _identityResources = context.GetIdentityResources();
            _apiResources = context.GetApiResources();
            _apiScopes = context.GetApiScopes();
        }

        #endregion

        #region Properties

        private readonly IMongoCollection<IdentityResource> _identityResources;

        private readonly IMongoCollection<ApiResource> _apiResources;

        private readonly IMongoCollection<ApiScope> _apiScopes;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames)
        {
            var identityResources = _identityResources.AsQueryable();
            identityResources = identityResources.Where(x => scopeNames.Contains(x.Name));

            var loadedIdentityResources = await identityResources.ToListAsync();
            return loadedIdentityResources;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            List<ApiScope> loadedApiScopes = null;

            if (scopeNames == null)
            {
                loadedApiScopes = await _apiScopes.Find(FilterDefinition<ApiScope>.Empty)
                    .ToListAsync();

                return loadedApiScopes;
            }

            var loadedScopeNames
                = scopeNames as string[] ?? scopeNames.ToArray();

            var apiScopeFilterDefinition = Builders<ApiScope>.Filter.Where(x => loadedScopeNames.Contains(x.Name));
            loadedApiScopes = await _apiScopes
                .Find(apiScopeFilterDefinition)
                .ToListAsync();

            return loadedApiScopes;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames)
        {
            var apiResourceFilterBuilder = Builders<ApiResource>.Filter;
            var scopes = scopeNames as string[] ?? scopeNames.ToArray();

            var apiResourceFilterDefinition = apiResourceFilterBuilder.AnyIn(x => x.Scopes, scopes);
            var loadedApiResources = await _apiResources.Find(apiResourceFilterDefinition)
                .ToListAsync();

            return loadedApiResources;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="apiResourceNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(
            IEnumerable<string> apiResourceNames)
        {
            var apiResources = _apiResources.AsQueryable();
            apiResources = apiResources.Where(x => apiResourceNames.Contains(x.Name));
            return await apiResources.ToListAsync();
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            var apiResources = await _apiResources.AsQueryable().ToListAsync();
            var identityResources = await _identityResources.AsQueryable().ToListAsync();
            var apiScopes = await FindApiScopesByNameAsync(null);

            return new Resources(identityResources, apiResources, apiScopes);
        }

        #endregion
    }
}