using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.HostedServices
{
    public class DummyHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}