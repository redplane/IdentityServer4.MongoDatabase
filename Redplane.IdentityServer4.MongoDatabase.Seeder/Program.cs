using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Redplane.IdentityServer4.Cores.Models.Entities;
using Redplane.IdentityServer4.MongoDatabase.Seeder.Services;

namespace Redplane.IdentityServer4.MongoDatabase.Seeder
{
    internal class Program
    {
        #region Properties

        private static IServiceProvider _serviceProvider;

        #endregion

        #region Main

        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            // Register mongo database.
            services.AddScoped(_ =>
            {
                var mongoClient = new MongoClient(configuration.GetConnectionString("AuthenticationDatabase"));
                return mongoClient.GetDatabase("ss-auth");
            });

            AddApiResourceCollection(services);
            AddClientCollection(services);
            AddApiScopeCollection(services);
            AddIdentityResourceCollection(services);

            // Database service registration.
            services.AddScoped<DatabaseService>();
            
            _serviceProvider = services.BuildServiceProvider();
            var databaseService = _serviceProvider.GetService<DatabaseService>();
            
            Console.WriteLine("Seeding database...");
            databaseService.SeedAsync().Wait();
            Console.WriteLine("Database has been seeded.");
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///     Register api resource collection.
        /// </summary>
        /// <param name="services"></param>
        private static void AddApiResourceCollection(IServiceCollection services)
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
        }

        /// <summary>
        ///     Register client collection.
        /// </summary>
        /// <param name="services"></param>
        private static void AddClientCollection(IServiceCollection services)
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
        }

        /// <summary>
        ///     Register api resource collection.
        /// </summary>
        /// <param name="services"></param>
        private static void AddApiScopeCollection(IServiceCollection services)
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
        }

        /// <summary>
        ///     Register api resource collection.
        /// </summary>
        /// <param name="services"></param>
        private static void AddIdentityResourceCollection(IServiceCollection services)
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
        }

        #endregion
    }
}