using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions
{
	public enum RecurrenceType
	{
		[JsonConfEditor(Label = "$$RecurrenceType_Unknown")]
		Unknown = 0,
		[JsonConfEditor(Label = "$$RecurrenceType_Interval")]
		Interval = 1,
		[JsonConfEditor(Label = "$$RecurrenceType_Schedule")]
		Schedule = 2
	}
}