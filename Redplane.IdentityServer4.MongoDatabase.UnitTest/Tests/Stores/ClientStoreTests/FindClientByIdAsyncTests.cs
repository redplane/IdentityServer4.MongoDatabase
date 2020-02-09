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

namespace Redplane.IdentityServer4.MongoDatabase.UnitTest.Tests.Stores.ClientStoreTests
{
    [TestFixture]
    public class FindClientByIdAsyncTests
    {
        #region Properties
        
        private MongoDbRunner _mongoDbRunner;

        private IContainer _container;
            
        #endregion
        
        #region Setup

        [SetUp]
        public void Setup()
        {
            _mongoDbRunner = MongoDbRunner.Start();
        
            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            var database = mongoClient.GetDatabase(DatabaseClientConstant.AuthenticationDatabase);
            
            BsonClassMap.RegisterClassMap<Client>(options =>
            {
                options.AutoMap();
                options.SetIgnoreExtraElements(true);
            });
            
            var clients = database.GetCollection<Client>(AuthenticationCollectionNameConstants.Clients);
            clients.InsertOne(new Client {ClientId = "client-01"});
            clients.InsertOne(new Client {ClientId = "client-02"});
            clients.InsertOne(new Client {ClientId = "client-03"});
            clients.InsertOne(new Client {ClientId = "client-04"});
            clients.InsertOne(new Client {ClientId = "client-05"});

            var containerBuilder = new ContainerBuilder();
            
            var authenticationMongoContext = new AuthenticationMongoContext(database, 
                DatabaseClientConstant.AuthenticationDatabase, AuthenticationCollectionNameConstants.Clients,
                AuthenticationCollectionNameConstants.IdentityResources,
                AuthenticationCollectionNameConstants.ApiResources,
                AuthenticationCollectionNameConstants.PersistedGrants);
            
            containerBuilder.RegisterInstance(authenticationMongoContext)
                .As<IAuthenticationMongoContext>()
                .OnActivating(x => x.ReplaceInstance(authenticationMongoContext))
                .SingleInstance();
            
            containerBuilder.RegisterType<ClientStore>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            _container = containerBuilder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            if (_mongoDbRunner != null && !_mongoDbRunner.Disposed)
                _mongoDbRunner.Dispose();
            
            _container?.Dispose();
        }
        
        #endregion
        
        #region Methods

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
        
        #endregion
    }
}