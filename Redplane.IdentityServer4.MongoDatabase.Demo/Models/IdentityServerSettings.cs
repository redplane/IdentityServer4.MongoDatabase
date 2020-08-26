namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models
{
    public class IdentityServerSettings
    {
        #region Properties

        public string ClientId { get; set; }

        public string Authority { get; set; }

        public string ApiSecret { get; set; }

        /// <summary>
        ///     Whether password validation should be ignored or not.
        ///     This flag can be used in DEBUG mode only.
        /// </summary>
        public bool IgnorePasswordValidation { get; set; }

        public string ClientsCollectionName { get; set; }

        public string IdentityResourcesCollectionName { get; set; }

        public string ApiResourcesCollectionName { get; set; }

        public string PersistedGrantsCollectionName { get; set; }

        public string ApiScopesCollectionName { get; set; }

        public string DatabaseName { get; set; }

        #endregion
    }
}