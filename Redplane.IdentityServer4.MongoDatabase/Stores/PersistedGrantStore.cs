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
    public class PersistedGrantStore : IPersistedGrantStore
    {
        #region Properties

        private readonly IMongoCollection<PersistedGrant> _persistedGrants;

        #endregion

        #region Constructors

        public PersistedGrantStore(IAuthenticationMongoContext context)
        {
            _persistedGrants = context.Collections.PersistedGrants;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="grant"></param>
        /// <returns></returns>
        public virtual async Task StoreAsync(PersistedGrant grant)
        {
            await _persistedGrants.InsertOneAsync(grant);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            IQueryable<PersistedGrant> persistedGrants = _persistedGrants.AsQueryable();
            persistedGrants = persistedGrants.Where(i => i.Key == key);

            return await ((IMongoQueryable<PersistedGrant>) persistedGrants).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            var persistedGrants = _persistedGrants.AsQueryable();

            // Filter is not defined. Returns everything.
            if (filter == null)
                return await persistedGrants.ToListAsync();

            // Subject id is defined.
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
                persistedGrants = persistedGrants.Where(x => x.SubjectId == filter.SubjectId);

            // Session id is defined.
            if (!string.IsNullOrWhiteSpace(filter.SessionId))
                persistedGrants = persistedGrants.Where(x => x.SessionId == filter.SessionId);

            // Client id is defined.
            if (!string.IsNullOrWhiteSpace(filter.ClientId))
                persistedGrants = persistedGrants.Where(x => x.ClientId == filter.ClientId);

            if (!string.IsNullOrWhiteSpace(filter.Type))
                persistedGrants = persistedGrants.Where(x => x.Type == filter.Type);

            return await persistedGrants.ToListAsync();
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual Task RemoveAsync(string key)
        {
            var persistedGrantFilterDefinition = Builders<PersistedGrant>
                .Filter
                .Where(x => x.Key.ToLower() == key.ToLower());

            return _persistedGrants.DeleteManyAsync(persistedGrantFilterDefinition);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            var persistedGrantFilterDefinitions = new LinkedList<FilterDefinition<PersistedGrant>>();

            // If the filter is null, remove every thing.
            if (filter == null)
            {
                await _persistedGrants.DeleteManyAsync(FilterDefinition<PersistedGrant>.Empty);
                return;
            }

            // Subject id is defined.
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                var subjectIdFilterDefinition = Builders<PersistedGrant>.Filter.Eq(x => x.SubjectId, filter.SubjectId);
                persistedGrantFilterDefinitions.AddLast(subjectIdFilterDefinition);
            }

            // Session id is defined.
            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                var sessionIdFilterDefinition = Builders<PersistedGrant>.Filter.Eq(x => x.SessionId, filter.SessionId);
                persistedGrantFilterDefinitions.AddLast(sessionIdFilterDefinition);
            }

            // Client id is defined.
            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                var clientIdFilterDefinition = Builders<PersistedGrant>.Filter.Eq(x => x.SessionId, filter.SessionId);
                persistedGrantFilterDefinitions.AddLast(clientIdFilterDefinition);
            }

            // Type is defined.
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                var typeFilterDefinition = Builders<PersistedGrant>.Filter.Eq(x => x.Type, filter.Type);
                persistedGrantFilterDefinitions.AddLast(typeFilterDefinition);
            }

            if (persistedGrantFilterDefinitions.Count < 1)
            {
                await _persistedGrants.DeleteManyAsync(FilterDefinition<PersistedGrant>.Empty);
                return;
            }

            await _persistedGrants.DeleteManyAsync(
                Builders<PersistedGrant>.Filter.And(persistedGrantFilterDefinitions));
        }

        #endregion
    }
}