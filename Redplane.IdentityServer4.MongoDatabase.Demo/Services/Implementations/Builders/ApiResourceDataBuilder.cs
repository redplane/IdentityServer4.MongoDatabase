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
	public class ApiResourceDataBuilder : IAuthenticationDataBuilder
	{
		#region Properties

		private readonly IAuthenticationDatabaseService _authenticationDatabaseService;

		#endregion

		#region Constructor

		public ApiResourceDataBuilder(IAuthenticationDatabaseService authenticationDatabaseService)
		{
			_authenticationDatabaseService = authenticationDatabaseService;
		}

		#endregion

		#region Methods

		public virtual async Task BuildAsync(IAuthenticationDatabaseContext context, CancellationToken cancellationToken = default)
		{
			if (_authenticationDatabaseService == null)
				return;

			var builtInApiResources = await _authenticationDatabaseService
				.LoadApiResourcesAsync(cancellationToken);

			if (builtInApiResources == null || builtInApiResources.Count < 1)
				return;

			// Get built in api resource names. Names are unique.
			var apiResourceNames = builtInApiResources.Select(x => x.Name).ToArray();

			// Get api resources from database.
			var apiResources = context.GetApiResources();

			// Delete the previous api resource names.
			var apiResourceFilterDefinition = Builders<ApiResource>.Filter
				.In(x => x.Name, apiResourceNames);
			await apiResources.DeleteManyAsync(apiResourceFilterDefinition, cancellationToken);

			// Insert all built in api resources into database.
			await apiResources.InsertManyAsync(builtInApiResources, null, cancellationToken);
		}

		#endregion
	}
}