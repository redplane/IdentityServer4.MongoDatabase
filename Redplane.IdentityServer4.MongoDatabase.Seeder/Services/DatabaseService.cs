using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.Cores.Enums;
using Redplane.IdentityServer4.Cores.Models;
using Redplane.IdentityServer4.Cores.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Seeder.Constants.Scopes;

namespace Redplane.IdentityServer4.MongoDatabase.Seeder.Services
{
    public class DatabaseService
    {
        #region Constructor

        public DatabaseService(
            IMongoDatabase database,
            IMongoCollection<ApplicationClient> applicationClients,
            IMongoCollection<ApplicationApiResource> applicationApiResources,
            IMongoCollection<ApplicationIdentityResource> applicationIdentityResources,
            IMongoCollection<ApplicationApiScope> applicationApiScopes)
        {
            _database = database;
            _applicationClients = applicationClients;
            _applicationApiResources = applicationApiResources;
            _applicationIdentityResources = applicationIdentityResources;
            _applicationApiScopes = applicationApiScopes;
        }

        #endregion

        #region Methods

        public virtual Task SeedAsync()
        {
            using var session = _database.Client.StartSession();
            session.StartTransaction();

            _applicationClients.DeleteMany(FilterDefinition<ApplicationClient>.Empty);
            _applicationApiResources.DeleteMany(FilterDefinition<ApplicationApiResource>.Empty);
            _applicationIdentityResources.DeleteMany(FilterDefinition<ApplicationIdentityResource>.Empty);
            _applicationApiScopes.DeleteMany(FilterDefinition<ApplicationApiScope>.Empty);


            // Add client.
            var resourceOwnerPasswordClient = new ApplicationClient(Guid.NewGuid(), "IdentityServer4-DemoApp");
            resourceOwnerPasswordClient.AllowedGrantTypes = new[] { GrantType.ResourceOwnerPassword };
            resourceOwnerPasswordClient.Secrets = new[]
            {
                new ApplicationSecret
                {
                    Value = "7c0cf2bf-4a83-4077-9f0e-c7d6da8e23c0"
                }
            };
            resourceOwnerPasswordClient.AllowedScopes = new[]
            {
                IdentityServerConstants.StandardScopes.Profile, IdentityServerConstants.StandardScopes.OfflineAccess,
                InvoiceScopes.Read, InvoiceScopes.Pay
            };
            resourceOwnerPasswordClient.AllowAccessTokensViaBrowser = true;
            resourceOwnerPasswordClient.AllowedCorsOrigins = new[] { "http://localhost:4200" };
            resourceOwnerPasswordClient.AllowOfflineAccess = true;
            resourceOwnerPasswordClient.RefreshTokenExpiration = TokenExpiration.Sliding;
            resourceOwnerPasswordClient.RefreshTokenUsage = TokenUsage.ReUse;
            resourceOwnerPasswordClient.AccessTokenType = AccessTokenType.Jwt;
            resourceOwnerPasswordClient.AccessTokenLifetime = new Lifetime(30, TimeUnits.Day);
            resourceOwnerPasswordClient.SlidingRefreshTokenLifetime = new Lifetime(30, TimeUnits.Day);
            _applicationClients.InsertOne(resourceOwnerPasswordClient);
            
            // Api resource.
            var invoiceApiResource = new ApplicationApiResource(Guid.NewGuid(), "invoice");
            invoiceApiResource.DisplayName = "Invoice API";
            invoiceApiResource.Scopes = new[] { InvoiceScopes.Read, InvoiceScopes.Pay, InvoiceScopes.Pay };
            _applicationApiResources.InsertOne(invoiceApiResource);

            var customerApiResource = new ApplicationApiResource(Guid.NewGuid(), "customer");
            customerApiResource.DisplayName = "Customer API";
            customerApiResource.Scopes = new[] { CustomerScopes.Read, CustomerScopes.Contact, CustomerScopes.Manage };
            _applicationApiResources.InsertOne(customerApiResource);
            
            // Identity resource.
            var administrator = new ApplicationIdentityResource( Guid.NewGuid(),"administrator");
            administrator.DisplayName = "Administrator";
            administrator.UserClaims = new[] { "user.*", "post.*", "order.*" };
            _applicationIdentityResources.InsertOne(administrator);

            var profile = new IdentityResources.Profile();
            var irProfile = new ApplicationIdentityResource(Guid.NewGuid(), profile.Name);
            irProfile.DisplayName = profile.DisplayName;
            irProfile.UserClaims = profile.UserClaims;
            _applicationIdentityResources.InsertOne(irProfile);

            var openId = new IdentityResources.OpenId();
            var irOpenId = new ApplicationIdentityResource(Guid.NewGuid(), openId.Name);
            irOpenId.DisplayName = openId.DisplayName;
            irOpenId.UserClaims = openId.UserClaims;
            _applicationIdentityResources.InsertOne(irOpenId);
            
            // Api scope.
            _applicationApiScopes.InsertOne(new ApplicationApiScope(Guid.NewGuid(), InvoiceScopes.Read){Description = "Read your invoices"});
            _applicationApiScopes.InsertOne(new ApplicationApiScope(Guid.NewGuid(), InvoiceScopes.Pay){Description = "Pays your invoices"});
            _applicationApiScopes.InsertOne(new ApplicationApiScope(Guid.NewGuid(), CustomerScopes.Read){Description = "Reads you customers information."});
            _applicationApiScopes.InsertOne(new ApplicationApiScope(Guid.NewGuid(), CustomerScopes.Contact)
                { Description = "Allows contacting one of your customers." });

            // Commit the transaction.
            session.CommitTransaction();

            return Task.CompletedTask;
        }

        #endregion

        #region Properties

        private readonly IMongoDatabase _database;

        private readonly IMongoCollection<ApplicationClient> _applicationClients;

        private readonly IMongoCollection<ApplicationApiResource> _applicationApiResources;

        private readonly IMongoCollection<ApplicationIdentityResource> _applicationIdentityResources;

        private readonly IMongoCollection<ApplicationApiScope> _applicationApiScopes;

        #endregion
    }
}