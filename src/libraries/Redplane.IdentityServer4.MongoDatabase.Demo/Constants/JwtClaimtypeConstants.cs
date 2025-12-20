namespace Redplane.IdentityServer4.MongoDatabase.Demo.Constants
{
    public class JwtClaimTypeConstants
    {
        #region Properties

        /// <summary>
        ///     User id claim
        /// </summary>
        public const string Id = "userId";

        /// <summary>
        ///     Username.
        /// </summary>
        public const string Username = "username";

        /// <summary>
        ///     Email address.
        /// </summary>
        public const string Email = "email";

        /// <summary>
        ///     Birthday in UTC.
        /// </summary>
        public const string Birthday = "birthday";

        /// <summary>
        ///     Full name claim.
        /// </summary>
        public const string FullName = "fullName";

        public const string AuthenticationProvider = "authenticationProvider";

        public const string UserStatus = "userStatus";

        public const string Role = "role";

        public const string JoinedTime = "joinedTime";

        #endregion
    }
}