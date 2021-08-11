using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Demo.ViewModels;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Builders;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations.Builders
{
    public class UserDataBuilder : IAuthenticationDataBuilder
    {
        #region Properties

        private readonly IConfiguration _configuration;

        private readonly IMongoCollection<User> _users;

        #endregion


        #region Constructor

        public UserDataBuilder(IConfiguration configuration, IMongoCollection<User> users)
        {
            _configuration = configuration;
            _users = users;
        }

        #endregion

        #region Methods

        public virtual async Task BuildAsync(IAuthenticationDatabaseContext context,
            CancellationToken cancellationToken = default)
        {
            // Get the configurations.
            var builtInUsers = new LinkedList<UserViewModel>();
            _configuration.GetSection("Users").Bind(builtInUsers);

            if (builtInUsers.Count < 1)
                return;

            // Get the usernames
            var usernames = builtInUsers.Select(x => x.Username).ToArray();
            if (usernames.Length < 1)
                return;

            var userFilterDefinition = Builders<User>.Filter
                .In(x => x.Username, usernames);
            await _users.DeleteManyAsync(userFilterDefinition, cancellationToken);

            // Insert users.
            await _users.InsertManyAsync(builtInUsers.Select(x => x.ToUser()), null, cancellationToken);
        }

        #endregion
    }
}