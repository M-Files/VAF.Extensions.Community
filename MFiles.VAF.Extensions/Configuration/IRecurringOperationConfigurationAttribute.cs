// ReSharper disable once CheckNamespace

using System;

namespace MFiles.VAF.Extensions
{
	public interface IRecurringOperationConfigurationAttribute
	{
		string QueueID { get; set; }
		string TaskType { get; set; }
		Type[] ExpectedPropertyOrFieldTypes { get; }
	}
}
