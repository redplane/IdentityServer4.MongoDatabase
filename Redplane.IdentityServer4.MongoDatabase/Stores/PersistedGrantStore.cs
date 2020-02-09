using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Bson;
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
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public virtual Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = _persistedGrants.AsQueryable();
            var result = Queryable.Where(persistedGrants, i => i.SubjectId == subjectId);
            return Task.FromResult(result.AsEnumerable());
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
        ///     Specify null to ignore the parameter.
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public virtual Task RemoveAllAsync(string subjectId, string clientId)
        {
            return RemoveAllAsync(subjectId, clientId, null);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="clientId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            return RemoveAllAsync(subjectId, clientId, type, false, CancellationToken.None);
        }

        /// <summary>
        ///     Remove all persisted grant asynchronously.
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="clientId"></param>
        /// <param name="type"></param>
        /// <param name="deleteExpiredGrants"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task RemoveAllAsync(string subjectId, string clientId, string type, bool deleteExpiredGrants,
            CancellationToken cancellationToken = default)
        {
            // Filter user definition & builder.
            var filterPersistedGrantBuilder = Builders<PersistedGrant>.Filter;
            var filterPersistedGrantDefinition = filterPersistedGrantBuilder.Empty;

            // Subject id is defined.
            if (!string.IsNullOrWhiteSpace(subjectId))
                filterPersistedGrantDefinition &=
                    filterPersistedGrantBuilder.Regex(x => x.SubjectId, new BsonRegularExpression($"^{subjectId}$"));

            // Client id is defined.
            if (!string.IsNullOrWhiteSpace(clientId))
                filterPersistedGrantDefinition &=
                    filterPersistedGrantBuilder.Regex(x => x.ClientId, new BsonRegularExpression($"^{clientId}$"));

            // Type is defined.
            if (!string.IsNullOrWhiteSpace(type))
                filterPersistedGrantDefinition &=
                    filterPersistedGrantBuilder.Regex(x => x.Type, new BsonRegularExpression($"^{type}$"));

            if (deleteExpiredGrants)
                filterPersistedGrantDefinition &= filterPersistedGrantBuilder.Lte(x => x.Expiration, DateTime.UtcNow);

            return _persistedGrants.DeleteManyAsync(filterPersistedGrantDefinition, null, cancellationToken);
        }

        #endregion
    }
}