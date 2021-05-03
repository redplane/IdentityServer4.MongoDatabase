using System;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities
{
	public abstract class DefaultEntity
	{
		#region Properties

		/// <summary>
		/// Id of entity.
		/// </summary>
		public Guid Id { get; private set; }

		/// <summary>
		/// Time when the entity was lastly modified.
		/// </summary>
		public virtual double? LastModifiedTime { get; set; }

		#endregion

		#region Constructor

		protected DefaultEntity(Guid id)
		{
			Id = id;
		}

		#endregion
	}
}