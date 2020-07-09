# TaskQueueBackgroundOperationManager

The Vault Application Framework's `BackgroundOperationManager`, and the associated concept of background operations, are depreciated when using version 2.2 and higher of the Vault Application Framework.  This is because VAF 2.2 and onwards enable support for M-Files Multi-Server Mode and try and depreciate functionality that is not directly compatible with this functionality.  VAF 1.0-style background operations will continue to function when targeting VAF 2.2, but their behaviour may be unexpected when running in Multi-Server environments.

The [replacement approach](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Recurring-Tasks/) is, instead, to use a task queue.  Your task queue would be populated with a single task representing the code you wish to execute and, if the operation is to recur, the task is set to [automatically re-queue itself upon completion](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Recurring-Tasks/#recurring).  However, this approach requires a significant amount of boilerplate code.

The `TaskQueueBackgroundOperationManager` class wraps the above approach, allowing a method signature very similar to the typical background operation approach.

## Creating a recurring background operation

In the following sample a task queue background operation manager is instantiated and then used to schedule a lambda to be run once every 10 seconds:

```csharp
using MFiles.VAF.Common;
using MFiles.VAF.Extensions.MultiServerMode;
using MFiles.VAF.MultiserverMode;

namespace RecurringTask
{
	public class VaultApplication
		: MFiles.VAF.Extensions.MultiServerMode.ConfigurableVaultApplicationBase<Configuration>
	{
		/// <summary>
		/// The task queue background operation manager for this application.
		/// </summary>
		protected TaskQueueBackgroundOperationManager TaskQueueBackgroundOperationManager { get; private set; }

		/// <inheritdoc />
		protected override void StartApplication()
		{
			// Instantiate the background operation manager.
			this.TaskQueueBackgroundOperationManager = new TaskQueueBackgroundOperationManager
			(
				this,
				this.GetType().FullName.Replace(".", "-") + "-BackgroundOperations"
			);

			// Create a background operation that runs once every ten seconds.
			this.TaskQueueBackgroundOperationManager.StartRecurringBackgroundOperation
			(
				"This is my background operation",
				TimeSpan.FromSeconds(10),
				() =>
				{
					SysUtils.ReportInfoToEventLog("Hello world");
				});
		}

	}
}
```

Note that the name of the background operation **must be unique within each task queue**.
{:.note}

## Creating a background operation that will be run on demand

```csharp
using MFiles.VAF.Common;
using MFiles.VAF.Extensions.MultiServerMode;
using MFiles.VAF.MultiserverMode;

namespace RecurringTask
{
	public class VaultApplication
		: MFiles.VAF.Extensions.MultiServerMode.ConfigurableVaultApplicationBase<Configuration>
	{
		/// <summary>
		/// The task queue background operation manager for this application.
		/// </summary>
		protected TaskQueueBackgroundOperationManager TaskQueueBackgroundOperationManager { get; private set; }

		/// <summary>
		/// The background operation that can be run on demand.
		/// </summary>
		protected TaskQueueBackgroundOperation MyBackgroundOperation { get; private set; }

		/// <inheritdoc />
		protected override void StartApplication()
		{
			// Instantiate the background operation manager.
			this.TaskQueueBackgroundOperationManager = new TaskQueueBackgroundOperationManager
			(
				this,
				this.GetType().FullName.Replace(".", "-") + "-BackgroundOperations"
			);

			// Create a background operation that can be run on demand.
			this.MyBackgroundOperation = this.TaskQueueBackgroundOperationManager.CreateBackgroundOperation
			(
				"My on-demand background operation",
				() =>
				{
					SysUtils.ReportInfoToEventLog("I have been run on demand.");
				}
			);
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
using MFiles.VAF.Common;
using MFiles.VAF.Extensions.MultiServerMode;
using MFiles.VAF.MultiserverMode;

namespace RecurringTask
{
	public class VaultApplication
		: MFiles.VAF.Extensions.MultiServerMode.ConfigurableVaultApplicationBase<Configuration>
	{

		/// <summary>
		/// The task queue background operation manager for this application.
		/// </summary>
		protected TaskQueueBackgroundOperationManager TaskQueueBackgroundOperationManager { get; private set; }

		/// <summary>
		/// The background operation that can be run on demand.
		/// </summary>
		protected TaskQueueBackgroundOperation<ObjVerExTaskQueueDirective> MyBackgroundOperation { get; private set; }

		/// <inheritdoc />
		protected override void StartApplication()
		{
			// Instantiate the background operation manager.
			this.TaskQueueBackgroundOperationManager = new TaskQueueBackgroundOperationManager
			(
				this,
				this.GetType().FullName.Replace(".", "-") + "-BackgroundOperations"
			);

			// Create a background operation that can be run on demand.
			this.MyBackgroundOperation
				= this.TaskQueueBackgroundOperationManager.CreateBackgroundOperation<ObjVerExTaskQueueDirective>
				(
					"My on-demand background operation",
					(job, directive) =>
					{
						var objVerEx = ObjVerEx.Parse(job.Vault, directive.ObjVerEx);
						SysUtils.ReportInfoToEventLog($"I have been run on demand for object {objVerEx.Title}");
					}
				);
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
{:.note}

