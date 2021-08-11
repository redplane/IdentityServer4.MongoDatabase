using Microsoft.Extensions.DependencyInjection;
using NCrontab;
using Redplane.IdentityServer4.MongoDatabase.HostedServices;
using Redplane.IdentityServer4.MongoDatabase.Models;

namespace Redplane.IdentityServer4.MongoDatabase.Extensions
{
    public static class AccessTokenCleanupExtensions
    {
        #region Methods

        /// <summary>
        ///     Remove all expired identity token
        /// </summary>
        /// <param name="identityServerBuilder"></param>
        /// <param name="accessTokenCleanerCronJob"></param>
        public static IIdentityServerBuilder AddExpiredAccessTokenCleaner(
            this IIdentityServerBuilder identityServerBuilder, string accessTokenCleanerCronJob = default)
        {
            identityServerBuilder.Services.AddHostedService<ExpiredTokenCleanUpHostedService>();

            var authenticationAdapterSettings = new AuthenticationAdapterSettings();

            if (!string.IsNullOrWhiteSpace(accessTokenCleanerCronJob) &&
                CrontabSchedule.TryParse(accessTokenCleanerCronJob) != null)
                authenticationAdapterSettings.UpdateAccessTokenCleanupCronJob(accessTokenCleanerCronJob, true);
            identityServerBuilder.Services.AddSingleton(authenticationAdapterSettings);

            return identityServerBuilder;
        }

        #endregion
    }
}