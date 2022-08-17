using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.JsonEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Configuration.ScheduledExecution
{
	public class TimeZoneStableValueOptionsProvider
		: IStableValueOptionsProvider
	{
		public IEnumerable<ValueOption> GetOptions(IConfigurationRequestContext context)
		{
			foreach(var timezone in TimeZoneInfo.GetSystemTimeZones())
			{
				yield return new ValueOption()
				{
					Value = timezone.Id,
					Label = timezone.DisplayName
				};
			}
		}
	}
}
