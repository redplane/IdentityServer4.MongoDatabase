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
using Redplane.IdentityServer4.MongoDatabase.Seeder.Services.Interfaces;

namespace Redplane.IdentityServer4.MongoDatabase.Seeder.Services
{
    public class DatabaseService : IDatabaseService
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
                "invoice"
            };
            resourceOwnerPasswordClient.AllowAccessTokensViaBrowser = true;
            resourceOwnerPasswordClient.AllowedCorsOrigins = new[] { "http://localhost:4200" };
            resourceOwnerPasswordClient.AllowOfflineAccess = true;
            resourceOwnerPasswordClient.RefreshTokenExpiration = TokenExpiration.Sliding;
            resourceOwnerPasswordClient.RefreshTokenUsage = TokenUsage.ReUse;
            resourceOwnerPasswordClient.AccessTokenType = AccessTokenType.Reference;
            resourceOwnerPasswordClient.AccessTokenLifetime = new Lifetime(30, TimeUnits.Day);
            resourceOwnerPasswordClient.SlidingRefreshTokenLifetime = new Lifetime(30, TimeUnits.Day);
            _applicationClients.InsertOne(resourceOwnerPasswordClient);
            
            // Add client.
            var smartSensorClient = new ApplicationClient(Guid.NewGuid(), "smart-sensor");
            smartSensorClient.AllowedGrantTypes = new[] { GrantType.ResourceOwnerPassword };
            smartSensorClient.Secrets = new[]
            {
                new ApplicationSecret
                {
                    Value = "7c0cf2bf-4a83-4077-9f0e-c7d6da8e23c0"
                }
            };
            smartSensorClient.AllowedScopes = new[]
            {
                IdentityServerConstants.StandardScopes.Profile, IdentityServerConstants.StandardScopes.OfflineAccess, "smart-sensor-api"
            };
            smartSensorClient.AllowAccessTokensViaBrowser = true;
            smartSensorClient.AllowedCorsOrigins = new[] { "http://localhost:4200" };
            smartSensorClient.AllowOfflineAccess = true;
            smartSensorClient.RefreshTokenExpiration = TokenExpiration.Sliding;
            smartSensorClient.RefreshTokenUsage = TokenUsage.ReUse;
            smartSensorClient.AccessTokenType = AccessTokenType.Reference;
            smartSensorClient.AccessTokenLifetime = new Lifetime(30, TimeUnits.Day);
            smartSensorClient.SlidingRefreshTokenLifetime = new Lifetime(30, TimeUnits.Day);
            _applicationClients.InsertOne(smartSensorClient);

            // Api resource.
            var invoiceApiResource = new ApplicationApiResource(Guid.NewGuid(), "invoice");
            invoiceApiResource.DisplayName = "Invoice API";
            invoiceApiResource.Scopes = new[] { "invoice", InvoiceScopes.Read, InvoiceScopes.Pay };
            invoiceApiResource.UserClaims = new List<string> { "email", "fullName" };
            invoiceApiResource.ApiSecrets = new[]
            {
                new ApplicationSecret
                {
                    Value = "7c0cf2bf-4a83-4077-9f0e-c7d6da8e23c0"
                }
            };
            _applicationApiResources.InsertOne(invoiceApiResource);

            var smartSensorApiResource = new ApplicationApiResource(Guid.NewGuid(), "smart-sensor");
            smartSensorApiResource.DisplayName = "Smart Sensor API";
            smartSensorApiResource.Scopes = new[] { "smart-sensor-api"};
            smartSensorApiResource.ApiSecrets = new[]
            {
                new ApplicationSecret
                {
                    Value = "7c0cf2bf-4a83-4077-9f0e-c7d6da8e23c0"
                }
            };
            _applicationApiResources.InsertOne(smartSensorApiResource);
            
            // Identity resource.
            var profile = new IdentityResources.Profile();
            var irProfile = new ApplicationIdentityResource(profile);
            _applicationIdentityResources.InsertOne(irProfile);

            // Api scope.
            _applicationApiScopes.InsertOne(new ApplicationApiScope(Guid.NewGuid(), "invoice"));
            _applicationApiScopes.InsertOne(new ApplicationApiScope(Guid.NewGuid(), "smart-sensor-api")
                { Description = "Process your invoices" });
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