using MediatR;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Commands
{
    public class AddUserCommand: IRequest<User>
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public decimal Balance { get; set; }

        public string FullName { get; set; }

        public AuthenticationProviders AuthenticationProvider { get; set; }

        public string Role { get; set; }

    }
}