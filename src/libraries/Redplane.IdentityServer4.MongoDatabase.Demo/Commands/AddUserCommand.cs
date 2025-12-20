using MediatR;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Commands
{
    public class CreateUserCommand: IRequest<User>
    {
        public required string Username { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public decimal Balance { get; set; }

        public required string FullName { get; set; }

        public AuthenticationProviders AuthenticationProvider { get; set; }

        public required string Role { get; set; }

    }
}