using MFiles.VAF.Configuration;

namespace MFiles.VAF.Extensions
{
	public enum RecurrenceType
	{
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.RecurrenceType_Unknown))]
		Unknown = 0,
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.RecurrenceType_Interval))]
		Interval = 1,
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.RecurrenceType_Schedule))]
		Schedule = 2
	}
}