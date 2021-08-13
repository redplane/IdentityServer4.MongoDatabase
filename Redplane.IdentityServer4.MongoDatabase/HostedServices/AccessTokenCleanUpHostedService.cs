using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Models;

namespace Redplane.IdentityServer4.MongoDatabase.HostedServices
{
    internal class AccessTokenCleanUpHostedService : IHostedService
    {
        #region Constructor

        public AccessTokenCleanUpHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Internal methods

        protected virtual async Task CleanupExpiredAccessTokensAsync()
        {
            while (true)
                using (var serviceScope = _serviceProvider.CreateScope())
                {
                    var serviceProvider = serviceScope.ServiceProvider;
                    var adapterSettings = serviceProvider.GetService<AuthenticationAdapterSettings>();

                    // Resolve logging service.
                    var logger = serviceProvider.GetService<ILogger<AccessTokenCleanUpHostedService>>();
                    var persistedGrants = serviceProvider.GetService<IMongoCollection<PersistedGrant>>();

                    if (persistedGrants == null)
                    {
                        logger?.LogError(
                            $"There is no repository attached to {nameof(PersistedGrant)}. {nameof(AccessTokenCleanUpHostedService)} will be stopped.");
                        break;
                    }

                    // Clean up the expired persisted grant.
                    var persistedGrantFilterBuilder = Builders<PersistedGrant>.Filter;

                    // Find the expired persisted grants.
                    var expiredPersistedGrantFilterDefinition = persistedGrantFilterBuilder.Where(persistedGrant =>
                        persistedGrant.Expiration != null && persistedGrant.Expiration < DateTime.UtcNow);

                    // Remove all expired persisted grants.
                    await persistedGrants.DeleteManyAsync(expiredPersistedGrantFilterDefinition);

                    // Get current unix time.
                    var unixTime = DateTime.UtcNow;

                    // Set next job to be started.
                    var nextJobTime = adapterSettings.CleanupJobSchedule.GetNextOccurrence(unixTime);

                    // Calculate the difference between 2 dates.
                    var dateDifference = nextJobTime - unixTime;
                    await Task.Delay(dateDifference);
                }
        }

        #endregion

        #region Properties

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     Task which is for controlling job cancellation.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Methods

        /// <summary>
        ///     Called when service started.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Cancel previous task.
            _cancellationTokenSource?.Cancel();

            Task.Run(CleanupExpiredAccessTokensAsync, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when service stopped.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Cancel the job.
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            return Task.CompletedTask;
        }

        #endregion
    }
}