// ReSharper disable once CheckNamespace
using System;

namespace MFiles.VAF.Extensions
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ScheduledOperationConfigurationAttribute
		: RecurringOperationConfigurationAttributeBase
	{
		public ScheduledOperationConfigurationAttribute
		(
			string queueId,
			string taskType
		): base(queueId, taskType)
		{
		}
	}
}
