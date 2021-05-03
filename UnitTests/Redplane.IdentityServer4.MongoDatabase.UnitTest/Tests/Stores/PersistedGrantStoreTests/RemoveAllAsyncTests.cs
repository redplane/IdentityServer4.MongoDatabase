using System;
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

namespace Redplane.IdentityServer4.MongoDatabase.UnitTest.Tests.Stores.PersistedGrantStoreTests
{
    public class RemoveAllAsyncTests
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
            {
                BsonClassMap.RegisterClassMap<PersistedGrant>(options =>
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
                DatabaseClientConstant.AuthenticationDatabase, AuthenticationCollectionNameConstants.Clients,
                AuthenticationCollectionNameConstants.IdentityResources,
                AuthenticationCollectionNameConstants.ApiResources,
                AuthenticationCollectionNameConstants.PersistedGrants, AuthenticationCollectionNameConstants.ApiScopes);

            containerBuilder.RegisterInstance(authenticationMongoContext)
                .As<IAuthenticationDatabaseContext>()
                .SingleInstance();

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
        public async Task DeletePersistedGrantWhenFilterIsNull_Expects_AllRecordsRemovedSuccessfully()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            var persistedGrantFilterDefinition = Builders<PersistedGrant>.Filter
                .And(Builders<PersistedGrant>.Filter
                .Eq(x => x.SubjectId, "subject-2"),
                    Builders<PersistedGrant>.Filter
                        .Eq(x => x.ClientId, "client-2"));

            var expectedPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            if (expectedPersistedGrant == null)
                throw new Exception("Please check the data source. Data must be available.");

            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            await persistedGrantStore
                .RemoveAllAsync(null);

            var actualPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            Assert.IsNull(actualPersistedGrant);
        }

        [Test]
        public async Task RemoveInvalidRecords_Expects_RecordStillInTheDatabase()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            var persistedGrantFilterDefinition = Builders<PersistedGrant>.Filter
                .And(Builders<PersistedGrant>.Filter
                        .Eq(x => x.SubjectId, "subject-2"),
                    Builders<PersistedGrant>.Filter
                        .Eq(x => x.ClientId, "client-2"));

            var expectedPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            if (expectedPersistedGrant == null)
                throw new Exception("Please check the data source. Data must be available.");

            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "subject-3";
            persistedGrantFilter.ClientId = "client-2";

            await persistedGrantStore
                .RemoveAllAsync(persistedGrantFilter);

            var actualPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            Assert.AreEqual(expectedPersistedGrant.SubjectId, actualPersistedGrant.SubjectId);
            Assert.AreEqual(expectedPersistedGrant.Key, actualPersistedGrant.Key);
            Assert.AreEqual(expectedPersistedGrant.Type, actualPersistedGrant.Type);
            Assert.AreEqual(expectedPersistedGrant.ClientId, actualPersistedGrant.ClientId);
            Assert.AreEqual(expectedPersistedGrant.CreationTime, actualPersistedGrant.CreationTime);
            Assert.AreEqual(expectedPersistedGrant.Expiration, actualPersistedGrant.Expiration);
            Assert.AreEqual(expectedPersistedGrant.Data, actualPersistedGrant.Data);
        }

        [Test]
        public async Task RemoveAllValidTypeRecords_Expects_RecordRemovedSuccessfully()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            var persistedGrantFilterDefinition = Builders<PersistedGrant>.Filter
                .And(Builders<PersistedGrant>.Filter
                        .Eq(x => x.SubjectId, "subject-2"),
                    Builders<PersistedGrant>.Filter
                        .Eq(x => x.ClientId, "client-2"),
                    Builders<PersistedGrant>.Filter
                        .Eq(x => x.Type, "type-2"));

            var expectedPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            if (expectedPersistedGrant == null)
                throw new Exception("Please check the data source. Data must be available.");

            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "subject-2";
            persistedGrantFilter.ClientId = "client-2";
            persistedGrantFilter.Type = "type-2";

            await persistedGrantStore
                .RemoveAllAsync(persistedGrantFilter);

            var actualPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            Assert.IsNull(actualPersistedGrant);
        }

        [Test]
        public async Task RemoveAllInvalidTypeRecords_Expects_RecordStillInDatabase()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            var persistedGrantFilterDefinition = Builders<PersistedGrant>.Filter
                .And(Builders<PersistedGrant>.Filter
                        .Eq(x => x.SubjectId, "subject-2"),
                    Builders<PersistedGrant>.Filter
                        .Eq(x => x.ClientId, "client-2"),
                    Builders<PersistedGrant>.Filter
                        .Eq(x => x.Type, "type-2"));

            var expectedPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            if (expectedPersistedGrant == null)
                throw new Exception("Please check the data source. Data must be available.");

            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "subject-2";
            persistedGrantFilter.ClientId = "client-2";
            persistedGrantFilter.Type = "type-2773";

            await persistedGrantStore
                .RemoveAllAsync(persistedGrantFilter);

            var actualPersistedGrant = await persistedGrants
                .Find(persistedGrantFilterDefinition)
                .FirstOrDefaultAsync();

            Assert.AreEqual(expectedPersistedGrant.SubjectId, actualPersistedGrant.SubjectId);
            Assert.AreEqual(expectedPersistedGrant.Key, actualPersistedGrant.Key);
            Assert.AreEqual(expectedPersistedGrant.Type, actualPersistedGrant.Type);
            Assert.AreEqual(expectedPersistedGrant.ClientId, actualPersistedGrant.ClientId);
            Assert.AreEqual(expectedPersistedGrant.CreationTime, actualPersistedGrant.CreationTime);
            Assert.AreEqual(expectedPersistedGrant.Expiration, actualPersistedGrant.Expiration);
            Assert.AreEqual(expectedPersistedGrant.Data, actualPersistedGrant.Data);
        }

        #endregion
    }
}