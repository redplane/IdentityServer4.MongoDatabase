namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models
{
    public class IdentityServerSettings
    {
        #region Properties

        public string ClientId { get; set; } = string.Empty;

        public string Authority { get; set; } = string.Empty;

        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>
        ///     Whether password validation should be ignored or not.
        ///     This flag can be used in DEBUG mode only.
        /// </summary>
        public bool IgnorePasswordValidation { get; set; }

        public string ClientsCollectionName { get; set; } = string.Empty;

        public string IdentityResourcesCollectionName { get; set; } = string.Empty;

        public string ApiResourcesCollectionName { get; set; } = string.Empty;

        public string PersistedGrantsCollectionName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        #endregion
    }
}