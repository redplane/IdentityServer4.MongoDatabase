using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Redplane.IdentityServer4.MongoDatabase.Demo.Cqrs.Commands;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Demo.Services.Interfaces;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Cqrs.CommandHandlers
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