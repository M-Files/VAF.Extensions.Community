// ReSharper disable once CheckNamespace

namespace MFiles.VAF.Extensions
{
	public interface IRecurringOperationConfigurationAttribute
	{
		string QueueID { get; set; }
		string TaskType { get; set; }
	}
}
