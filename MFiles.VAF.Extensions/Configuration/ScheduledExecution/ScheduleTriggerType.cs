using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions.ScheduledExecution

{
	public enum ScheduleTriggerType
	{
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.ScheduleTriggerType_Unknown))]
		Unknown = 0,
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.ScheduleTriggerType_Daily))]
		Daily = 1,
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.ScheduleTriggerType_Weekly))]
		Weekly = 2,
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.ScheduleTriggerType_Monthly))]
		Monthly = 3
	}
}