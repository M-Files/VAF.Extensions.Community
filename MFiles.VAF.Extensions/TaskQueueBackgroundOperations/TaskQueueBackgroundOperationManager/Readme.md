# TaskQueueBackgroundOperationManager

The Vault Application Framework's `BackgroundOperationManager`, and the associated concept of background operations, are depreciated when using version 2.2 and higher of the Vault Application Framework.  This is because VAF 2.2 and onwards enable support for M-Files Multi-Server Mode and try and depreciate functionality that is not directly compatible with this functionality.  VAF 1.0-style background operations will continue to function when targeting VAF 2.2, but their behaviour may be unexpected when running in Multi-Server environments.

The [replacement approach](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Recurring-Tasks/) is, instead, to use a task queue.  Your task queue would be populated with a single task representing the code you wish to execute and, if the operation is to recur, the task is set to [automatically re-queue itself upon completion](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Recurring-Tasks/#recurring).  However, this approach requires a significant amount of boilerplate code.

The `TaskQueueBackgroundOperationManager` class wraps the above approach, allowing a method signature very similar to the typical background operation approach.

## Creating a recurring background operation

In the following sample a task queue background operation manager is instantiated and then used to schedule a lambda to be run once every 10 seconds:

```csharp
using System;
using MFiles.VAF.Common;
using MFiles.VAF.Extensions;
using MFiles.VAF;

namespace RecurringTask
{
	public class VaultApplication
		// Important - from 1.2 onwards this base class will ensure that "this.TaskQueueBackgroundOperationManager"
		// is available and populated as appropriate.  If you do not use this base class then you
		// need to declare your own TaskQueueBackgroundOperationManager and ensure it is instantiated
		// in StartApplication.
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{
		/// <inheritdoc />
		protected override void StartApplication()
		{
			try
			{
				// Create a background operation that runs once every ten seconds.
				this.TaskQueueBackgroundOperationManager.StartRecurringBackgroundOperation
				(
					"This is my background operation",
					TimeSpan.FromSeconds(10),
					(job) =>
					{
						SysUtils.ReportInfoToEventLog("Hello world");
					
						// If your background job processing takes more than a few seconds then
						// you should periodically report back its status:
						this.TaskQueueBackgroundOperationManager.TaskProcessor.UpdateTaskInfo
						(
							job,
							MFTaskState.MFTaskStateInProgress,
							"The process is ongoing...",
							false
						);

						// If you fail to do the above then the system may think that the task has
						// aborted, and start it running a second time!
					}
				);
			}
			catch(Exception e)
			{
				SysUtils.ReportErrorToEventLog("Exception starting background operations", e);
			}
		}

	}
}
```

Note that the name of the background operation **must be unique within each task queue**.

## Creating a background operation that will be run on demand

```csharp
using System;
using MFiles.VAF.Common;
using MFiles.VAF.Extensions;
using MFiles.VAF;

namespace RecurringTask
{
	public class VaultApplication
		// Important - from 1.2 onwards this base class will ensure that "this.TaskQueueBackgroundOperationManager"
		// is available and populated as appropriate.  If you do not use this base class then you
		// need to declare your own TaskQueueBackgroundOperationManager and ensure it is instantiated
		// in StartApplication.
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{
		/// <summary>
		/// The background operation that can be run on demand.
		/// </summary>
		protected TaskQueueBackgroundOperation MyBackgroundOperation { get; private set; }

		/// <inheritdoc />
		protected override void StartApplication()
		{
			try
			{
				// Create a background operation that can be run on demand.
				this.MyBackgroundOperation = this.TaskQueueBackgroundOperationManager.CreateBackgroundOperation
				(
					"My on-demand background operation",
					(job) =>
					{
						SysUtils.ReportInfoToEventLog("I have been run on demand.");
					
						// If your background job processing takes more than a few seconds then
						// you should periodically report back its status:
						this.TaskQueueBackgroundOperationManager.TaskProcessor.UpdateTaskInfo
						(
							job,
							MFTaskState.MFTaskStateInProgress,
							"The process is ongoing...",
							false
						);

						// If you fail to do the above then the system may think that the task has
						// aborted, and start it running a second time!
					}
				);
			}
			catch(Exception e)
			{
				SysUtils.ReportErrorToEventLog("Exception starting background operations", e);
			}
		}

		[StateAction("MyWorkflowState")]
		void MyWorkflowStateAction(StateEnvironment env)
		{
			this.MyBackgroundOperation.RunOnce();
		}

	}
}
```

### Allowing the user to run the background operation via the dashboard

When the dashboard is rendered it will, by default, include details on the registered background operations.  You can allow the user to manually run a task-based background operation (whether it is on-demand or already scheduled to run somehow) by adding the `[ShowRunCommandOnDashboard]` attribute to a property that references the background operation returned from `CreateBackgroundOperation`, `StartRecurringBackgroundOperation`, or `StartScheduledBackgroundOperation`.

```csharp
using System;
using MFiles.VAF.Common;
using MFiles.VAF.Extensions;
using MFiles.VAF;

namespace RecurringTask
{
	public class VaultApplication
		// Important - from 1.2 onwards this base class will ensure that "this.TaskQueueBackgroundOperationManager"
		// is available and populated as appropriate.  If you do not use this base class then you
		// need to declare your own TaskQueueBackgroundOperationManager and ensure it is instantiated
		// in StartApplication.
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{
		/// <summary>
		/// The background operation that can be run on demand.
		/// The ShowRunCommandOnDashboard attribute adds the "Run now" button next to the
		/// background operation in the list on the dashboard.  No other changes are required.
		/// </summary>
		[ShowRunCommandOnDashboard]
		protected TaskQueueBackgroundOperation MyBackgroundOperation { get; private set; }

		/// <inheritdoc />
		protected override void StartApplication()
		{
			try
			{
				// Create a background operation that can be run on demand.
				this.MyBackgroundOperation = this.TaskQueueBackgroundOperationManager.CreateBackgroundOperation
				(
					"My on-demand background operation",
					(job) =>
					{
						SysUtils.ReportInfoToEventLog("I have been run on demand.");
					
						// If your background job processing takes more than a few seconds then
						// you should periodically report back its status:
						this.TaskQueueBackgroundOperationManager.TaskProcessor.UpdateTaskInfo
						(
							job,
							MFTaskState.MFTaskStateInProgress,
							"The process is ongoing...",
							false
						);

						// If you fail to do the above then the system may think that the task has
						// aborted, and start it running a second time!
					}
				);
			}
			catch(Exception e)
			{
				SysUtils.ReportErrorToEventLog("Exception starting background operations", e);
			}
		}

		[StateAction("MyWorkflowState")]
		void MyWorkflowStateAction(StateEnvironment env)
		{
			this.MyBackgroundOperation.RunOnce();
		}

	}
}
```

### Passing custom data to the background operation

It is important to note that the server which schedules the background operation may not be the one that executes it.  Therefore you must not attempt to access vault application instance variables from within your background operation method.  You can, however, pass custom data into the background operation call by using a custom directive:

```csharp
using System;
using MFiles.VAF.Common;
using MFiles.VAF.Extensions;
using MFiles.VAF;

namespace RecurringTask
{
	public class VaultApplication
		// Important - from 1.2 onwards this base class will ensure that "this.TaskQueueBackgroundOperationManager"
		// is available and populated as appropriate.  If you do not use this base class then you
		// need to declare your own TaskQueueBackgroundOperationManager and ensure it is instantiated
		// in StartApplication.
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{
		/// <summary>
		/// The background operation that can be run on demand.
		/// </summary>
		protected TaskQueueBackgroundOperation<ObjVerExTaskQueueDirective> MyBackgroundOperation { get; private set; }

		/// <inheritdoc />
		protected override void StartApplication()
		{
			try
			{
				// Create a background operation that can be run on demand.
				this.MyBackgroundOperation
					= this.TaskQueueBackgroundOperationManager.CreateBackgroundOperation<ObjVerExTaskQueueDirective>
					(
						"My on-demand background operation",
						(job, directive) =>
						{
							var objVerEx = ObjVerEx.Parse(job.Vault, directive.ObjVerEx);
							SysUtils.ReportInfoToEventLog($"I have been run on demand for object {objVerEx.Title}");
					
							// If your background job processing takes more than a few seconds then
							// you should periodically report back its status:
							this.TaskQueueBackgroundOperationManager.TaskProcessor.UpdateTaskInfo
							(
								job,
								MFTaskState.MFTaskStateInProgress,
								"The process is ongoing...",
								false
							);

							// If you fail to do the above then the system may think that the task has
							// aborted, and start it running a second time!
						}
					);
			}
			catch(Exception e)
			{
				SysUtils.ReportErrorToEventLog("Exception starting background operations", e);
			}
		}

		[StateAction("MyWorkflowState")]
		void MyWorkflowStateAction(StateEnvironment env)
		{
			this.MyBackgroundOperation.RunOnce(directive: new ObjVerExTaskQueueDirective()
			{
				ObjVerEx = env.ObjVerEx.ToString()
			});
		}

		/// <summary>
		/// A custom implementation of <see cref="TaskQueueDirective"/>
		/// that can provide data about an object to the job processing method.
		/// </summary>
		public class ObjVerExTaskQueueDirective
			: TaskQueueDirective
		{
			/// <summary>
			/// Parse-able ObjVerEx string.
			/// </summary>
			public string ObjVerEx { get; set; }
		}

	}
}
```

Note that the name of the background operation **must be unique within each task queue**.

## Creating a background operation that runs on a schedule

The `StartBackgroundOperationOnSchedule` method can be used to create a schedule that defines when a background operation should be run.
Multiple triggers can be provided to tailor exactly how the schedule should be calculated.

```csharp
public class VaultApplication
	// Important - from 1.2 onwards this base class will ensure that "this.TaskQueueBackgroundOperationManager"
	// is available and populated as appropriate.  If you do not use this base class then you
	// need to declare your own TaskQueueBackgroundOperationManager and ensure it is instantiated
	// in StartApplication.
    : MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
    /// <inheritdoc />
    protected override void StartApplication()
    {
		try
		{
			// Create the schedule.
			var schedule = new MFiles.VAF.Extensions.ScheduledExecution.Schedule();
			schedule.Triggers.Add
			(
				// Run every day at the specified times.
				new MFiles.VAF.Extensions.ScheduledExecution.DailyTrigger()
				{
					TriggerTimes = new List<TriggerTime>
					{
						new TimeSpan(09, 00, 00), // 9am
						new TimeSpan(14, 00, 00), // 2pm
						new TimeSpan(15, 00, 00), // 3pm
					}
				}
			);

			// Create a background operation that runs according to the provided schedule.
			this.TaskQueueBackgroundOperationManager.StartScheduledBackgroundOperation
			(
				"This is my scheduled background operation",
				schedule,
				(job) =>
				{
					var nextRunString = "NO FUTURE EXECUTIONS";
					{
						var nextRun = schedule.GetNextExecution();
						if (nextRun.HasValue)
							nextRunString = nextRun.Value.ToString("O");
					}
					SysUtils.ReportInfoToEventLog($"Hello world, it is now {DateTime.UtcNow.ToString("O")} (re-scheduling for {nextRunString}).");
				}
			);
		}
		catch(Exception e)
		{
			SysUtils.ReportErrorToEventLog("Exception starting background operations", e);
		}
    }
}
```

### Exposing the schedule via configuration

The `Schedule` object can be exposed via a standard Configuration object, allowing the user to configure the required schedule.

```csharp
// Configuration.cs
[DataContract]
public class Configuration
{
	[DataMember]
	public Schedule SampleBackgroundOperationSchedule { get; set; } = new Schedule();
}
```

```csharp
// VaultApplication.cs
public class VaultApplication
	// Important - from 1.2 onwards this base class will ensure that "this.TaskQueueBackgroundOperationManager"
	// is available and populated as appropriate.  If you do not use this base class then you
	// need to declare your own TaskQueueBackgroundOperationManager and ensure it is instantiated
	// in StartApplication.
	: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
	/// <summary>
	/// The background operation that will be executed according to the configured schedule.
	/// </summary>
	protected TaskQueueBackgroundOperation ScheduledBackgroundOperation { get; private set; }

	/// <inheritdoc />
	protected override void StartApplication()
	{
		try
		{
			// Create a background operation.  This will be scheduled further down.
			this.ScheduledBackgroundOperation = this.TaskQueueBackgroundOperationManager.CreateBackgroundOperation
			(
				"This is my scheduled background operation",
				(job) =>
				{
					// This is the code which is run when the schedule is called.

					// When is it next scheduled for?
					var nextRun = this.Configuration?.SampleBackgroundOperationSchedule?.GetNextExecution();
					if(false == nextRun.HasValue)
					{
						// No future executions scheduled.
						SysUtils.ReportInfoToEventLog($"It is now {DateTime.UtcNow.ToString("O")}.  There are no future executions scheduled.")
					}
					else
					{
						// Is scheduled for nextRun.Value.
						SysUtils.ReportInfoToEventLog($"Hello world, it is now {DateTime.UtcNow.ToString("O")} (re-scheduling for {nextRun.Value.ToString("O")}).");
					}
				}
			);

			// Start the background operation, if there's a schedule.
			this.ScheduleBackgroundOperation();
		}
		catch(Exception e)
		{
			SysUtils.ReportErrorToEventLog("Exception starting background operations", e);
		}

	}
	
	/// <inheritdoc />
	protected override void OnConfigurationUpdated(IConfigurationRequestContext context, ClientOperations clientOps, Configuration oldConfiguration)
	{
		// Call any base implementation.,
		base.OnConfigurationUpdated(context, clientOps, oldConfiguration);
		
		// Start the background operation, if there's a schedule.
		this.ScheduleBackgroundOperation();
	}

	/// <summary>
	/// Stops any existing future executions,
	/// then re-schedules according to the configured schedule
	/// if appropriate.
	/// </summary>
	protected void ScheduleBackgroundOperation()
	
		// Stop any scheduled future executions.
		this.ScheduledBackgroundOperation.CancelFutureExecutions();

		// If we have a schedule then re-schedule.
		if (null != this.Configuration.SampleBackgroundOperationSchedule)
			this.ScheduledBackgroundOperation.RunOnSchedule(this.Configuration.SampleBackgroundOperationSchedule);
	}
}
```