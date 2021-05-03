using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts;

namespace Redplane.IdentityServer4.MongoDatabase.Interfaces.Builders
{
	public interface IAuthenticationDataBuilder
	{
		#region Methods

		Task BuildAsync(IAuthenticationDatabaseContext context, CancellationToken cancellationToken = default);

		#endregion
	}
}