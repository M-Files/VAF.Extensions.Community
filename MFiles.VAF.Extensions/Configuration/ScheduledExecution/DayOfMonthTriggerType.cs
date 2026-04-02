using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration.ScheduledExecution
{
	[UsesConfigurationResources]
	public enum DayOfMonthTriggerType
	{
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_MonthlyTrigger_DayType_SpecificDate))]
		SpecificDate = 0,
		[JsonConfEditor(Label = ResourceMarker.Id + nameof(Resources.Configuration.Schedule_MonthlyTrigger_DayType_VariableDate))]
		VariableDate = 1
	}
}
