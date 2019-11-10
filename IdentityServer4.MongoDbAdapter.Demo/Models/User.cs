using System;
using IdentityServer4.MongoDbAdapter.Demo.Enums;

namespace IdentityServer4.MongoDbAdapter.Demo.Models
{
    public class User
    {
        #region Constructor

        public User(Guid id, string username)
        {
            Id = id;
            Username = username;
        }

        #endregion

        #region Properties

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public Guid Id { get; private set; }

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
        /// User full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Authentication provider.
        /// </summary>
        public AuthenticationProviders AuthenticationProvider { get; set; }

        /// <summary>
        /// Status of user account.
        /// </summary>
        public UserStatuses Status { get; set; }

        /// <summary>
        ///     Name of role that is assigned to user.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// When the user joined in the system.
        /// </summary>
        public double JoinedTime { get; set; }

        /// <summary>
        /// When the last 
        /// </summary>
        public double? LastModifiedTime { get; set; }

        #endregion
    }
}