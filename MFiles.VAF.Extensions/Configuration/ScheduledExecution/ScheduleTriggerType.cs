using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions.ScheduledExecution

{
	public enum ScheduleTriggerType
	{
		[JsonConfEditor(Label = "$$ScheduleTriggerType_Unknown")]
		Unknown = 0,
		[JsonConfEditor(Label = "$$ScheduleTriggerType_Daily")]
		Daily = 1,
		[JsonConfEditor(Label = "$$ScheduleTriggerType_Weekly")]
		Weekly = 2,
		[JsonConfEditor(Label = "$$ScheduleTriggerType_Monthly")]
		Monthly = 3
	}
}