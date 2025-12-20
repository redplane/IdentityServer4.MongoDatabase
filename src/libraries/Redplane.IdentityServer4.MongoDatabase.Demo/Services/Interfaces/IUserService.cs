using System;
using System.Threading;
using System.Threading.Tasks;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Interfaces
{
    public interface IUserService
    {
        #region Methods

        /// <summary>
        /// Do basic login asynchronously.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="shouldPasswordIgnored"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<User> BasicLoginAsync(string username, string password, bool shouldPasswordIgnored, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add an user into system asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<User> AddUserAsync(string username, string email, string password, DateTime? birthday, 
            decimal balance, string fullName, AuthenticationProviders authenticationProvider, 
            UserStatuses status, string role, CancellationToken cancellationToken = default);

        #endregion
    }
}