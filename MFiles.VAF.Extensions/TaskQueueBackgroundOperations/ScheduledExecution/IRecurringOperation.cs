using System;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	public interface IRecurringOperation
	{
		string ToDashboardDisplayString();
		DateTime? GetNextExecution(DateTime? after = null);
	}
}