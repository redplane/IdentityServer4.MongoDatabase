using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Mongo2Go;
using MongoDB.Bson.Serialization;
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
    public class FindIdentityResourcesByScopeNameAsyncTests
    {
        #region Setup

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _mongoDbRunner = MongoDbRunner.Start();

            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityResource)))
            {
                BsonClassMap.RegisterClassMap<IdentityResource>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);
                });
            }

            var containerBuilder = new ContainerBuilder();

            var authenticationMongoContext = new AuthenticationMongoContext(database,
                DatabaseClientConstant.AuthenticationDatabase, AuthenticationCollectionNameConstants.Clients,
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

            var identityResources =
                database.GetCollection<IdentityResource>(AuthenticationCollectionNameConstants.IdentityResources);
            for (var i = 0; i < 10; i++)
            {
                var name = $"ir-name-{i}";
                var displayName = $"ir-display-name-{i}";
                var userClaims = new List<string>();
                for (var claimId = 0; claimId < 10; claimId++)
                    userClaims.Add($"ir-{i}-uc-{claimId}");

                var identityResource = new IdentityResource(name, displayName, userClaims);
                identityResources.InsertOne(identityResource);
            }
        }

        [TearDown]
        public void TearDown()
        {
              var mongoClient = _container.Resolve<IMongoClient>();
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            var identityResources =
                database.GetCollection<IdentityResource>(AuthenticationCollectionNameConstants.IdentityResources);

            identityResources.DeleteMany(FilterDefinition<IdentityResource>.Empty);
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

        /// <summary>
        ///     Precondition:
        ///     - Identity resources are setup.
        ///     Action:
        ///     - Find identity resources with valid scope names.
        ///     Expects:
        ///     - Identity with valid scope names returned.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task FindIdentityResourceWithValidScopeNames_Returns_ValidIdentityResources()
        {
            var validScopeNames = new[] { "ir-name-1", "ir-name-2", "ir-name-3" };
            var resourceStore = _container.Resolve<IResourceStore>();
            var identityResources = await resourceStore.FindIdentityResourcesByScopeNameAsync(validScopeNames);

            // Result must be not null.
            Assert.NotNull(identityResources);
            Assert.AreEqual(validScopeNames.Length, identityResources.Count());

            foreach (var identityResource in identityResources)
            {
                // Find the name in the pre-defined scope.
                var hasScopeDefined = validScopeNames.Any(x => x.Contains(identityResource.Name));
                Assert.IsTrue(hasScopeDefined);
            }
        }

        /// <summary>
        ///     Precondition:
        ///     - Identity resources are setup.
        ///     Action:
        ///     - Find identity resources with invalid scope names.
        ///     Expects:
        ///     - No item is returned.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task FindIdentityResourceWithInvalidScopeNames_Returns_NoIdentityResource()
        {
            var validScopeNames = new[] { "invalid-ir-name-1", "invalid-ir-name-2", "invalid-ir-name-3" };
            var resourceStore = _container.Resolve<IResourceStore>();
            var identityResources = await resourceStore.FindIdentityResourcesByScopeNameAsync(validScopeNames);

            // Result must be not null.
            Assert.AreEqual(0, identityResources.Count());
        }

        #endregion
    }
}