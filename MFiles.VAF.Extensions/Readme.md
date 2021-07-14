# M-Files Vault Application Framework Extensions (Community)

Please drill into the sub-folders for details on available objects/methods and their use.

## ConfigurableVaultApplicationBase<T>

This base class should be used for vault applications that use the VAF Extensions library.  This base class implements various functionality such as generating dashboards for your task processors and background operations:

![An image showing a VAF dashboard with a list of background operations and their current status](sample-dashboard.png)

### Using the TaskQueueBackgroundOperationManager

The [TaskQueueBackgroundOperationManager](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager) is a drop-in replacement for the old `BackgroundOperationManager` from previous VAF releases.  This updated manager is fully compatible with [M-Files Multi-Server Mode](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/) by utilising task queues behind the scenes.  The manager allows creation of both [background operations that recur on an interval](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager#creating-a-recurring-background-operation) and also [background operations that run on a more complex schedule](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager#creating-a-background-operation-that-runs-on-a-schedule).

*The `TaskQueueBackgroundOperationManager` approach should only be used for migrating from old-style background operations; it is recommended that new development uses the attribute-based declarative approach detailed below.*

### Using VAF 2.3 task processors

Standard VAF task queues can also be easily exposed on the above dashboard by adding a `[ShowOnDashboard]` attribute.  This allows administrative users to easily "ad-hoc" schedule a task for processing.

```csharp
namespace sampleApplication
{
	public class VaultApplication
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{

		[TaskQueue]
		public const string QueueId = "sampleApplication.VaultApplication";
		public const string ImportDataFromRemoteSystemTaskType = "ImportDataFromRemoteSystem";

		[TaskProcessor(QueueId, ImportDataFromRemoteSystemTaskType)]
		[ShowOnDashboard("Import data from web service", ShowRunCommand = true)]
		public void ImportDataFromRemoteSystem(ITaskProcessingJob<TaskDirective> job)
		{
			// TODO: Connect to the remote system and import data.
		}
	}
}
```

#### Automatically running task processors

Sometimes task processors should automatically be run, either on an interval (e.g. "every 10 minutes"), or on a more complex schedule (e.g. "every Saturday at 9am").  Configuration of these can be done by decorating configuration items with additional attributes:

##### On a schedule

```csharp
namespace sampleApplication
{
	public class VaultApplication
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{

		[TaskQueue]
		public const string QueueId = "sampleApplication.VaultApplication";
		public const string ImportDataFromRemoteSystemTaskType = "ImportDataFromRemoteSystem";

		[TaskProcessor(QueueId, ImportDataFromRemoteSystemTaskType)]
		[ShowOnDashboard("Import data from web service", ShowRunCommand = true)]
		public void ImportDataFromRemoteSystem(ITaskProcessingJob<TaskDirective> job)
		{
			// TODO: Connect to the remote system and import data.
		}
	}
	[DataContract]
	public class Configuration
	{
		// The import will run daily at 3am but can be configured via the M-Files Admin software.
		[DataMember]
		[ScheduledOperationConfiguration(VaultApplication.QueueId, VaultApplication.ImportDataFromRemoteSystemTaskType)]
		public Schedule ImportDataSchedule { get; set; } = new Schedule()
		{
			Enabled = true,
			Triggers = new List<Trigger>()
			{
				new DailyTrigger()
				{
					TriggerTimes = new List<TimeSpan>()
					{
						new TimeSpan(3, 0, 0) // 3am
					}
				}
			}
		};
	}
}
```

##### After a time interval

```csharp
namespace sampleApplication
{
	public class VaultApplication
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{

		[TaskQueue]
		public const string QueueId = "sampleApplication.VaultApplication";
		public const string ImportDataFromRemoteSystemTaskType = "ImportDataFromRemoteSystem";

		[TaskProcessor(QueueId, ImportDataFromRemoteSystemTaskType)]
		[ShowOnDashboard("Import data from web service", ShowRunCommand = true)]
		public void ImportDataFromRemoteSystem(ITaskProcessingJob<TaskDirective> job)
		{
			// TODO: Connect to the remote system and import data.
		}
	}
	[DataContract]
	public class Configuration
	{
		// The import will run every 10 minutes but can be configured via the M-Files Admin software.
		[DataMember]
		[ScheduledOperationConfiguration(VaultApplication.QueueId, VaultApplication.ImportDataFromRemoteSystemTaskType)]
		public TimeSpan Interval { get; set; } = new TimeSpan(0, 10, 0);
	}
}
```
