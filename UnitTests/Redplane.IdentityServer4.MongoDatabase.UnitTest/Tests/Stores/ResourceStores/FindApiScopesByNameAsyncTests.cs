using System;
using System.Collections.Generic;
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
    public class FindApiScopesByNameAsyncTests
    {
        #region Life cycle

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _mongoDbRunner = MongoDbRunner.Start();

            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            if (!BsonClassMap.IsClassMapRegistered(typeof(ApiScope)))
            {
                BsonClassMap.RegisterClassMap<ApiScope>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);
                });
            }

            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Remove("IgnoreExtraElements");
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            var containerBuilder = new ContainerBuilder();
            containerBuilder
                .Register(provider =>
                {
                    var clients = database.GetCollection<Client>("clients");
                    var persistedGrants = database.GetCollection<PersistedGrant>("persistedGrants");
                    var identityResources = database.GetCollection<IdentityResource>("identityResources");
                    var apiResources = database.GetCollection<ApiResource>("apiResources");
                    var apiScopes = database.GetCollection<ApiScope>("apiScopes");

                    return new AuthenticationDatabaseContext(Guid.NewGuid().ToString("D"), clients, persistedGrants,
                        apiResources, identityResources, apiScopes,
                        () => mongoClient.StartSession());
                })
                .As<IAuthenticationDatabaseContext>()
                .InstancePerLifetimeScope();

            containerBuilder
                .Register(x => mongoClient)
                .As<IMongoClient>()
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

            var apiScopes = database.GetCollection<ApiScope>(AuthenticationCollectionNameConstants.ApiScopes);
            for (var apiScopeId = 0; apiScopeId < 10; apiScopeId++)
            {
                var name = $"as-name-{apiScopeId}";
                var displayName = $"as-display-name-{apiScopeId}";
                var userClaims = new List<string>();

                for (var userClaimId = 0; userClaimId < 10; userClaimId++)
                    userClaims.Add($"as-uc-{userClaimId}");

                var apiScope = new ApiScope(name, displayName, userClaims);
                apiScopes.InsertOne(apiScope);
            }
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
        public async Task FindApiScopeWithValidScopeNames_Returns_ValidApiScopes()
        {
            var validScopeNames = new[] { "as-name-1", "as-name-3" };
            var resourceStore = _container.Resolve<IResourceStore>();
            var apiScopes = await resourceStore.FindApiScopesByNameAsync(validScopeNames);

            // Result must be not null.
            Assert.NotNull(apiScopes);
            Assert.GreaterOrEqual(apiScopes.Count(), 1);

            foreach (var apiScope in apiScopes)
            {
                // Find the name in the pre-defined scope.
                var hasScopeDefined = validScopeNames.Any(x => x.Contains(apiScope.Name));
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
        public async Task FindApiScopeWithInvalidScopeNames_Returns_NoIdentityResource()
        {
            var validScopeNames = new[] { "invalid-ir-name-1", "invalid-ir-name-2", "invalid-ir-name-3" };
            var resourceStore = _container.Resolve<IResourceStore>();
            var apiScopes = await resourceStore.FindApiScopesByNameAsync(validScopeNames);

            // Result must be not null.
            Assert.AreEqual(0, apiScopes.Count());
        }

        #endregion
    }
}