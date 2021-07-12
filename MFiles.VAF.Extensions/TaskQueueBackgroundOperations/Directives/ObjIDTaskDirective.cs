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
		public int ObjectID { get; set; }
		public int ObjectTypeID { get; set; }
		public ObjIDTaskDirective() { }
		public ObjIDTaskDirective(ObjID objID, string displayName = null)
			: this()
		{
			if (null == objID)
				throw new ArgumentNullException(nameof(objID));
			this.ObjectID = objID.ID;
			this.ObjectTypeID = objID.Type;
			this.DisplayName = displayName;
		}

		public bool TryGetObjID(Vault vault, out ObjID objID)
		{
			objID = new ObjID();
			objID.SetIDs(this.ObjectTypeID, this.ObjectID);
			return true;
		}

		public static implicit operator ObjIDClass(ObjIDTaskDirective input)
		{
			if (null == input)
				return null;
			var objID = new ObjIDClass();
			objID.SetIDs(input.ObjectTypeID, input.ObjectID);
			return objID;
		}

		public static implicit operator ObjIDTaskDirective(ObjIDClass input)
		{
			return new ObjIDTaskDirective(input);
		}
	}
}
