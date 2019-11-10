using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.MongoDbAdapter.Demo.Models;

namespace IdentityServer4.MongoDbAdapter.Demo.Services.Interfaces
{
    public interface IUserService
    {
        #region Methods

        Task<User> BasicLoginAsync(string username, string password, bool shouldPasswordIgnored, CancellationToken cancellationToken = default);

        #endregion
    }
}