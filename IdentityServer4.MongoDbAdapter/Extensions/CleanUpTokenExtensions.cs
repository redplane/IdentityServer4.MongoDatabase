using IdentityServer4.Mongo.Stores;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Mongo.Extensions
{
    public static class CleanUpTokenExtensions
    {
        #region Methods

        /// <summary>
        ///     Remove all expired identity token
        /// </summary>
        /// <param name="applicationBuilder"></param>
        public static void RemoveExpiredIdentityToken(this IApplicationBuilder applicationBuilder)
        {
            // Run mail template configuration.
            applicationBuilder.Use(async (handler, next) =>
            {
                var basePersistedGrantStore = handler.RequestServices.GetService<IPersistedGrantStore>();
                if (!(basePersistedGrantStore is PersistedGrantStore persistedGrantStore))
                    return;

                await persistedGrantStore.RemoveAllAsync(null, null, null, true);
                await next.Invoke();
            });
        }

        #endregion
    }
}