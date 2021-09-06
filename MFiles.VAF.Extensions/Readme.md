# M-Files Vault Application Framework Extensions (Community)

Please drill into the sub-folders for details on available objects/methods and their use.

## ConfigurableVaultApplicationBase<T>

This base class should be used for vault applications that use the VAF Extensions library.  This base class implements various functionality such as generating dashboards for your task processors and background operations:

![An image showing a VAF dashboard with a list of background operations and their current status](sample-dashboard.png)

### Using the TaskQueueBackgroundOperationManager

The [TaskQueueBackgroundOperationManager](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager) is a drop-in replacement for the old `BackgroundOperationManager` from previous VAF releases.  This updated manager is fully compatible with [M-Files Multi-Server Mode](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/) by utilising task queues behind the scenes.  The manager allows creation of both [background operations that recur on an interval](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager#creating-a-recurring-background-operation) and also [background operations that run on a more complex schedule](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager#creating-a-background-operation-that-runs-on-a-schedule).

*The `TaskQueueBackgroundOperationManager` approach should only be used for migrating from old-style background operations; it is recommended that new development uses the attribute-based declarative approach detailed below.*

### Using VAF 2.3 task processors

Standard VAF task queues are automatically exposed on the above dashboard, but can be customised by adding a `[ShowOnDashboard]` attribute.  In the example below the `ShowRunCommand` property is set to `true`, adding a button to the dashboard allowing the task to be run ad-hoc.

![An image showing a task queue that can be run on demand](runondemand-taskqueue.png)

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

#### Hiding queues or task processors

If you would like to hide a specific task processor (or an entire queue), then add a `[HideOnDashboard]` attribute:

```csharp
namespace sampleApplication
{
	public class VaultApplication
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{

		[TaskQueue]
		[HideOnDashboard] // Hide this queue entirely (all processors).
		public const string QueueId = "sampleApplication.VaultApplication";
		public const string ImportDataFromRemoteSystemTaskType = "ImportDataFromRemoteSystem";

		[TaskProcessor(QueueId, ImportDataFromRemoteSystemTaskType)]
		public void ImportDataFromRemoteSystem(ITaskProcessingJob<TaskDirective> job)
		{
			// TODO: Connect to the remote system and import data.
		}
	}
}
```

```csharp
namespace sampleApplication
{
	public class VaultApplication
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{

		[TaskQueue]
		public const string QueueId = "sampleApplication.VaultApplication";
		public const string ImportDataFromRemoteSystemTaskType = "ImportDataFromRemoteSystem";
		public const string ImportDataFromRemoteSystemTaskType2 = "ImportDataFromRemoteSystem2";

		[TaskProcessor(QueueId, ImportDataFromRemoteSystemTaskType)]
		[HideOnDashboard] // Hide just this processor.
		public void ImportDataFromRemoteSystem(ITaskProcessingJob<TaskDirective> job)
		{
			// TODO: Connect to the remote system and import data.
		}

		// This processor would still be shown.
		[TaskProcessor(QueueId, ImportDataFromRemoteSystemTaskType2)]
		public void ImportDataFromRemoteSystem2(ITaskProcessingJob<TaskDirective> job)
		{
			// TODO: Connect to the remote system and import data.
		}
	}
}
```

#### Automatically running task processors

Sometimes task processors should automatically be run, either on an interval (e.g. "every 10 minutes"), or on a more complex schedule (e.g. "every Saturday at 9am").  Configuration of these can be done by decorating configuration items with additional attributes:

##### On a schedule

![An image showing a task queue executing on a schedule](scheduled-taskqueue.png)

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
		// The import will run daily at 9am but can be configured via the M-Files Admin software.
		[DataMember]
		[RecurringOperationConfiguration(VaultApplication.QueueId, VaultApplication.ImportDataFromRemoteSystemTaskType)]
		public Schedule ImportDataSchedule { get; set; } = new Schedule()
		{
			Enabled = true,
			Triggers = new List<Trigger>()
			{
				new DailyTrigger()
				{
					TriggerTimes = new List<TimeSpan>()
					{
						new TimeSpan(9, 0, 0) // 9am
					}
				}
			}
		};
	}
}
```

##### After a time interval

![An image showing a task queue executing after an interval](interval-taskqueue.png)

Note: tasks set to run on an interval will automatically be scheduled to run when the vault starts, then to recur after the set interval.  To change this behaviour see [suppressing an interval-based task from running at vault startup](suppressing-an-interval-based-task-from-running-at-vault-startup).

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
		// The import will run every 10 minutes but can be changed to another interval via the M-Files Admin software.
		[DataMember]
		[RecurringOperationConfiguration
		(
			VaultApplication.QueueId,
			VaultApplication.ImportDataFromRemoteSystemTaskType,
			TypeEditor = "time"
		)]
		public TimeSpan Interval { get; set; } = new TimeSpan(0, 10, 0);
	}
}
```

###### Suppressing an interval-based task from running at vault startup

Declaring the property or field type as `TimeSpanEx` will allow the user to confirm whether the task should or should not run when the vault starts.  By default this value is true (run at vault startup, then after a time period).

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
		// The import will run every 10 minutes but can be changed to another interval via the M-Files Admin software.
		[DataMember]
		[RecurringOperationConfiguration
		(
			VaultApplication.QueueId,
			VaultApplication.ImportDataFromRemoteSystemTaskType
		)]
		public TimeSpanEx Interval { get; set; } = new TimeSpanEx()
		{
			Interval = new TimeSpan(0, 10, 0),
			// This one does not run at startup, although the user can change this in the admin configuration.
			RunOnVaultStartup = false
		};
	}
}
```

##### Allowing the administrator to change between an interval and a schedule

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
		// The import will run every 10 minutes but can be changed to another interval, or another schedule, by the administrator.
		// For example: the configuration could be altered to instead only run once a day at 4am.
		[DataMember]
		[RecurringOperationConfiguration(VaultApplication.QueueId, VaultApplication.ImportDataFromRemoteSystemTaskType)]
		public Frequency Schedule { get; set; } = new TimeSpan(0, 10, 0);
	}
}
```

##### Implementing IRecurrenceConfiguration

The `RecurringOperationConfiguration` attribute can be used with any property or field whose type implements `IRecurrenceConfiguration` (plus `TimeSpan`, but that's a special case).  If you want to create your own logic then you can create a class that implements this interface and use it in your configuration:

```csharp
[DataContract]
public class RandomRecurrenceConfiguration
	: IRecurrenceConfiguration
{
	private static Random rnd = new Random();

	[DataMember]
	[JsonConfIntegerEditor(Min = 5, Max = 1000)]
	public int MinimumMinutes { get;set; } = 5;
	
	[DataMember]
	[JsonConfIntegerEditor(Min = 5, Max = 1000)]
	public int MaximumMinutes { get;set; } = 200;
	
	[DataMember]
	public bool RunOnVaultStartup { get; } = false;
	
	/// <inheritdoc />
	public string ToDashboardDisplayString()
	{
		return $"<p>Runs randomly between {this.MinimumMinutes} and {this.MaximumMinutes} minutes after the last run time.</p>";
	}

	/// <inheritdoc />
	public DateTime? GetNextExecution(DateTime? after = null)
	{
		// Create a random interval.
		var interval = TimeSpan.FromMinutes(rnd.Next(this.MinimumMinutes, this.MaximumMinutes));

		// Return the next-run time.
		return (after ?? DateTime.UtcNow).Add(interval);
	}
}

// Use the custom logic.
[DataContract]
public class Configuration
{
	[DataMember]
	[RecurringOperationConfiguration
	(
		VaultApplication.QueueId,
		VaultApplication.UploadToRemoteSystemTaskType,
		DefaultValue = "Runs randomly throughout the day"
	)]
	public RandomRecurrenceConfiguration TaskOneSchedule { get; set; } = new RandomRecurrenceConfiguration();
}
```
