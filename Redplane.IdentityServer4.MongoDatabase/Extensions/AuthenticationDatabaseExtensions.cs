using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Builders;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Services;
using Redplane.IdentityServer4.MongoDatabase.Models;
using Redplane.IdentityServer4.MongoDatabase.Stores;

namespace Redplane.IdentityServer4.MongoDatabase.Extensions
{
	public static class AuthenticationDatabaseExtensions
	{
		#region External methods

		/// <summary>
		///     Add integration part in mongo database.
		/// </summary>
		public static IIdentityServerBuilder AddMongoDatabaseAdapter(
			this IIdentityServerBuilder identityServerBuilder,
			Func<IServiceProvider, IAuthenticationDatabaseContext> context)
		{
			// Get services collection.
			var services = identityServerBuilder.Services;

			// Register identity stores (factories)
			services.AddScoped<IClientStore, ClientStore>();
			services.AddScoped<IResourceStore, ResourceStore>();
			services.AddScoped<IPersistedGrantStore, PersistedGrantStore>();

			// Build identity resource collection.
			BuildIdentityResourceCollection();

			// Build client collection.
			BuildClientCollection();

			// Build api collection.
			BuildApiResourceCollection();

			// Build persisted grant collection.
			BuildPersistedGrantCollection();

			// Build API Scopes collection.
			BuildApiScopeCollection();

			// Register authentication mongo database.
			services.AddScoped(context);

			return identityServerBuilder;
		}

		/// <summary>
		///     Add identity server authentication.
		/// </summary>
		/// <param name="identityServerBuilder"></param>
		public static IIdentityServerBuilder AddIdentityServerMongoDbService<T>(
			this IIdentityServerBuilder identityServerBuilder) where T : IAuthenticationDatabaseService
		{
			var services = identityServerBuilder.Services;
			services.AddScoped(typeof(IAuthenticationDatabaseService), typeof(T));
			return identityServerBuilder;
		}

		/// <summary>
		///     Use authentication database seed.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="enabled"></param>
		public static IApplicationBuilder BuildAuthenticationDatabaseRecords(this IApplicationBuilder app, bool enabled = true)
		{
			if (!enabled)
				return app;

			// Get application services list.
			var applicationServices = app.ApplicationServices;
			using (var serviceScope = applicationServices.CreateScope())
			{
				// Service provider.
				var serviceProvider = serviceScope.ServiceProvider;

				// Mongo client.
				var authenticationMongoContext = serviceProvider.GetService<IAuthenticationDatabaseContext>();

				// Get the authentication data builders.
				var authenticationDataBuilders = serviceProvider
					.GetServices<IAuthenticationDataBuilder>()
					.ToArray();

				if (!authenticationDataBuilders.Any())
					return app;

				// Start database session and transaction.
				var session = authenticationMongoContext.StartSession();
				try
				{
					// Start database transaction.
					session.StartTransaction();

					foreach (var authenticationDataBuilder in authenticationDataBuilders)
						authenticationDataBuilder.BuildAsync(authenticationMongoContext)
							.Wait();
				}
				catch
				{
					session.AbortTransaction();
					throw;
				}
			}

			return app;
		}

		#endregion

		#region Internal methods

		/// <summary>
		///     Build identity resource collection mapping.
		/// </summary>
		internal static void BuildIdentityResourceCollection()
		{
			// Class map not yet registered.
			if (BsonClassMap.IsClassMapRegistered(typeof(IdentityResource)))
				return;

			BsonClassMap.RegisterClassMap<IdentityResource>(options =>
			{
				options.AutoMap();
				options.SetIgnoreExtraElements(true);
				options.MapCreator(x => new IdentityResource(x.Name, x.DisplayName, x.UserClaims));
				options.SetIgnoreExtraElementsIsInherited(true);
			});
		}

		/// <summary>
		///     Build client collection mapping.
		/// </summary>
		internal static void BuildClientCollection()
		{
			// Class map not yet registered.
			if (BsonClassMap.IsClassMapRegistered(typeof(Client)))
				return;

			BsonClassMap.RegisterClassMap<Client>(options =>
			{
				options.AutoMap();
				options.SetIgnoreExtraElements(true);
				options.SetIgnoreExtraElementsIsInherited(true);
			});
		}

		/// <summary>
		///     Build api resource collection mapping.
		/// </summary>
		internal static void BuildApiResourceCollection()
		{
			if (BsonClassMap.IsClassMapRegistered(typeof(ApiResource)))
				return;

			// Api resource not yet registered.
			BsonClassMap.RegisterClassMap<ApiResource>(options =>
			{
				options.AutoMap();
				options.SetIgnoreExtraElements(true);
				options.MapCreator(x => new ApiResource(x.Name, x.DisplayName, x.UserClaims));
			});
		}

		/// <summary>
		///     Build persisted grant collection.
		/// </summary>
		internal static void BuildApiScopeCollection()
		{
			// Persisted gran not yet registered.
			if (BsonClassMap.IsClassMapRegistered(typeof(ApiScope)))
				return;

			BsonClassMap.RegisterClassMap<ApiScope>(options =>
			{
				options.AutoMap();
				options.SetIgnoreExtraElements(true);
				options.SetIgnoreExtraElementsIsInherited(true);
			});
		}

		/// <summary>
		///     Build persisted grant collection.
		/// </summary>
		internal static void BuildPersistedGrantCollection()
		{
			// Persisted gran not yet registered.
			if (BsonClassMap.IsClassMapRegistered(typeof(PersistedGrant)))
				return;

			BsonClassMap.RegisterClassMap<PersistedGrant>(options =>
			{
				options.AutoMap();
				options.SetIgnoreExtraElements(true);
				options.SetIgnoreExtraElementsIsInherited(true);
			});
		}

		#endregion
	}
}