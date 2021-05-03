﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NUnit.Framework;
using Redplane.IdentityServer4.MongoDatabase.Constants;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;
using Redplane.IdentityServer4.MongoDatabase.Models;
using Redplane.IdentityServer4.MongoDatabase.Stores;
using Redplane.IdentityServer4.MongoDatabase.UnitTest.Constants;

namespace Redplane.IdentityServer4.MongoDatabase.UnitTest.Tests.Stores.ResourceStores
{
    [TestFixture]
    public class FindApiResourcesByScopeNameAsyncTests
    {
        #region Life cycle

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _mongoDbRunner = MongoDbRunner.Start();

            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            if (!BsonClassMap.IsClassMapRegistered(typeof(ApiResource)))
            {
                BsonClassMap.RegisterClassMap<ApiResource>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);
                    options.SetIgnoreExtraElementsIsInherited(true);
                });
            }

            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Remove("IgnoreExtraElements");
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            var containerBuilder = new ContainerBuilder();

            var authenticationMongoContext = new AuthenticationMongoContext(database,
                DatabaseClientConstant.AuthenticationDatabase,
                AuthenticationCollectionNameConstants.Clients,
                AuthenticationCollectionNameConstants.IdentityResources,
                AuthenticationCollectionNameConstants.ApiResources,
                AuthenticationCollectionNameConstants.PersistedGrants, AuthenticationCollectionNameConstants.ApiScopes);

            containerBuilder
                .Register(x => mongoClient)
                .As<IMongoClient>()
                .InstancePerLifetimeScope();

            containerBuilder.Register(x => authenticationMongoContext)
                .As<IAuthenticationDatabaseContext>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ClientStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ResourceStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            _container = containerBuilder.Build();
        }

        [SetUp]
        public void Setup()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            var apiResources = database.GetCollection<ApiResource>(AuthenticationCollectionNameConstants.ApiResources);
            for (var apiResourceIndex = 0; apiResourceIndex < 10; apiResourceIndex++)
            {
                var name = $"ar-name-{apiResourceIndex}";
                var displayName = $"ar-display-name-{apiResourceIndex}";
                var userClaims = new List<string>();

                for (var userClaimId = 0; userClaimId < 10; userClaimId++)
                    userClaims.Add($"ar-uc-{userClaimId}");

                var apiScopeName = $"ars-name-1";
                var apiResource = new ApiResource(name, displayName, userClaims);
                apiResource.Scopes = new List<string> { apiScopeName };

                apiResources.InsertOne(apiResource);
            }
        }

        [TearDown]
        public void TearDown()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            var apiResources = database.GetCollection<ApiResource>(AuthenticationCollectionNameConstants.ApiResources);
            apiResources.DeleteMany(FilterDefinition<ApiResource>.Empty);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (_mongoDbRunner != null && !_mongoDbRunner.Disposed)
                _mongoDbRunner.Dispose();

            _container?.Dispose();
        }

        #endregion

        #region Properties

        private MongoDbRunner _mongoDbRunner;

        private IContainer _container;

        #endregion

        #region Methods

        [Test]
        public async Task FindApiResourceWithValidScopeName_Returns_ValidApiResources()
        {
            var resourceStore = _container.Resolve<IResourceStore>();
            var validScopes = new[] {"ars-name-1"};

            var apiResources = await resourceStore.FindApiResourcesByScopeNameAsync(validScopes);
            foreach (var apiResource in apiResources)
            {
                var hasValidScope = apiResource.Scopes.Any(apiResourceScope => validScopes.Contains(apiResourceScope));
                Assert.AreEqual(true, hasValidScope);
            }
        }

        [Test]
        public async Task FindApiResourceWithInvalidScopeName_Returns_NoItem()
        {
            var resourceStore = _container.Resolve<IResourceStore>();
            var validScopes = new[] { "invalid-ars-name-1" };

            var apiResources = await resourceStore.FindApiResourcesByScopeNameAsync(validScopes);
            Assert.NotNull(apiResources);
            Assert.AreEqual(0, apiResources.Count());
        }

        #endregion
    }
}