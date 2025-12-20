namespace Redplane.IdentityServer4.MongoDatabase.Constants
{
    internal class AuthenticationCollectionNameConstants
    {
        #region Properties

        /// <summary>
        ///     List of identity resources in the system.
        /// </summary>
        public const string IdentityResources = "identity-resources";

        /// <summary>
        ///     List of api resource in the system.
        /// </summary>
        public const string ApiResources = "api-resources";

        /// <summary>
        ///     List of clients in the system.
        /// </summary>
        public const string Clients = "clients";

        /// <summary>
        ///     List of persisted grants in the system.
        /// </summary>
        public const string PersistedGrants = "persisted-grants";

        #endregion
    }
}