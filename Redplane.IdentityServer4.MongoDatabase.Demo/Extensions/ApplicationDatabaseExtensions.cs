using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Redplane.IdentityServer4.Cores.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Demo.Constants;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;
using Redplane.IdentityServer4.Portal.MongoDatabases.Extensions;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Extensions
{
    public static class ApplicationDatabaseExtensions
    {
        #region Methods

        public static IServiceCollection AddApplicationDatabase(this IServiceCollection services)
        {
            // Register is4 entities class map.
            IdentityServerMongoDatabaseExtensions.RegisterIdentityServerEntityClassMaps();

            // Entities registration.
            services.AddUserCollection()
                .AddApiResourceCollection()
                .AddClientCollection()
                .AddApiScopeCollection()
                .AddIdentityResourceCollection()
                .AddPersistedGrantCollection();

            return services;
        }

        #endregion

        #region Internal methods

        private static IServiceCollection AddUserCollection(this IServiceCollection services)
        {
            // Class map registration.
            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
                BsonClassMap.RegisterClassMap<User>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);
                    options.MapCreator(x => new User(x.Id, x.Username));
                    options.SetIgnoreExtraElementsIsInherited(true);
                });

            services.AddScoped(options =>
            {
                var dbClient = options.GetService<IMongoDatabase>();
                var users = dbClient!.GetCollection<User>(DatabaseCollectionNames.Users);

                var userIndexesBuilder = Builders<User>.IndexKeys;
                var uniqueIndexOptions = new CreateIndexOptions();
                uniqueIndexOptions.Unique = true;
                var emailIndex = new CreateIndexModel<User>(userIndexesBuilder.Ascending(user => user.Username),
                    uniqueIndexOptions);
                users
                    .Indexes
                    .CreateOne(emailIndex);

                return users;
            });

            return services;
        }

        /// <summary>
        ///     Register api resource collection.
        /// </summary>
        /// <param name="services"></param>
        private static IServiceCollection AddApiResourceCollection(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var database = provider.GetService<IMongoDatabase>();
                var apiResources = database!.GetCollection<ApplicationApiResource>("ApiResources");

                var indexBuilder = Builders<ApplicationApiResource>.IndexKeys;
                var uniqueIndex = indexBuilder.Ascending(x => x.Name);
                apiResources.Indexes.CreateOne(
                    new CreateIndexModel<ApplicationApiResource>(uniqueIndex,
                        new CreateIndexOptions { Unique = true }));

                return apiResources;
            });

            return services;
        }

        /// <summary>
        ///     Register client collection.
        /// </summary>
        /// <param name="services"></param>
        private static IServiceCollection AddClientCollection(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var database = provider.GetService<IMongoDatabase>();
                var clients = database!.GetCollection<ApplicationClient>("Clients");

                var indexBuilder = Builders<ApplicationClient>.IndexKeys;
                var uniqueIndex = indexBuilder.Ascending(x => x.ClientId);
                clients.Indexes.CreateOne(
                    new CreateIndexModel<ApplicationClient>(uniqueIndex, new CreateIndexOptions { Unique = true }));

                return clients;
            });

            return services;
        }

        /// <summary>
        ///     Register api resource collection.
        /// </summary>
        /// <param name="services"></param>
        private static IServiceCollection AddApiScopeCollection(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var database = provider.GetService<IMongoDatabase>();
                var clients = database!.GetCollection<ApplicationApiScope>("ApiScopes");
                var indexBuilder = Builders<ApplicationApiScope>.IndexKeys;
                var uniqueIndex = indexBuilder.Ascending(x => x.Name);
                clients.Indexes.CreateOne(
                    new CreateIndexModel<ApplicationApiScope>(uniqueIndex, new CreateIndexOptions { Unique = true }));
                return clients;
            });

            return services;
        }

        /// <summary>
        ///     Register api resource collection.
        /// </summary>
        /// <param name="services"></param>
        private static IServiceCollection AddIdentityResourceCollection(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var database = provider.GetService<IMongoDatabase>();
                var identityResources = database!.GetCollection<ApplicationIdentityResource>("IdentityResources");

                var indexBuilder = Builders<ApplicationIdentityResource>.IndexKeys;
                var uniqueIndex = indexBuilder.Ascending(x => x.Name);
                identityResources.Indexes.CreateOne(
                    new CreateIndexModel<ApplicationIdentityResource>(uniqueIndex,
                        new CreateIndexOptions { Unique = true }));

                return identityResources;
            });

            return services;
        }

        private static IServiceCollection AddPersistedGrantCollection(this IServiceCollection services)
        {
            services.AddScoped(provider =>
            {
                var database = provider.GetService<IMongoDatabase>();
                var persistedGrants = database!.GetCollection<PersistedGrant>("PersistedGrants");
                // var indexBuilder = Builders<PersistedGrant>.IndexKeys;
                // var uniqueIndex = indexBuilder.Ascending(x => x.Key);
                // persistedGrants.Indexes.CreateOne(
                //     new CreateIndexModel<PersistedGrant>(uniqueIndex,
                //         new CreateIndexOptions { Unique = true }));
                return persistedGrants;
            });

            return services;
        }

        #endregion
    }
}