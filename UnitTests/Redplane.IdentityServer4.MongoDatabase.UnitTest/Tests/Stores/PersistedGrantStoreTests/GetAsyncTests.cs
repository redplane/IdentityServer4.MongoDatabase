using System;
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

namespace Redplane.IdentityServer4.MongoDatabase.UnitTest.Tests.Stores.PersistedGrantStoreTests
{
    [TestFixture]
    public class GetAsyncTests
    {
        #region Properties

        private MongoDbRunner _mongoDbRunner;

        private IContainer _container;

        #endregion

        #region Setup

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _mongoDbRunner = MongoDbRunner.Start();

            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            if (!BsonClassMap.IsClassMapRegistered(typeof(PersistedGrant)))
                BsonClassMap.RegisterClassMap<PersistedGrant>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);
                    options.SetIgnoreExtraElementsIsInherited(true);
                });

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

            containerBuilder.Register(x => mongoClient)
                .As<IMongoClient>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PersistedGrantStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            _container = containerBuilder.Build();
        }

        [SetUp]
        public void Setup()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants =
                database.GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);
            persistedGrants.DeleteMany(FilterDefinition<PersistedGrant>.Empty);

            for (var i = 0; i < 10; i++)
            {
                var persistedGrant = new PersistedGrant();
                persistedGrant.SubjectId = $"subject-{i}";
                persistedGrant.ClientId = $"client-{i}";
                persistedGrant.Key = $"key-{i}";
                persistedGrant.Type = $"type-{i}";
                persistedGrant.Expiration = DateTime.Now;
                persistedGrant.Data = $"data-{i}";
                persistedGrants.InsertOne(persistedGrant);
            }
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            if (_mongoDbRunner != null && !_mongoDbRunner.Disposed)
                _mongoDbRunner.Dispose();

            _container?.Dispose();
        }

        #endregion

        #region Methods

        [Test]
        public async Task GetExistItemByKey_Returns_ItemWithTheSameKey()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var item = await persistedGrantStore.GetAsync("key-2");

            Assert.NotNull(item);
            Assert.AreEqual(item.Key, "key-2");
        }

        [Test]
        public async Task GetNonExistItemByKey_Returns_Null()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var item = await persistedGrantStore.GetAsync("invalid-key");

            Assert.IsNull(item);
        }

        #endregion
    }
}