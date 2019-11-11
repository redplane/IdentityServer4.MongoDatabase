using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.MongoDbAdapter.Demo.Commands;
using IdentityServer4.MongoDbAdapter.Demo.Enums;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using IdentityServer4.MongoDbAdapter.Demo.Services.Interfaces;
using MediatR;

namespace IdentityServer4.MongoDbAdapter.Demo.CommandHandlers
{
    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, User>
    {
        private readonly IUserService _userService;

        public AddUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public Task<User> Handle(AddUserCommand command, CancellationToken cancellationToken)
        {
            return _userService.AddUserAsync(command.Username, command.Email, command.Password, null, command.Balance,
                command.FullName, command.AuthenticationProvider, UserStatuses.Active, command.Role, cancellationToken);
        }
    }
}