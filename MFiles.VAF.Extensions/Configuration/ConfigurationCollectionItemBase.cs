using MFiles.VAF.Configuration;
using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions.Configuration
{
	public abstract class ConfigurationCollectionItemBase
	{
		/// <summary>
		/// ArrayElementGuid is needed for the VAF to track items in a collection.
		/// This base class should be used where a collection of POCO configuration items
		/// are exposed. (e.g. in "public List<XXXX> MyCollection { get;set; }" the XXXX class
		/// should inherit from this class).
		/// </summary>
		/// <remarks>Hidden. Not to be edited by user.</remarks>
		[DataMember(Name = "arrayElementGuid", EmitDefaultValue = false, Order = 99)]
		[JsonConfEditor
		(
			TypeEditor = "guid",
			Hidden = true,
			IsRequired = true,
			ClearOnCopy = true
		)]
		public string ArrayElementGuid { get; set; }
			= System.Guid.NewGuid().ToString();
	}
}
