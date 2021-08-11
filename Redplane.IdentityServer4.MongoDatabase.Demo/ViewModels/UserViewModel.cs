using System;
using Newtonsoft.Json;
using Redplane.IdentityServer4.MongoDatabase.Demo.Enums;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.ViewModels
{
    public class UserViewModel
    {
        #region Properties

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public Guid Id { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string Username { get; set; }

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


        public User ToUser()
        {
            var user = new User(Id, Username);
            user.Email = Email;
            user.HashedPassword = HashedPassword;
            user.Birthday = Birthday;
            user.Balance = Balance;
            user.FullName = FullName;
            user.AuthenticationProvider = AuthenticationProvider;
            user.Status = Status;
            user.Role = Role;
            user.JoinedTime = JoinedTime;
            user.LastModifiedTime = LastModifiedTime;

            return user;
        }
    }
}