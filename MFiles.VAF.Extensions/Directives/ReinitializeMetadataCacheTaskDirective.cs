using MFiles.VAF.AppTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Directives
{
	/// <summary>
	/// Used to process metadata cache reinitialization requests.
	/// </summary>
	[DataContract]
	public class ReinitializeMetadataCacheTaskDirective
		: BroadcastDirective
	{
		public const string TaskType = "extensions.ReinitializeMetadataStructure";
	}
}
