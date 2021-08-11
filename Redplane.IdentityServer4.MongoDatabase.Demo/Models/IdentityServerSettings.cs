using IdentityServer4.Models;

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

        /// <summary>
        /// Name of database to read identity server configuration.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Built-in api resources.
        /// </summary>
        public ApiResource[] ApiResources { get; set; }

        /// <summary>
        /// Built-in api scopes.
        /// </summary>
        public ApiScope[] ApiScopes { get; set; }

        /// <summary>
        /// Built-in clients.
        /// </summary>
        public Client[] Clients { get; set; }

        /// <summary>
        /// Built-in identity resources.
        /// </summary>
        public IdentityResource[] IdentityResource { get; set; }

        #endregion
    }
}