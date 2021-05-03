using IdentityServer4.Models;
using MongoDB.Driver;

namespace Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts
{
	public interface IAuthenticationDatabaseContext
	{
		#region Properties

		/// <summary>
		/// Name of context.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     Client instance.
		/// </summary>
		IMongoClient Client { get; }

		/// <summary>
		///     Context of mongo database.
		/// </summary>
		IMongoDatabase Database { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Get clients from database.
		/// </summary>
		/// <returns></returns>
		IMongoCollection<Client> GetClients();

		/// <summary>
		/// Get persisted grants from database.
		/// </summary>
		/// <returns></returns>
		IMongoCollection<PersistedGrant> GetPersistedGrants();

		/// <summary>
		/// Get api resources from database.
		/// </summary>
		/// <returns></returns>
		IMongoCollection<ApiResource> GetApiResources();

		/// <summary>
		/// Get identity resources from database.
		/// </summary>
		/// <returns></returns>
		IMongoCollection<IdentityResource> GetIdentityResources();

		/// <summary>
		/// Get api scopes from database.
		/// </summary>
		/// <returns></returns>
		IMongoCollection<ApiScope> GetApiScopes();

		#endregion
	}
}