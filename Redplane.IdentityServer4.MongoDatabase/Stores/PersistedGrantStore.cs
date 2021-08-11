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

        public PersistedGrantStore(IAuthenticationDatabaseContext context)
        {
            _persistedGrants = context.GetPersistedGrants();
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

            return await ((IMongoQueryable<PersistedGrant>)persistedGrants).FirstOrDefaultAsync();
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            var filterBuilder = Builders<PersistedGrant>.Filter;
            var filterDefinitions = new LinkedList<FilterDefinition<PersistedGrant>>();

            // Filter is not defined. Returns everything.
            if (filter == null)
                return await _persistedGrants.Find(FilterDefinition<PersistedGrant>.Empty).ToListAsync();

            // Subject id is defined.
            var subjectId = filter.SubjectId?.Trim();

            if (!string.IsNullOrWhiteSpace(subjectId) && subjectId.Length > 0)
            {
                var subjectFilterDefinition = filterBuilder.Eq(x => x.SubjectId, subjectId);
                filterDefinitions.AddLast(subjectFilterDefinition);
            }

            // Session id is defined.
            var sessionId = filter.SessionId?.Trim();
            if (!string.IsNullOrWhiteSpace(sessionId) && sessionId.Length > 0)
            {
                var sessionIdFilterDefinition = filterBuilder.Eq(x => x.SessionId, sessionId);
                filterDefinitions.AddLast(sessionIdFilterDefinition);
            }

            // Client id is defined.
            var clientId = filter.ClientId?.Trim();
            if (!string.IsNullOrWhiteSpace(clientId) && clientId.Length > 0)
            {
                var clientIdFilterDefinition = filterBuilder.Eq(x => x.ClientId, clientId);
                filterDefinitions.AddLast(clientIdFilterDefinition);
            }

            // Type is defined.
            var type = filter.Type?.Trim();
            if (!string.IsNullOrWhiteSpace(type) && type.Length > 1)
            {
                var typeFilterDefinition = filterBuilder.Eq(x => x.Type, type);
                filterDefinitions.AddLast(typeFilterDefinition);
            }

            if (filterDefinitions.Count > 0)
                return await _persistedGrants.Find(filterBuilder.And(filterDefinitions.ToArray()))
                    .ToListAsync();

            return await _persistedGrants.Find(FilterDefinition<PersistedGrant>.Empty).ToListAsync();
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