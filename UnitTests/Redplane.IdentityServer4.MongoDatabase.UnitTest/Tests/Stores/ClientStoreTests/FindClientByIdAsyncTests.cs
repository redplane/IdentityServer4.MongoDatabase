using System;
using System.Threading.Tasks;
using Autofac;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NUnit.Framework;
using Redplane.IdentityServer4.MongoDatabase.Constants;
using Redplane.IdentityServer4.MongoDatabase.Stores;
using Redplane.IdentityServer4.MongoDatabase.UnitTest.Constants;

namespace Redplane.IdentityServer4.MongoDatabase.UnitTest.Tests.Stores.ClientStoreTests
{
    [TestFixture]
    public class FindClientByIdAsyncTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _mongoDbRunner = MongoDbRunner.Start();

            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);

            if (!BsonClassMap.IsClassMapRegistered(typeof(Client)))
                BsonClassMap.RegisterClassMap<Client>(options =>
                {
                    options.AutoMap();
                    options.SetIgnoreExtraElements(true);
                });

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

            _container = containerBuilder.Build();
        }

        [SetUp]
        public void Setup()
        {
            var mongoClient = _container.Resolve<IMongoClient>();
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            var clients = database.GetCollection<Client>(AuthenticationCollectionNameConstants.Clients);
            clients.DeleteMany(FilterDefinition<Client>.Empty);

            clients.InsertOne(new Client { ClientId = "client-01" });
            clients.InsertOne(new Client { ClientId = "client-02" });
            clients.InsertOne(new Client { ClientId = "client-03" });
            clients.InsertOne(new Client { ClientId = "client-04" });
            clients.InsertOne(new Client { ClientId = "client-05" });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (_mongoDbRunner != null && !_mongoDbRunner.Disposed)
                _mongoDbRunner.Dispose();

            _container?.Dispose();
        }

        private MongoDbRunner _mongoDbRunner;

        private IContainer _container;

        [Test]
        public async Task GetExistClientId_Returns_ValidClient()
        {
            var clientStore = _container
                .Resolve<IClientStore>();

            var client = await clientStore.FindClientByIdAsync("client-01");
            Assert.NotNull(client);
            Assert.AreEqual("client-01", client.ClientId);
        }

        [Test]
        public async Task GetNotExistClientId_Returns_Null()
        {
            var clientStore = _container
                .Resolve<IClientStore>();

            var client = await clientStore
                .FindClientByIdAsync("invalid-client");

            Assert.IsNull(client);
        }
    }
}