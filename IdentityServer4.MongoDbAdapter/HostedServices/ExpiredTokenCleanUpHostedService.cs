using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.MongoDbAdapter.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NCrontab;

namespace IdentityServer4.MongoDbAdapter.HostedServices
{
    internal class ExpiredTokenCleanUpHostedService : IHostedService
    {
        #region Properties

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Whether service has been started or not.
        /// </summary>
        private bool _hasServiceStarted;

        /// <summary>
        /// Task which is for controlling job cancellation.
        /// </summary>
        private TaskCompletionSource<bool> _jobCancellationTaskCompletionSource;

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
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                _hasServiceStarted = false;
                _jobCancellationTaskCompletionSource?.TrySetCanceled();
            });

            // Mark the service to be stared.
            _hasServiceStarted = true;

            // Cancel the completion source.
            _jobCancellationTaskCompletionSource?.TrySetCanceled();

            // Initialize task cancellation controller.
            _jobCancellationTaskCompletionSource = new TaskCompletionSource<bool>();

            while (_hasServiceStarted)
            {
                using (var serviceScope = _serviceProvider.CreateScope())
                {
                    var serviceProvider = serviceScope.ServiceProvider;
                    var adapterSettings = serviceProvider.GetService<AuthenticationAdapterSettings>();

                    // Resolve logging service.
                    var logger = serviceProvider.GetService<ILogger<ExpiredTokenCleanUpHostedService>>();
                    
                    // Get the persisted grant collection.
                    var persistedGrants = _serviceProvider.GetService<IMongoCollection<PersistedGrant>>();

                    if (persistedGrants == null)
                    {
                        logger?.LogError($"There is no repository attached to {nameof(PersistedGrant)}. {nameof(ExpiredTokenCleanUpHostedService)} will be stopped." );
                        break;
                    }

                    // Clean up the expired persisted grant.
                    var persistedGrantFilterBuilder = Builders<PersistedGrant>.Filter;

                    // Find the expired persisted grants.
                    var expiredPersistedGrantFilterDefinition = persistedGrantFilterBuilder.Where(persistedGrant =>
                        persistedGrant.Expiration != null && persistedGrant.Expiration < DateTime.UtcNow);

                    // Remove all expired persisted grants.
                    persistedGrants.DeleteMany(expiredPersistedGrantFilterDefinition);

                    // Get current unix time.
                    var unixTime = DateTime.UtcNow;

                    // Set next job to be started.
                    var nextJobTime = adapterSettings.ParsedCronSchedule.GetNextOccurrence(unixTime);
                    var delayedTask = Task.Delay(nextJobTime - unixTime, cancellationToken);

                    await Task.WhenAny(delayedTask, _jobCancellationTaskCompletionSource.Task);
                }
            }
        }

        /// <summary>
        ///     Called when service stopped.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Cancel the job.
            _hasServiceStarted = false;
            _jobCancellationTaskCompletionSource?.TrySetCanceled();
            return Task.CompletedTask;
        }

        #endregion
    }
}