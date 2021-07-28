using MFiles.VAF.Common;
using MFilesAPI;
using System;

namespace MFiles.VAF.Extensions
{
	/// <summary>
	/// A <see cref="TaskDirectiveWithDisplayName"/> that represents a single <see cref="ObjID"/>.
	/// </summary>
	public class ObjIDTaskDirective
		: TaskDirectiveWithDisplayName
	{
		/// <summary>
		/// The internal ID of the object (unique within one object type).
		/// </summary>
		public int ObjectID { get; set; }

		/// <summary>
		/// The ID of the object type.
		/// </summary>
		public int ObjectTypeID { get; set; }

		/// <summary>
		/// Instantiates an <see cref="ObjIDTaskDirective"/>.
		/// </summary>
		public ObjIDTaskDirective() { }

		/// <summary>
		/// Instantiates an <see cref="ObjIDTaskDirective"/> representing <paramref name="objID"/>.
		/// </summary>
		/// <param name="objID">The object to represent.</param>
		/// <param name="displayName">The display name for this directive.</param>
		public ObjIDTaskDirective(ObjID objID, string displayName = null)
			: this()
		{
			if (null == objID)
				throw new ArgumentNullException(nameof(objID));
			this.ObjectID = objID.ID;
			this.ObjectTypeID = objID.Type;
			this.DisplayName = displayName;
		}

		/// <summary>
		/// Attempts to set <paramref name="objID"/> from
		/// <see cref="ObjectTypeID"/> and <see cref="ObjectID"/>.
		/// </summary>
		/// <param name="objID">The object ID.</param>
		/// <returns><see langword="true"/> if successful.</returns>
		public bool TryGetObjID(out ObjID objID)
		{
			// Set the ObjID instance.
			objID = new ObjID();

			// If we do not have a valid object type ID then return false.
			if (0 > this.ObjectTypeID)
				return false;

			// If we do not have a valid object ID then return false.
			if (0 >= this.ObjectID)
				return false;

			// We can't guarantee that the object exists, but it seems reasonable.
			objID.SetIDs(this.ObjectTypeID, this.ObjectID);
			return true;
		}
	}
	public static partial class ObjVerExExtensionMethods
	{
		/// <summary>
		/// Creates a <see cref="ObjIDTaskDirective"/>
		/// representing the provided <paramref name="objID"/>.
		/// </summary>
		/// <param name="objID">The object to represent.</param>
		/// <param name="displayName">The name to display for this task.</param>
		/// <returns>The task directive for the supplied object version.</returns>
		public static ObjIDTaskDirective ToObjIDTaskDirective
		(
			this ObjID objID,
			string displayName = null
		)
		{
			// Sanity.
			if (null == objID)
				throw new ArgumentNullException(nameof(objID));

			return new ObjIDTaskDirective
			(
				objID,
				displayName
			);
		}

		/// <summary>
		/// Creates a <see cref="ObjIDTaskDirective"/>
		/// representing the provided <paramref name="objVer"/>.
		/// </summary>
		/// <param name="objVer">The object version to represent.</param>
		/// <param name="displayName">The name to display for this task.</param>
		/// <returns>The task directive for the supplied object version.</returns>
		public static ObjIDTaskDirective ToObjIDTaskDirective
		(
			this ObjVer objVer,
			string displayName = null
		)
		{
			// Sanity.
			if (null == objVer)
				throw new ArgumentNullException(nameof(objVer));

			return new ObjIDTaskDirective
			(
				objVer.ObjID,
				displayName
			);
		}

		/// <summary>
		/// Creates a <see cref="ObjVerExTaskDirective"/>
		/// representing the provided <paramref name="objVerEx"/>.
		/// </summary>
		/// <param name="objVerEx">The object version to represent.</param>
		/// <param name="displayName">The name to display for this task.</param>
		/// <returns>The task directive for the supplied object version.</returns>
		public static ObjIDTaskDirective ToObjIDTaskDirective
		(
			this ObjVerEx objVerEx,
			string displayName = null
		)
		{
			// Sanity.
			if (null == objVerEx)
				throw new ArgumentNullException(nameof(objVerEx));

			// Use the other method.
			return objVerEx.ObjVer.ToObjIDTaskDirective(displayName: displayName);
		}
	}
}
