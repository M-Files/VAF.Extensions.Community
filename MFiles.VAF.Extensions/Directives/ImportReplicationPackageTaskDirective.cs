using System.Runtime.Serialization;

namespace MFiles.VAF.Extensions
{
	[DataContract]
	public class ImportReplicationPackageTaskDirective
		: TaskDirectiveWithDisplayName
	{
		[DataMember]
		public string CommandId { get; set; }
	}
}
