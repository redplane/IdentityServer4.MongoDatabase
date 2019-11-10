using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using IdentityServer4.MongoDbAdapter.Demo.Services.Interfaces;
using MongoDB.Driver;

namespace IdentityServer4.MongoDbAdapter.Demo.Services.Implementations
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
            var findUserFilterBuilder = Builders<User>.Filter;
            var findUserFilterDefinition = findUserFilterBuilder
                .And(
                    findUserFilterBuilder.Eq(x => x.Username, username),
                    findUserFilterBuilder.Eq(x => x.HashedPassword, password)
                );

            return _users.Find(findUserFilterDefinition)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion
    }
}