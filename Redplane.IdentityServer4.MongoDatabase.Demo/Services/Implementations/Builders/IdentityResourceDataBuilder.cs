using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Builders;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Services;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations.Builders
{
    public class IdentityResourceDataBuilder : IAuthenticationDataBuilder
    {
        #region Properties

        private readonly IAuthenticationDatabaseService _authenticationDatabaseService;

        #endregion

        #region Constructor

        public IdentityResourceDataBuilder(IAuthenticationDatabaseService authenticationDatabaseService)
        {
            _authenticationDatabaseService = authenticationDatabaseService;
        }

        #endregion

        #region Methods

        public virtual async Task BuildAsync(IAuthenticationDatabaseContext context,
            CancellationToken cancellationToken = default)
        {
            if (_authenticationDatabaseService == null)
                return;

            // Get the built in identity resources.
            var builtInIdentityResources = await _authenticationDatabaseService
                .LoadIdentityResourcesAsync(cancellationToken);

            if (builtInIdentityResources == null || builtInIdentityResources.Count < 1)
                return;

            builtInIdentityResources = builtInIdentityResources
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToList();

            if (builtInIdentityResources.Count < 1)
                return;

            var builtInIdentityResourceNames = builtInIdentityResources
                .Select(x => x.Name)
                .ToArray();

            // Get the identity resource collection.
            var identityResources = context.GetIdentityResources();

            // Delete all previous identity resources.
            var identityResourceFilterDefinition = Builders<IdentityResource>.Filter
                .In(x => x.Name, builtInIdentityResourceNames);
            await identityResources.DeleteManyAsync(identityResourceFilterDefinition, cancellationToken);

            // Insert new records.
            await identityResources.InsertManyAsync(builtInIdentityResources, null, cancellationToken);
        }

        #endregion
    }
}