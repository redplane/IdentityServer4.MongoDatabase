using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Constants.Scopes;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Services;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations
{
    public class AuthenticationDbService : IAuthenticationMongoDatabaseService
    {
        #region Constructor

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<List<Client>> LoadClientsAsync(CancellationToken cancellationToken = default)
        {
            var clients = new List<Client>();

            var resourceOwnerPasswordClient = new Client();
            resourceOwnerPasswordClient.ClientId = "sodakoq-app";
            resourceOwnerPasswordClient.AllowedGrantTypes = new List<string>();
            resourceOwnerPasswordClient.AllowedGrantTypes.Add(GrantType.ResourceOwnerPassword);
            resourceOwnerPasswordClient.ClientSecrets = new List<Secret>();
            resourceOwnerPasswordClient.ClientSecrets.Add(new Secret("7c0cf2bf-4a83-4077-9f0e-c7d6da8e23c0".Sha256()));
            resourceOwnerPasswordClient.AllowedScopes = new List<string>();
            resourceOwnerPasswordClient.AllowedScopes.Add(IdentityServerConstants.StandardScopes.Profile);
            resourceOwnerPasswordClient.AllowedScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
            resourceOwnerPasswordClient.AllowedScopes.Add(InvoiceScopes.Read);
            resourceOwnerPasswordClient.AllowedScopes.Add(InvoiceScopes.Pay);
            resourceOwnerPasswordClient.AllowAccessTokensViaBrowser = true;
            resourceOwnerPasswordClient.AllowedCorsOrigins = new List<string> { "http://localhost:4200" };
            resourceOwnerPasswordClient.AllowOfflineAccess = true;
            resourceOwnerPasswordClient.RefreshTokenExpiration = TokenExpiration.Sliding;
            resourceOwnerPasswordClient.RefreshTokenUsage = TokenUsage.ReUse;
            resourceOwnerPasswordClient.AccessTokenType = AccessTokenType.Jwt;
            resourceOwnerPasswordClient.AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds;
            resourceOwnerPasswordClient.SlidingRefreshTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds;
            clients.Add(resourceOwnerPasswordClient);

            return Task.FromResult(clients);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<List<ApiResource>> LoadApiResourcesAsync(CancellationToken cancellationToken = default)
        {
            var apiResources = new List<ApiResource>
            {
                new ApiResource("invoice", "Invoice API")
                {
                    Scopes = { InvoiceScopes.Read, InvoiceScopes.Pay, InvoiceScopes.Pay }
                },

                new ApiResource("customer", "Customer API")
                {
                    Scopes = { CustomerScopes.Read , CustomerScopes .Contact, CustomerScopes .Manage}
                }
            };

            return Task.FromResult(apiResources);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<List<IdentityResource>> LoadIdentityResourcesAsync(
            CancellationToken cancellationToken = default)
        {
            var identityResources = new List<IdentityResource>();
            return Task.FromResult(identityResources);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<List<ApiScope>> LoadApiScopesAsync(CancellationToken cancellationToken = default)
        {
            var apiScopes = new List<ApiScope>
            {
                // invoice API specific scopes
                new ApiScope(InvoiceScopes.Read, "Reads your invoices."),
                new ApiScope(InvoiceScopes.Pay, "Pays your invoices."),

                // customer API specific scopes
                new ApiScope(CustomerScopes.Read, "Reads you customers information."),
                new ApiScope(CustomerScopes.Contact, "Allows contacting one of your customers.")
            };

            return Task.FromResult(apiScopes);
        }

        #endregion
    }
}