using System;

namespace MFiles.VAF.Extensions
{
	public interface IRecurrenceConfiguration
	{
		string ToDashboardDisplayString();
		DateTime? GetNextExecution(DateTime? after = null);
	}
}