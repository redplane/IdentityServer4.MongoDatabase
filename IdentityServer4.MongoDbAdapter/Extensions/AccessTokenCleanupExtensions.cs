using IdentityServer4.MongoDbAdapter.HostedServices;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.MongoDbAdapter.Extensions
{
    public static class AccessTokenCleanupExtensions
    {
        #region Methods

        /// <summary>
        ///     Remove all expired identity token
        /// </summary>
        /// <param name="identityServerBuilder"></param>
        public static IIdentityServerBuilder AddExpiredAccessTokenCleaner(this IIdentityServerBuilder identityServerBuilder)
        {
            identityServerBuilder.Services.AddHostedService<ExpiredTokenCleanUpHostedService>();
            return identityServerBuilder;
        }

        #endregion
    }
}