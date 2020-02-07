using MongoDB.Driver;

namespace Redplane.IdentityServer4.MongoDatabase.Interfaces.Contexts
{
    public interface IAuthenticationMongoContext
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

        /// <summary>
        ///     List of collections in authentication database.
        /// </summary>
        IAuthenticationMongoCollections Collections { get; }

        #endregion
    }
}