﻿using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;
using Redplane.IdentityServer4.MongoDatabase.Models;

namespace Redplane.IdentityServer4.MongoDatabase.HostedServices
{
    internal class ExpiredTokenCleanUpHostedService : IHostedService
    {
        #region Properties

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Task which is for controlling job cancellation.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        private Task _expiredAccessTokenCleanerTask;

        #endregion

        #region Constructor

        public ExpiredTokenCleanUpHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

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

            _expiredAccessTokenCleanerTask = Task.Run(CleanupExpiredAccessTokensAsync, cancellationToken);
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

        #region Internal methods

        protected virtual async Task CleanupExpiredAccessTokensAsync()
        {
            while (true)
            {
                using (var serviceScope = _serviceProvider.CreateScope())
                {
                    var serviceProvider = serviceScope.ServiceProvider;
                    var adapterSettings = serviceProvider.GetService<AuthenticationAdapterSettings>();

                    // Resolve logging service.
                    var logger = serviceProvider.GetService<ILogger<ExpiredTokenCleanUpHostedService>>();

                    // Get the persisted grant collection.
                    var authenticationMongoContext = serviceProvider.GetService<IAuthenticationMongoContext>();

                    if (authenticationMongoContext == null)
                    {
                        logger?.LogError($"There is no repository attached to {nameof(PersistedGrant)}. {nameof(ExpiredTokenCleanUpHostedService)} will be stopped.");
                        break;
                    }

                    // Clean up the expired persisted grant.
                    var persistedGrantFilterBuilder = Builders<PersistedGrant>.Filter;

                    // Find the expired persisted grants.
                    var expiredPersistedGrantFilterDefinition = persistedGrantFilterBuilder.Where(persistedGrant =>
                        persistedGrant.Expiration != null && persistedGrant.Expiration < DateTime.UtcNow);

                    // Remove all expired persisted grants.
                    authenticationMongoContext.Collections
                        .PersistedGrants
                        .DeleteMany(expiredPersistedGrantFilterDefinition);

                    // Get current unix time.
                    var unixTime = DateTime.UtcNow;

                    // Set next job to be started.
                    var nextJobTime = adapterSettings.CleanupJobSchedule.GetNextOccurrence(unixTime);

                    // Calculate the difference between 2 dates.
                    var dateDifference = nextJobTime - unixTime;
                    await Task.Delay(dateDifference);
                }
            }
        }

        #endregion
    }
}