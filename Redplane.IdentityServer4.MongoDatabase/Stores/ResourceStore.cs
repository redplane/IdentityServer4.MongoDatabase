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

        public ResourceStore(IAuthenticationMongoContext context)
        {
            _identityResources = context.Collections.IdentityResources;
            _apiResources = context.Collections.ApiResources;
        }

        #endregion

        #region Properties

        private readonly IMongoCollection<IdentityResource> _identityResources;

        private readonly IMongoCollection<ApiResource> _apiResources;

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var identityResources = _identityResources.AsQueryable();
            identityResources = identityResources.Where(x => scopeNames.Contains(x.Name));

            return await identityResources.ToListAsync();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            var apiResourceFilterBuilder = Builders<ApiResource>.Filter;
            IList<ApiResource> loadedApiResources = null;

            if (scopeNames == null)
            {
                loadedApiResources = await _apiResources.Find(FilterDefinition<ApiResource>.Empty)
                   .ToListAsync();

                return loadedApiResources.SelectMany(apiResource =>
                    apiResource.Scopes.Select(apiScope => new ApiScope(apiScope, apiScope, apiResource.UserClaims)));
            }

            var loadedScopeNames
                = scopeNames as string[] ?? scopeNames.ToArray();
            var apiResourceFilterDefinition = apiResourceFilterBuilder.AnyIn(x => x.Scopes, loadedScopeNames);
            loadedApiResources = await _apiResources
                .Find(apiResourceFilterDefinition)
                .ToListAsync();

            return loadedApiResources.SelectMany(apiResource => apiResource.Scopes
                .Select(apiScope => new ApiScope(apiScope, apiScope, apiResource.UserClaims)));
        }

        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var apiResourceFilterBuilder = Builders<ApiResource>.Filter;
            var scopes = scopeNames as string[] ?? scopeNames.ToArray();

            var apiResourceFilterDefinition = apiResourceFilterBuilder.AnyIn(x => x.Scopes, scopes);
            return await _apiResources.Find(apiResourceFilterDefinition)
                .ToListAsync();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <param name="apiResourceNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
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