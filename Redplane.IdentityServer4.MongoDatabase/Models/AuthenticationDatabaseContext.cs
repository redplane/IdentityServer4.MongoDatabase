using System;
using System.Collections.Generic;
using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Models
{
	public abstract class AuthenticationDatabaseContext : IAuthenticationDatabaseContext
	{
		#region Constructor

		protected AuthenticationDatabaseContext(
			string contextName,
			IMongoDatabase database
		)
		{
			Name = contextName;
			Database = database;
			Client = database.Client;

		}

		#endregion

		#region Accessors

		public string Name { get; }

		public IMongoClient Client { get; }

		public IMongoDatabase Database { get; }

		#endregion

		#region Methods

		public abstract IMongoCollection<Client> GetClients();

		public abstract IMongoCollection<PersistedGrant> GetPersistedGrants();

		public abstract IMongoCollection<ApiResource> GetApiResources();

		public abstract IMongoCollection<IdentityResource> GetIdentityResources();

		public abstract IMongoCollection<ApiScope> GetApiScopes();

		#endregion
	}
}