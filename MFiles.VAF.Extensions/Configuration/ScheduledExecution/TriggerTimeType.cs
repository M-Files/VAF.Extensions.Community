using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	[UsesConfigurationResources]
	public enum TriggerTimeType
	{
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.TriggerTimeType_ServerTime))]
		ServerTime = 0,

		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.TriggerTimeType_Utc))]
		Utc = 1,

		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.TriggerTimeType_Custom))]
		Custom = 2
	}
}