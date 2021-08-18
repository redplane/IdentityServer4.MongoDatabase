using System.Threading.Tasks;

namespace Redplane.IdentityServer4.MongoDatabase.Seeder.Services.Interfaces
{
    public interface IDatabaseService
    {
        #region Methods

        Task SeedAsync();

        #endregion
    }
}