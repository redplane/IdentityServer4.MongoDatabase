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
        ///     <inheritdoc />
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<ApiResource> FindApiResourceAsync(string name)
        {
            var apiResources = _apiResources.AsQueryable();
            var apiResource = await apiResources.Where(x => x.Name == name).FirstOrDefaultAsync();
            return apiResource;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var apiResources = _apiResources.AsQueryable();
            apiResources = apiResources.Where(x => scopeNames.Contains(x.Name));

            return Task.FromResult(apiResources.AsEnumerable());
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(
            IEnumerable<string> scopeNames)
        {
            var identityResources = _identityResources.AsQueryable();
            identityResources = identityResources.Where(x => scopeNames.Contains(x.Name));

            return Task.FromResult(identityResources.AsEnumerable());
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            var apiResources = await _apiResources.AsQueryable().ToListAsync();
            var identityResources = await _identityResources.AsQueryable().ToListAsync();

            return new Resources(identityResources, apiResources);
        }

        #endregion
    }
}