using System;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities
{
    public class User : DefaultEntity
    {
        #region Constructor

        public User(Guid id, string username) : base(id)
        {
            Username = username;
        }

        #endregion

        #region Properties

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string Username { get; private set; }

        public string Email { get; set; }

        public string HashedPassword { get; set; }

        public DateTime? Birthday { get; set; }

        /// <summary>
        ///     How much money user is having.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        ///     User full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        ///     Authentication provider.
        /// </summary>
        public AuthenticationProviders AuthenticationProvider { get; set; }

        /// <summary>
        ///     Status of user account.
        /// </summary>
        public UserStatuses Status { get; set; }

        /// <summary>
        ///     Name of role that is assigned to user.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        ///     When the user joined in the system.
        /// </summary>
        public double JoinedTime { get; set; }

        #endregion
    }
}