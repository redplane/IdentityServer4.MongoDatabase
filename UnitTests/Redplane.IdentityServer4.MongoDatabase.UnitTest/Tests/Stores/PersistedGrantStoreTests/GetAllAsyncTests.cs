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
                persistedGrant.SessionId = $"session-{i}";
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

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database
        /// Action:
        /// - Set persisted grant to null
        /// - Search for persisted grants
        /// Expected:
        /// - All grants are returned from database.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task PersistentGrantFilterIsNull_Returns_AllAvailablePersistedGrants()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            // Get the expected grants.
            var expectedPersistedGrants = await persistedGrants
                .Find(FilterDefinition<PersistedGrant>.Empty)
                .ToListAsync();

            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            // Get all actual persistent grants.
            var actualPersistedGrants = (await persistedGrantStore
                .GetAllAsync(null)).ToArray();


            // Two collection must be in the same length.
            Assert.NotNull(expectedPersistedGrants);
            Assert.AreEqual(expectedPersistedGrants.Count, actualPersistedGrants.Count());

            for (var i = 0; i < expectedPersistedGrants.Count; i++)
            {
                Assert.AreEqual(expectedPersistedGrants[i].SubjectId, actualPersistedGrants[i].SubjectId);
                Assert.AreEqual(expectedPersistedGrants[i].ClientId, actualPersistedGrants[i].ClientId);
                Assert.AreEqual(expectedPersistedGrants[i].Key, actualPersistedGrants[i].Key);
                Assert.AreEqual(expectedPersistedGrants[i].Type, actualPersistedGrants[i].Type);
                Assert.AreEqual(expectedPersistedGrants[i].Expiration, actualPersistedGrants[i].Expiration);
                Assert.AreEqual(expectedPersistedGrants[i].Data, actualPersistedGrants[i].Data);
            }
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant to search for invalid subject id.
        /// Expected:
        /// - No grant is returned.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWithInvalidSubjectId_Returns_EmptyList()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "invalid-subject-id";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            Assert.IsEmpty(actualPersistedGrants);
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant to search for invalid session id.
        /// Expected:
        /// - No grant is returned.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWithInvalidSessionId_Returns_NoItem()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "invalid-session-id";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            Assert.IsEmpty(actualPersistedGrants);
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant to search for invalid client.
        /// Expected:
        /// - No grant is returned.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWIthInvalidClientId_Returns_NotItem()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "invalid-client-id";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            Assert.IsEmpty(actualPersistedGrants);
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant to search for invalid type.
        /// Expected:
        /// - No grant is returned.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWIthInvalidType_Returns_NotItem()
        {
            var persistedGrantStore = _container
                .Resolve<IPersistedGrantStore>();

            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "invalid-type";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            Assert.IsEmpty(actualPersistedGrants);
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant to search for subject id 'subject-5'
        /// Expected:
        /// - Return one result whose information belongs to 'subject-5'
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWithSpecificSubjectId_Returns_ItemWithValid()
        {
            // Get the actual result.
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            // Get the expected grants.
            var databaseFilterDefinition = Builders<PersistedGrant>.Filter.Where(x => x.SubjectId == "subject-5");
            var expectedPersistedGrant = await persistedGrants
                .Find(databaseFilterDefinition)
                .FirstOrDefaultAsync();

            var persistedGrantStore = _container.Resolve<IPersistedGrantStore>();
            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SubjectId = "subject-5";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            // Persisted grant must be returned.
            Assert.NotNull(actualPersistedGrants);
            Assert.AreEqual(1, actualPersistedGrants.Count());

            var actualPersistedGrant = actualPersistedGrants.FirstOrDefault();
            Assert.NotNull(actualPersistedGrant);

            Assert.AreEqual(expectedPersistedGrant.SubjectId, actualPersistedGrant.SubjectId);
            Assert.AreEqual(expectedPersistedGrant.ClientId, actualPersistedGrant.ClientId);
            Assert.AreEqual(expectedPersistedGrant.Key, actualPersistedGrant.Key);
            Assert.AreEqual(expectedPersistedGrant.Type, actualPersistedGrant.Type);
            Assert.AreEqual(expectedPersistedGrant.Expiration, actualPersistedGrant.Expiration);
            Assert.AreEqual(expectedPersistedGrant.Data, actualPersistedGrant.Data);
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant filter to search for session id 7
        /// Expected:
        /// - Return 1 record whose session id is 7
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWithSpecificSessionId_Returns_ItemWithValid()
        {
            // Get the actual result.
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            // Get the expected grants.
            var databaseFilterDefinition = Builders<PersistedGrant>.Filter.Where(x => x.SessionId == "session-7");
            var expectedPersistedGrant = await persistedGrants
                .Find(databaseFilterDefinition)
                .FirstOrDefaultAsync();

            var persistedGrantStore = _container.Resolve<IPersistedGrantStore>();
            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.SessionId = "session-7";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            // Persisted grant must be returned.
            Assert.NotNull(actualPersistedGrants);
            Assert.AreEqual(1, actualPersistedGrants.Count());

            var actualPersistedGrant = actualPersistedGrants.FirstOrDefault();
            Assert.NotNull(actualPersistedGrant);

            Assert.AreEqual(expectedPersistedGrant.SubjectId, actualPersistedGrant.SubjectId);
            Assert.AreEqual(expectedPersistedGrant.ClientId, actualPersistedGrant.ClientId);
            Assert.AreEqual(expectedPersistedGrant.Key, actualPersistedGrant.Key);
            Assert.AreEqual(expectedPersistedGrant.Type, actualPersistedGrant.Type);
            Assert.AreEqual(expectedPersistedGrant.Expiration, actualPersistedGrant.Expiration);
            Assert.AreEqual(expectedPersistedGrant.Data, actualPersistedGrant.Data);
        }

        /// <summary>
        /// Pre-condition:
        /// - Items are defined in database.
        /// Action:
        /// - Set persisted grant to search for 'client-9'
        /// Expected:
        /// - Return information whose client id is 'client-9'
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetPersistedGrantWithSpecificClientId_Returns_ItemWithValid()
        {
            // Get the actual result.
            var mongoClient = _container.Resolve<IMongoClient>();
            var mongoDatabase = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var persistedGrants = mongoDatabase
                .GetCollection<PersistedGrant>(AuthenticationCollectionNameConstants.PersistedGrants);

            // Get the expected grants.
            var databaseFilterDefinition = Builders<PersistedGrant>.Filter.Where(x => x.ClientId == "client-9");
            var expectedPersistedGrant = await persistedGrants
                .Find(databaseFilterDefinition)
                .FirstOrDefaultAsync();

            var persistedGrantStore = _container.Resolve<IPersistedGrantStore>();
            var persistedGrantFilter = new PersistedGrantFilter();
            persistedGrantFilter.ClientId = "client-9";

            var actualPersistedGrants = await persistedGrantStore
                .GetAllAsync(persistedGrantFilter);

            // Persisted grant must be returned.
            Assert.NotNull(actualPersistedGrants);
            Assert.AreEqual(1, actualPersistedGrants.Count());

            var actualPersistedGrant = actualPersistedGrants.FirstOrDefault();
            Assert.NotNull(actualPersistedGrant);

            Assert.AreEqual(expectedPersistedGrant.SubjectId, actualPersistedGrant.SubjectId);
            Assert.AreEqual(expectedPersistedGrant.ClientId, actualPersistedGrant.ClientId);
            Assert.AreEqual(expectedPersistedGrant.Key, actualPersistedGrant.Key);
            Assert.AreEqual(expectedPersistedGrant.Type, actualPersistedGrant.Type);
            Assert.AreEqual(expectedPersistedGrant.Expiration, actualPersistedGrant.Expiration);
            Assert.AreEqual(expectedPersistedGrant.Data, actualPersistedGrant.Data);
        }

        #endregion
    }
}