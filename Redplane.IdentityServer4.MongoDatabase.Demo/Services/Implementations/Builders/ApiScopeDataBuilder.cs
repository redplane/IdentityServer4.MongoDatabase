using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Builders;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Services;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Services.Implementations.Builders
{
	public class ApiScopeDataBuilder : IAuthenticationDataBuilder
	{
		#region Properties

		private readonly IAuthenticationDatabaseService _authenticationDatabaseService;

		#endregion

		#region Constructor

		public ApiScopeDataBuilder(IAuthenticationDatabaseService authenticationDatabaseService)
		{
			_authenticationDatabaseService = authenticationDatabaseService;
		}

		#endregion

		#region Methods

		public virtual async Task BuildAsync(IAuthenticationDatabaseContext context, CancellationToken cancellationToken = default)
		{
			if (_authenticationDatabaseService == null)
				return;

			// Get the built in identity resources.
			var builtInApiScopes = await _authenticationDatabaseService
				.LoadApiScopesAsync(cancellationToken);

			if (builtInApiScopes == null || builtInApiScopes.Count < 1)
				return;

			builtInApiScopes = builtInApiScopes
				.Where(x => !string.IsNullOrWhiteSpace(x.Name))
				.ToList();

			if (builtInApiScopes.Count < 1)
				return;

			var builtInApiScopeNames = builtInApiScopes
				.Select(x => x.Name)
				.ToArray();

			// Get the identity resource collection.
			var apiScopes = context.GetApiScopes();

			// Delete all previous identity resources.
			var apiScopeFilterDefinition = Builders<ApiScope>.Filter
				.In(x => x.Name, builtInApiScopeNames);
			await apiScopes.DeleteManyAsync(apiScopeFilterDefinition, cancellationToken);

			// Insert new records.
			await apiScopes.InsertManyAsync(builtInApiScopes, null, cancellationToken);
		}

		#endregion
	}
}