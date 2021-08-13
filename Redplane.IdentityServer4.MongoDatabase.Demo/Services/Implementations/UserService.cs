using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Constants;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;
using Redplane.IdentityServer4.MongoDatabase.Demo.Extensions;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Exceptions;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Interfaces;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations
{
    public class UserService : IUserService
    {
        #region Properties

        private readonly IMongoCollection<User> _users;

        #endregion

        #region Constructors

        public UserService(IMongoCollection<User> users)
        {
            _users = users;
        }

        #endregion

        #region Methods

        public Task<User> BasicLoginAsync(string username, string password, bool shouldPasswordIgnored,
            CancellationToken cancellationToken = default)
        {
            // Make username to be lower cased.
            var lowerCasedUsername = username.ToLower();
            var hashedPassword = password.CalculateHash();

            var findUserFilterBuilder = Builders<User>.Filter;
            var findUserFilterDefinition = findUserFilterBuilder
                .And(
                    findUserFilterBuilder.Eq(x => x.Username, lowerCasedUsername),
                    findUserFilterBuilder.Eq(x => x.HashedPassword, hashedPassword)
                );

            return _users.Find(findUserFilterDefinition)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public async Task<User> AddUserAsync(string username, string email, string password, DateTime? birthday,
            decimal balance,
            string fullName, AuthenticationProviders authenticationProvider, UserStatuses status, string role,
            CancellationToken cancellationToken = default)
        {
            // Calculate hashed password.
            try
            {
                var user = new User(Guid.NewGuid(), username);
                user.Email = email;
                user.HashedPassword = password.CalculateHash();
                user.Birthday = birthday;
                user.Balance = balance;
                user.FullName = fullName;
                user.AuthenticationProvider = authenticationProvider;
                user.Status = status;
                user.Role = role;
                user.JoinedTime = DateTime.UtcNow.ToUnixTime();

                await _users.InsertOneAsync(user, null, cancellationToken);
                return user;
            }
            catch (Exception exception)
            {
                if (exception is MongoWriteException mongoWriteException)
                    if (mongoWriteException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                        throw new HttpResponseException(HttpStatusCode.Conflict,
                            HttpMessageCodeConstants.UserDuplicated);

                throw;
            }
        }

        #endregion
    }
}