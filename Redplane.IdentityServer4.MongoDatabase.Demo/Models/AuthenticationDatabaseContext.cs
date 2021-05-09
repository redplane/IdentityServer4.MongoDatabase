using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Demo.Constants;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models
{
	public class AuthenticationDatabaseContext : MongoDatabase.Models.AuthenticationDatabaseContext
	{
		#region Properties

		private readonly IMongoDatabase _database;

		#endregion

		#region Constructor

		public AuthenticationDatabaseContext(string contextName, IMongoDatabase database) : base(contextName, database)
		{
			_database = database;
		}

		#endregion

		#region Methods

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public override IMongoCollection<Client> GetClients()
		{
			return _database.GetCollection<Client>(DatabaseCollectionNames.Clients);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public override IMongoCollection<PersistedGrant> GetPersistedGrants()
		{
			return _database.GetCollection<PersistedGrant>(DatabaseCollectionNames.PersistedGrants);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public override IMongoCollection<ApiResource> GetApiResources()
		{
			return _database.GetCollection<ApiResource>(DatabaseCollectionNames.ApiResources);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public override IMongoCollection<IdentityResource> GetIdentityResources()
		{
			return _database.GetCollection<IdentityResource>(DatabaseCollectionNames.IdentityResources);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		/// <returns></returns>
		public override IMongoCollection<ApiScope> GetApiScopes()
		{
			return _database.GetCollection<ApiScope>("apiScopes");
		}

		#endregion
	}
}