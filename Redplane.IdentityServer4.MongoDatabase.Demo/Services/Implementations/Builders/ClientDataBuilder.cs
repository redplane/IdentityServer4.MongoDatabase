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
	public class ClientDataBuilder : IAuthenticationDataBuilder
	{
		#region Properties

		private readonly IAuthenticationDatabaseService _applicationDatabaseService;

		#endregion

		#region Constructor

		public ClientDataBuilder(IAuthenticationDatabaseService applicationDatabaseService)
		{
			_applicationDatabaseService = applicationDatabaseService;
		}

		#endregion

		#region Methods

		public virtual async Task BuildAsync(IAuthenticationDatabaseContext context, CancellationToken cancellationToken = default)
		{
			// Get the built-in clients.
			var builtInClients = await _applicationDatabaseService.LoadClientsAsync(cancellationToken);
			if (builtInClients == null || builtInClients.Count < 1)
				return;

			// Get the built in client id. Client ids are unique.
			var builtInClientIds = builtInClients.Select(x => x.ClientId).ToArray();
			if (builtInClientIds.Length < 1)
				return;

			var clients = context.GetClients();

			// Delete all previous clients.
			var builtInClientFilterDefinition = Builders<Client>.Filter
				.In(x => x.ClientId, builtInClientIds);
			await clients.DeleteManyAsync(builtInClientFilterDefinition, cancellationToken);

			// Insert client into database.
			await clients.InsertManyAsync(builtInClients, null, cancellationToken);
		}

		#endregion
	}
}