using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using Redplane.IdentityServer4.MongoDatabase.Demo.Constants;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models
{
    public class UserCredential
    {
        #region Properties

        /// <summary>
        ///     List of claims attached to the current credential.
        /// </summary>
        private readonly Claim[] _claims;

        #endregion

        #region Constructor

        public UserCredential(List<Claim> claims)
        {
            FromClaim(claims);
            _claims = claims.ToArray();
        }

        public UserCredential(IList<Claim> claims)
        {
            FromClaim(claims);
            _claims = claims.ToArray();
        }

        public UserCredential(User user)
        {
            var claims = new HashSet<Claim>();
            claims.Add(new Claim(JwtClaimTypes.Subject, user.Id.ToString("D")));
            claims.Add(new Claim(JwtClaimTypeConstants.Username, user.Username));
            claims.Add(new Claim(JwtClaimTypeConstants.Email, user.Email));
            
            if (user.Birthday != null)
                claims.Add(new Claim(JwtClaimTypeConstants.Birthday, user.Birthday.Value.ToString("yyyy-MM-dd")));

            claims.Add(new Claim(JwtClaimTypeConstants.FullName, user.FullName));
            claims.Add(new Claim(JwtClaimTypeConstants.Role, user.Role));
            claims.Add(new Claim(JwtClaimTypeConstants.AuthenticationProvider, user.AuthenticationProvider.ToString("D")));
            claims.Add(new Claim(JwtClaimTypeConstants.JoinedTime, user.JoinedTime.ToString("N")));

            _claims = claims.ToArray();
        }

        #endregion

        #region Accessors

        public Guid Id { get; private set; }

        public string Username { get; private set; }

        public string Email { get; private set; }

        public DateTime? Birthday { get; private set; }
        
        public string FullName { get; private set; }

        public string Role { get; private set; }

        public double JoinedTime { get; private set; }

        public double? LastModifiedTime => throw new NotImplementedException();

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///     Get user entity from credential.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public bool FromClaim(IList<Claim> claims)
        {
            var originalUserId =
                claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Subject)?.Value;

            // No id has been found.
            if (!Guid.TryParse(originalUserId, out var id))
                return false;

            // Find username.
            var username = claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.Username)?.Value;
            if (string.IsNullOrEmpty(username))
                return false;

            // Email
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.Email)?.Value;

            // Birthday
            var originalBirthday = claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.Birthday)?.Value;
            if (string.IsNullOrEmpty(originalBirthday))
                return false;

            // Birthday cannot be parsed.
            if (!DateTime.TryParse(originalBirthday, out var birthday))
                return false;

            // Full name.
            var fullName = claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.FullName)?.Value;
            if (string.IsNullOrEmpty(fullName))
                return false;

            // Role name.
            var role = claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                return false;

            // Authentication provider.
            var originalAuthenticationProvider =
                claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.AuthenticationProvider)?.Value;
            if (string.IsNullOrEmpty(originalAuthenticationProvider))
                return false;

            double.TryParse(claims.FirstOrDefault(x => x.Type == JwtClaimTypeConstants.JoinedTime)?.Value,
                out var joinedTime);

            Id = id;
            Username = username;
            Email = email;
            Birthday = birthday;
            FullName = fullName;
            Role = role;
            JoinedTime = joinedTime;

            return true;
        }

        /// <summary>
        ///     Convert credential to claims.
        /// </summary>
        /// <returns></returns>
        public Claim[] ToClaims()
        {
            return _claims;
        }

        /// <summary>
        ///     Check whether user is in a role or not.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool IsInRole(string roleName)
        {
            return string.Equals(Role, roleName, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}