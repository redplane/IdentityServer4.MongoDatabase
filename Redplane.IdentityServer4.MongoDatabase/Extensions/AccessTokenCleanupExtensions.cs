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
        /// <param name="services"></param>
        /// <param name="scheduleExpression"></param>
        public static IServiceCollection AddExpiredAccessTokenCleaner(this IServiceCollection services,
            string scheduleExpression = default)
        {
            services.AddHostedService<AccessTokenCleanUpHostedService>();
            var authenticationAdapterSettings = new AuthenticationAdapterSettings();

            if (!string.IsNullOrWhiteSpace(scheduleExpression) &&
                CrontabSchedule.TryParse(scheduleExpression) != null)
            {
                authenticationAdapterSettings.UpdateSchedule(scheduleExpression, true);
                services.AddSingleton(authenticationAdapterSettings);
            }

            return services;
        }

        #endregion
    }
}