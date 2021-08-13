using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Redplane.IdentityServer4.Cores.Models.Entities;

namespace Redplane.IdentityServer4.MongoDatabase.Stores
{
    public class ResourceStore : IResourceStore
    {
        #region Constructor

        public ResourceStore(IMongoCollection<ApplicationIdentityResource> applicationApplicationIdentityResources,
            IMongoCollection<ApplicationApiResource> applicationApplicationApiResource,
            IMongoCollection<ApplicationApiScope> applicationApplicationApiScope)
        {
            _applicationIdentityResources = applicationApplicationIdentityResources;
            _applicationApiResources = applicationApplicationApiResource;
            _applicationApiScopes = applicationApplicationApiScope;
        }

        #endregion

        #region Properties

        private readonly IMongoCollection<ApplicationIdentityResource> _applicationIdentityResources;

        private readonly IMongoCollection<ApplicationApiResource> _applicationApiResources;

        private readonly IMongoCollection<ApplicationApiScope> _applicationApiScopes;

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames)
        {
            var identityResources = _applicationIdentityResources.AsQueryable();
            identityResources = identityResources.Where(x => scopeNames.Contains(x.Name));

            var loadedIdentityResources = await identityResources.ToListAsync();
            return loadedIdentityResources.Select(x => x.ToIdentityResource());
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            IEnumerable<ApplicationApiScope> applicationApiScopes = null;

            if (scopeNames == null)
            {
                applicationApiScopes = await _applicationApiScopes.Find(FilterDefinition<ApplicationApiScope>.Empty)
                    .ToListAsync();

                return applicationApiScopes.Select(x => x.ToApiScope());
            }

            var apiScopeFilterDefinition = Builders<ApplicationApiScope>.Filter.Where(x => scopeNames.Contains(x.Name));
            applicationApiScopes = await _applicationApiScopes
                .Find(apiScopeFilterDefinition)
                .ToListAsync();

            return applicationApiScopes.Select(x => x.ToApiScope());
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(
            IEnumerable<string> scopeNames)
        {
            IEnumerable<ApplicationApiResource> applicationApiResources;

            if (scopeNames == null)
            {
                applicationApiResources = await _applicationApiResources
                    .Find(FilterDefinition<ApplicationApiResource>.Empty)
                    .ToListAsync();
            }
            else
            {
                var apiResourceFilterBuilder = Builders<ApplicationApiResource>.Filter;
                var apiResourceFilterDefinition = apiResourceFilterBuilder.AnyIn(x => x.Scopes, scopeNames);
                applicationApiResources = await _applicationApiResources.Find(apiResourceFilterDefinition)
                    .ToListAsync();
            }

            return applicationApiResources.Select(x => x.ToApiResource());
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="apiResourceNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(
            IEnumerable<string> apiResourceNames)
        {
            var applicationApiResources = await _applicationApiResources.AsQueryable()
                .Where(x => apiResourceNames.Contains(x.Name))
                .ToListAsync();

            return applicationApiResources.Select(x => x.ToApiResource());
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            var applicationApiResources = await _applicationApiResources.AsQueryable().ToListAsync();
            var applicationIdentityResources = await _applicationIdentityResources.AsQueryable().ToListAsync();
            var applicationApiScopes = await _applicationApiScopes.AsQueryable().ToListAsync();

            var apiResources = applicationApiResources.Select(x => x.ToApiResource());
            var identityResources = applicationIdentityResources.Select(x => x.ToIdentityResource());
            var apiScopes = applicationApiScopes.Select(x => x.ToApiScope());

            return new Resources(identityResources, apiResources, apiScopes);
        }

        #endregion
    }
}