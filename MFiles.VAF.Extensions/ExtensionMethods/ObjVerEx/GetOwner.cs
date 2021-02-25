using System;
using MFiles.VAF.Common;

namespace MFiles.VAF.Extensions
{
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Gets the "owner" of this object.
		/// </summary>
		/// <param name="objVerEx">The child/owned object.</param>
		/// <returns>The parent/owning object.</returns>
		/// <remarks>Can return null if the owner is deleted.</remarks>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="objVerEx"/> represents an object without an owner.</exception>
		public static ObjVerEx GetOwner
		(
			this ObjVerEx objVerEx
		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));

			// Load the current object's ObjType to find the owning type.
			var objType = objVerEx
				.Vault
				.ObjectTypeOperations
				.GetObjectType(objVerEx.Type);

			// Does this have an owning type?
			if (false == objType.HasOwnerType)
				throw new ArgumentException
				(
					$"{objType.NamePlural} do not have an owning object type.",
					nameof(objVerEx)
				);

			// Get the owning type.
			var owningObjType = objVerEx
				.Vault
				.ObjectTypeOperations
				.GetObjectType(objType.OwnerType);
			
			// Get the direct reference on this ObjVerEx to the owner.
			return objVerEx.GetDirectReference(owningObjType.OwnerPropertyDef);
		}

	}
}
