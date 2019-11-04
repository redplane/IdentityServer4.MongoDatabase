using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Mongo.Interfaces.Services
{
    public interface IAuthenticationMongoDatabaseService
    {
        #region Methods

        /// <summary>
        ///     Load default clients asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Client>> LoadClientsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Load default api resources async.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<ApiResource>> LoadApiResourcesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Load identity resources asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<IdentityResource>> LoadIdentityResourcesAsync(
            CancellationToken cancellationToken = default);

        #endregion
    }
}