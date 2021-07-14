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
			objID = new ObjID();
			objID.SetIDs(this.ObjectTypeID, this.ObjectID);
			return true;
		}

		/// <summary>
		/// Converts <paramref name="input"/> to an <see cref="ObjIDClass"/>.
		/// </summary>
		/// <param name="input">The item to convert.</param>
		public static implicit operator ObjIDClass(ObjIDTaskDirective input)
		{
			if (null == input)
				return null;
			var objID = new ObjIDClass();
			objID.SetIDs(input.ObjectTypeID, input.ObjectID);
			return objID;
		}

		/// <summary>
		/// Converts <paramref name="input"/> to an <see cref="ObjIDTaskDirective" />.
		/// </summary>
		/// <param name="input">The item to convert.</param>
		public static implicit operator ObjIDTaskDirective(ObjIDClass input)
		{
			if (null == input)
				return null;
			return new ObjIDTaskDirective(input);
		}
	}
}
