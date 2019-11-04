using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Mongo.Interfaces.Contexts;
using IdentityServer4.Mongo.Interfaces.Services;
using IdentityServer4.Mongo.Models;
using IdentityServer4.Mongo.Stores;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace IdentityServer4.Mongo.Setups
{
    public static class AuthenticationMongoDatabaseSetup
    {
        #region Methods

        /// <summary>
        ///     Add integration part in mongo database.
        /// </summary>
        public static IIdentityServerBuilder AddIdentityServerMongoIntegration(
            this IIdentityServerBuilder identityServerBuilder, string connectionString,
            string databaseName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception($"{nameof(connectionString)} cannot be blank.");

            if (string.IsNullOrEmpty(databaseName))
                throw new Exception($"{nameof(databaseName)} cannot be blank.");

            // Get services collection.
            var services = identityServerBuilder.Services;

            // Register identity stores (factories)
            services.AddScoped<IClientStore, ClientStore>();
            services.AddScoped<IResourceStore, ResourceStore>();
            services.AddScoped<IPersistedGrantStore, PersistedGrantStore>();

            BsonClassMap.RegisterClassMap<IdentityResource>(options =>
            {
                options.SetIgnoreExtraElements(true);
                options.MapCreator(x => new IdentityResource());
            });

            BsonClassMap.RegisterClassMap<Client>(options =>
            {
                options.AutoMap();
                options.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<ApiResource>(options =>
            {
                options.AutoMap();
                options.SetIgnoreExtraElements(true);
                options.MapCreator(x => new ApiResource());
            });

            BsonClassMap.RegisterClassMap<PersistedGrant>(options =>
            {
                options.AutoMap();
                options.SetIgnoreExtraElements(true);
            });


            // Register authentication mongo database.
            services.AddScoped<IAuthenticationMongoContext>(options =>
            {
                // Get mongo client.
                var mongoClient = new MongoClient(new MongoUrl(connectionString));
                var authenticationMongoContext = new AuthenticationMongoContext(mongoClient, databaseName);

                // Register standard collections.
                return authenticationMongoContext;
            });

            return identityServerBuilder;
        }

        /// <summary>
        ///     Add identity server authentication.
        /// </summary>
        /// <param name="identityServerBuilder"></param>
        public static IIdentityServerBuilder AddIdentityServerMongoSeedService<T>(
            this IIdentityServerBuilder identityServerBuilder) where T : IAuthenticationMongoDatabaseService
        {
            var services = identityServerBuilder.Services;
            services.AddScoped(typeof(IAuthenticationMongoDatabaseService), typeof(T));
            return identityServerBuilder;
        }

        /// <summary>
        ///     Use authentication database seed.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="logger"></param>
        /// <param name="resetDatabase"></param>
        public static void UseAuthenticationDatabaseSeed(this IApplicationBuilder app, ILogger logger,
            bool resetDatabase = false)
        {
            // Get application services list.
            var applicationServices = app.ApplicationServices;
            using (var serviceScope = applicationServices.CreateScope())
            {
                // Service provider.
                var serviceProvider = serviceScope.ServiceProvider;

                // Mongo client.
                var authenticationMongoContext = serviceProvider.GetService<IAuthenticationMongoContext>();
                var mongoClient = authenticationMongoContext.Client;

                // Start database session and transaction.
                var session = mongoClient.StartSession();
                session.StartTransaction();

                try
                {
                    // Insert identity resources.
                    var seedIdentityResourcesTask = SeedIdentityResourcesAsync(serviceProvider, resetDatabase);

                    // Insert api resources
                    var seedApiResourcesTask = SeedApiResourcesAsync(serviceProvider, resetDatabase);

                    // Insert clients.
                    var seedClientsTask = SeedClientsAsync(serviceProvider, resetDatabase);

                    // List of tasks that must be completed.
                    Task.WhenAll(seedIdentityResourcesTask, seedApiResourcesTask, seedClientsTask)
                        .Wait();

                    var hasIdentityResourceSeeded = seedIdentityResourcesTask.Result;
                    var hasApiResourceSeeded = seedApiResourcesTask.Result;
                    var hasClientSeeded = seedClientsTask.Result;

                    if (hasIdentityResourceSeeded)
                        logger.LogInformation("Identity has been seeded");

                    if (hasApiResourceSeeded)
                        logger.LogInformation("Api resource has been seeded");

                    if (hasClientSeeded)
                        logger.LogInformation("Client has been seeded");
                }
                catch (Exception exception)
                {
                    logger.LogError(exception.Message);
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///     Seed client asynchronously.
        /// </summary>
        /// <param name="applicationServices"></param>
        /// <param name="resetDatabase"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<bool> SeedClientsAsync(IServiceProvider applicationServices,
            bool resetDatabase = false,
            CancellationToken cancellationToken = default)
        {
            var authenticationMongoContext = applicationServices.GetService<IAuthenticationMongoContext>();
            var clients = authenticationMongoContext.Collections.Clients;
            var authenticationMongoDatabaseService =
                applicationServices.GetService<IAuthenticationMongoDatabaseService>();

            await clients.DeleteManyAsync(FilterDefinition<Client>.Empty, cancellationToken);

            if (authenticationMongoDatabaseService == null)
                return false;

            // No client is found.
            if (clients.CountDocuments(FilterDefinition<Client>.Empty) > 0)
                return false;

            var predefinedClients = await authenticationMongoDatabaseService.LoadClientsAsync(cancellationToken);
            if (predefinedClients == null || !predefinedClients.Any())
                return false;

            await clients.InsertManyAsync(predefinedClients, null, cancellationToken);

            return true;
        }

        /// <summary>
        ///     Seed identity resource.
        /// </summary>
        /// <returns></returns>
        internal static async Task<bool> SeedIdentityResourcesAsync(IServiceProvider applicationServices,
            bool resetDatabase = false,
            CancellationToken cancellationToken = default)
        {
            var authenticationMongoContext = applicationServices.GetService<IAuthenticationMongoContext>();
            var identityResources = authenticationMongoContext.Collections.IdentityResources;
            var authenticationMongoDatabaseService =
                applicationServices.GetService<IAuthenticationMongoDatabaseService>();

            await identityResources.DeleteManyAsync(FilterDefinition<IdentityResource>.Empty, cancellationToken);

            // Identity resource is found.
            if (identityResources.CountDocuments(FilterDefinition<IdentityResource>.Empty) > 0)
                return false;

            // List of pre-defined identity resources.
            var predefinedIdentityResources =
                await authenticationMongoDatabaseService.LoadIdentityResourcesAsync(cancellationToken);

            // List of tasks that must be done.
            var insertIdentityResourceTasks = new List<Task>();

            foreach (var predefinedIdentityResource in predefinedIdentityResources)
            {
                var addIdentityResourceTask = identityResources.InsertOneAsync(predefinedIdentityResource, null,
                    cancellationToken);

                insertIdentityResourceTasks.Add(addIdentityResourceTask);
            }


            await Task.WhenAll(insertIdentityResourceTasks.ToArray());
            return true;
        }

        /// <summary>
        ///     Seed api resource asynchronously.
        /// </summary>
        /// <returns></returns>
        internal static async Task<bool> SeedApiResourcesAsync(IServiceProvider applicationServices,
            bool resetDatabase = false,
            CancellationToken cancellationToken = default)
        {
            var authenticationMongoContext = applicationServices.GetService<IAuthenticationMongoContext>();
            var apiResources = authenticationMongoContext.Collections.ApiResources;
            var authenticationMongoDatabaseService =
                applicationServices.GetService<IAuthenticationMongoDatabaseService>();

            await apiResources.DeleteManyAsync(FilterDefinition<ApiResource>.Empty, cancellationToken);

            // No client is found.
            if (apiResources.CountDocuments(FilterDefinition<ApiResource>.Empty) > 0)
                return false;

            var predefinedApiResources =
                await authenticationMongoDatabaseService.LoadApiResourcesAsync(cancellationToken);
            if (predefinedApiResources == null || !predefinedApiResources.Any())
                return false;

            await apiResources.InsertManyAsync(predefinedApiResources, null, cancellationToken);
            return true;
        }

        #endregion
    }
}