using System;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities
{
    public abstract class DefaultEntity
    {
        #region Constructor

        protected DefaultEntity(Guid id)
        {
            Id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Id of entity.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Time when the entity was lastly modified.
        /// </summary>
        public virtual double? LastModifiedTime { get; set; }

        #endregion
    }
}