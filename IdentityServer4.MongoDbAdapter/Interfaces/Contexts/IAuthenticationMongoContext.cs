using MongoDB.Driver;

namespace IdentityServer4.Mongo.Interfaces.Contexts
{
    public interface IAuthenticationMongoContext
    {
        #region Properties

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