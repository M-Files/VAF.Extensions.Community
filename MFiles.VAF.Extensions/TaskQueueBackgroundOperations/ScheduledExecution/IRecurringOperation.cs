using System;

namespace MFiles.VAF.Extensions.ScheduledExecution
{
	public interface IRecurringOperation
	{
		DateTime? GetNextExecution(DateTime? after = null);
	}
}