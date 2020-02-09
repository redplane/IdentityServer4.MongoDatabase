using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using Redplane.IdentityServer4.MongoDatabase.Constants;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;
using Redplane.IdentityServer4.MongoDatabase.Models;
using Redplane.IdentityServer4.MongoDatabase.Stores;
using Redplane.IdentityServer4.MongoDatabase.UnitTest.Constants;

namespace Redplane.IdentityServer4.MongoDatabase.UnitTest.Tests.Stores.PersistedGrantStoreTests
{
    public class GetAllAsyncTests
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

            BsonClassMap.RegisterClassMap<PersistedGrant>(options =>
            {
                options.AutoMap();
                options.SetIgnoreExtraElements(true);
            });

            var persistedGrants =
                database.GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);
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

            var containerBuilder = new ContainerBuilder();

            var authenticationMongoContext = new AuthenticationMongoContext(database,
                DatabaseClientConstant.AuthenticationDatabase, AuthenticationCollectionNameConstants.Clients,
                AuthenticationCollectionNameConstants.IdentityResources,
                AuthenticationCollectionNameConstants.ApiResources,
                AuthenticationCollectionNameConstants.PersistedGrants);

            containerBuilder.RegisterInstance(authenticationMongoContext)
                .As<IAuthenticationMongoContext>()
                .SingleInstance();

            containerBuilder.Register(x => mongoClient)
                .As<IMongoClient>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PersistedGrantStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            _container = containerBuilder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            if (_mongoDbRunner != null && !_mongoDbRunner.Disposed)
                _mongoDbRunner.Dispose();
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            _container?.Dispose();
        }

        #endregion

        #region Methods

        [Test]
        public async Task GetAllAvailablePersistedGrants_Returns_AllAvailablePersistedGrants()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            var persistedGrantFilterDefinition = Builders<PersistedGrant>.Filter
                .Eq(x => x.SubjectId, "subject-2");

            var expectedPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();
            
            if (expectedPersistedGrant == null)
                throw new Exception("Please check the data source. Data must be available.");


            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync("subject-2");

            var actualPersistedGrant = actualPersistedGrants.First();
            
            Assert.NotNull(actualPersistedGrant);
            Assert.AreEqual(actualPersistedGrant.SubjectId, expectedPersistedGrant.SubjectId);
            Assert.AreEqual(actualPersistedGrant.ClientId, expectedPersistedGrant.ClientId);
            Assert.AreEqual(actualPersistedGrant.Key, expectedPersistedGrant.Key);
            Assert.AreEqual(actualPersistedGrant.Type, expectedPersistedGrant.Type);
            Assert.AreEqual(actualPersistedGrant.Expiration, expectedPersistedGrant.Expiration);
            Assert.AreEqual(actualPersistedGrant.Data, expectedPersistedGrant.Data);
        }
        
        [Test]
        public async Task GetAllNonAvailablePersistedGrants_Returns_EmptyList()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync("invalid-persisted-grant");

            Assert.IsEmpty(actualPersistedGrants);
        }

        #endregion
    }
}