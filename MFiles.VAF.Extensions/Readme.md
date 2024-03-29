﻿# M-Files Vault Application Framework Extensions (Community)

Please drill into the sub-folders for details on available objects/methods and their use.

## ConfigurableVaultApplicationBase<T>

This base class should be used for vault applications that use the VAF Extensions library.  This base class implements various functionality such as generating dashboards for your task processors and background operations, and implementing the [logging framework](https://development.m-files.com/Frameworks/Logging/):

![An image showing a VAF dashboard with a list of background operations and their current status](sample-dashboard.png)

### Controlling configuration upgrades

When using the `ConfigurableVaultApplicationBase<T>` class, changes to the structure of the configuration type (T) provided can cause issues with your application starting.  For example: if a class previously defines a property as a string but is changed to be an integer, the deserialization of any held configuration will fail and the application will not start.

This library supports the ability for you to [programmatically control the upgrade process](Configuration/Upgrading) so that the application can convert any old configuration across to the new structures and continue loading.  More information is available in the [dedicated readme page](Configuration/Upgrading/Readme.md).

### Using the TaskQueueBackgroundOperationManager

The [TaskQueueBackgroundOperationManager](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager) is a drop-in replacement for the old `BackgroundOperationManager` from previous VAF releases.  This updated manager is fully compatible with [M-Files Multi-Server Mode](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/) by utilising task queues behind the scenes.  The manager allows creation of both [background operations that recur on an interval](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager#creating-a-recurring-background-operation) and also [background operations that run on a more complex schedule](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager#creating-a-background-operation-that-runs-on-a-schedule).

*The `TaskQueueBackgroundOperationManager` approach should only be used for migrating from old-style background operations; it is recommended that new development uses the attribute-based declarative approach detailed below.*

### Using VAF 2.3 task processors

Read more about VAF 2.3 task processors here: [https://developer.m-files.com/Frameworks/Vault-Application-Framework/Task-Queues/](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Task-Queues/).

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

## Logging

The VAF Extensions library also implements the boilerplate code required to integrate the [M-Files Vault Application Logging Framework](https://developer.m-files.com/Frameworks/Logging/) into your VAF applications.  For this to work your vault application class **must** inherit from `MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<T>`.  Your configuration class must also either inherit from `MFiles.VAF.Extensions.Configuration.ConfigurationBase`, or must implement `MFiles.VAF.Extensions.Configuration.IConfigurationWithLoggingConfiguration`.

To use logging from within your vault application class, simply log using the logger instance:

```
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Configuration.Logging;
namespace LoggingExample
{
	public class VaultApplication
		: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
	{
 
		[TaskQueue]
		public const string QueueId = "test.VaultApplication.myQueueId";
		public const string TaskTypeA = "taskTypeA";
 
		[TaskProcessor(QueueId, TaskTypeA, TransactionMode = TransactionMode.Full)]
		[ShowOnDashboard("Import data", ShowRunCommand = true)]
		public void ProcessBackgroundTask(ITaskProcessingJob<TaskDirective> job)
		{
			// Log "hello world" to appropriate targets
			this.Logger?.Info("hello world");
		}
 
	}
}
```

Where you need to log from other classes, create your own logger instance.  Each class should have its own logger.

```
using MFiles.VAF.Configuration.Logging;
namespace LoggingExample
{
	public class DocumentProcessor
	{
		private ILogger Logger { get; }
		public DocumentProcessor()
		{
			// Instantiate the logger.
			this.Logger = LogManager.GetLogger(this.GetType());
		}

		public void ProcessDocument(ObjVerEx o)
		{
			// Note the syntax for compatibility with log sensitivity filters.
			this.Logger?.Trace($"Starting processing {o}");

			try
			{
				// TODO: Implement.
			}
			catch(Exception e)
			{
				// Also log the exceptions.
				this.Logger?.Error(e, $"Exception processing {o}.");
			}
		}
 
	}
}
```

## Dashboards

The VAF Extensions library contains some default functionality to create basic [VAF dashboards](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Configuration/Custom-Dashboards/) showing information about your application, any asynchronous operations that are defined, and logging configuration.  Version 22.4 of the extensions library brings some breaking changes aimed to improve the extensibility of this concept.

### Breaking changes in VAF Extensions 22.4

#### Approach

If you wanted to customise the dashboard in previous versions of the VAF Extensions (e.g. to create your own section of content) the you needed to override `GetDashboardContent`.  The primary issue with this was that, if you wanted to include any of the standard content, you had to build the status dashboard in a specific way to ensure that the refresh functionality and the like still worked.  VAF Extensions 22.4 changes the approach needed to allow you more control.

**IT IS STRONGLY RECOMMENDED THAT YOU DO NOT OVERRIDE `GetDashboardContent` ANY MORE**

Instead, you can choose to override `GetStatusDashboardRootItems`.  This method returns the dashboard content that should be placed into the status dashboard, in the order it should be shown.  By default it will return:

1. The application overview (name, version, etc.) details from the underlying VAF implementation (provided by `GetApplicationOverviewDashboardControl`), then
2. Details on any asynchronous operations that are defined in this application (provided by `GetAsynchronousOperationDashboardControl`), then
3. Details on logging configuration (provided by `GetLoggingDashboardControl`).

To add an existing item to the end of the dashboard, you could do this:

```csharp
public override IEnumerable<IDashboardContent> GetStatusDashboardRootItems(IConfigurationRequestContext context)
{
	// Include everything we would by default.
	foreach(var c in base.GetStatusDashboardRootItems(context) ?? Enumerable.Empty<IDashboardContent>())
		yield return c;

	// Add some more content to the bottom.
	yield return this.GenerateCustomDashboardContent(context); // Not shown, but imagine it does something awesome.
}
```

To change the way in which details about your application are shown, but to keep everything else, you could override `GetApplicationOverviewDashboardContent`:

```csharp
public override IDashboardContent GetApplicationOverviewDashboardContent(IConfigurationRequestContext context)
{
	// This replaces the table that includes the application name, version, etc.
	return new DashboardCustomContent("hello world!");
}
```
Note that all of the other dashboard content is still included in the order it would be normally.

You could even remove a standard section by overriding the appropriate method and returning null:

```csharp
public override IDashboardContent GetApplicationOverviewDashboardContent(IConfigurationRequestContext context)
{
    // This would cause the application overview section to be omitted, but the rest of the dashboard rendered as normal.
    return null;
}
```

#### Method signatures

The dashboard-generation methods now get provided with the configuration request context, so a method with a signature of `IDashboardContent GetAsynchronousOperationDashboardContent()` has changed to `IDashboardContent GetAsynchronousOperationDashboardContent(IConfigurationRequestContext context)`.  If you are overriding these methods then alter the method signature to the new one.

## JsonVaultExtensionMethod

The `JsonVaultExtensionMethodAttribute` class allows developers to easily define vault extension methods that pass data back and forth as JSON strings.  The underlying mechanism used is still vault extension methods, but the developer does not need to write code to deserialize data from the input and serialize it back to the output.

An example is shown below.  In this example a vault extension method is declared with the name "MyApplication.MyVaultExtensionMethod" (**remember that vault extension methods should be unique within the vault, so use a "namespace-style" name to reduce the chances of clashes between applications**).  When the vault extension method is called it will be provided with a JSON string representing `MyInputType`.  The result from this method will be encoded as JSON, in this case to a JSON array.

The type data is inferred automatically from the method signature.  The types must be serializable/deserializable by Newtonsoft.Json (e.g. decorated with [DataContract]/[DataMember]), and the input type must not be an interface.

```csharp
[DataContract]
internal class MyInputType
{
	[DataMember]
	public string Name { get;set; }
}
[DataContract]
internal class OutputType
{
	[DataMember]
	public string Value { get;set; }
}

[JsonVaultExtensionMethod("MyApplication.MyVaultExtensionMethod")]
internal IEnumerable<MyOutputType> MyVaultExtensionMethod
(
    Vault vault,
    MyInputType input
)
{
	// TODO: Implement the method.
	throw new NotImplementedException();
}
```

It is also possible to use the attribute with a method that accepts a string input (in which case the vault extension method input will be passed verbatim):

```csharp
[JsonVaultExtensionMethod("MyApplication.MyVaultExtensionMethod")]
internal IEnumerable<MyOutputType> MyVaultExtensionMethod
(
    Vault vault,
	string input
)
{
	// TODO: Implement the method.
	throw new NotImplementedException();
}
```

The method signature must declare either one parameter (the transactional vault reference), or two parameters (the transactional vault reference, and then a value for the input).  Other method signatures are not supported.

### Controlling failures

In the case that the vault extension method fails, the system will trap the exception and return the value from `JsonVaultExtensionMethodAttribute.GetFailedOutput`.  By default this value contains no information on the exception itself.  This can be optionally included by setting `IncludeExceptionDetailsInResponse` to true.  This could be done only in debug scenarios, for example, by using the following syntax:

```csharp

[JsonVaultExtensionMethod("MyApplication.MyVaultExtensionMethod"
#if DEBUG
    , IncludeExceptionDetailsInResponse = true
#endif
    )]
internal IEnumerable<MyOutputType> MyVaultExtensionMethod
(
    Vault vault,
    MyInputType input
)
{
	// TODO: Implement the method.
	throw new NotImplementedException();
}
```

It is recommended that you use try/catch to log exceptions within the method, even if they are then re-thrown to use the default behaviour.

### Not accepting any input

It is also possible to use the attribute with a method does care about the input.  In this case any value provided to the vault extension method input is not passed to this method.

```csharp
[JsonVaultExtensionMethod("MyApplication.MyVaultExtensionMethod")]
internal MyOutputType MyVaultExtensionMethod
(
    Vault vault
)
{
	// TODO: Implement the method.
	throw new NotImplementedException();
}
```

### Not returning any value

It is also possible to use the attribute with a method does not return anything at all.  In this case the methods on `JsonVaultExtensionMethodAttribute` named `GetFailedOutput` and `GetSuccessfulOutput` will be used to define the JSON to return.

```csharp
[JsonVaultExtensionMethod("MyApplication.MyVaultExtensionMethod")]
internal void MyVaultExtensionMethod
(
    Vault vault,
	MyInputType input
)
{
	// TODO: Implement the method.
	throw new NotImplementedException();
}
```

## Defining commands for the M-Files Admin area via attributes

In general terms, [commands can be added to the M-Files Admin area](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Configuration/Commands/) by overriding `ConfigurableVaultApplicationBase<T>.GetCommands` and returning appropritately-defined instances of `CustomDomainCommand`.  The VAF Extensions adds the abilty to define these commands using attributes instead.

*Note that the method signature must be correct for these attributes to work.  The method return type must be defined as `void` and it must define two parameters, the first of type `IConfigurationRequestContext` and the second of type `ClientOperations`.*

### Buttons in the header

*The code below is equivalent to [this example](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Configuration/Commands/#displaying-commands-in-the-header) in the Developer Portal.*

```csharp
public class VaultApplication 
: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
	// Create a command with "Say hello" as the button text.
	[CustomCommand("Say hello")]
	// Add it to the header bar.
	[ButtonBarCommandLocation]
	public void SayHello
	(
		IConfigurationRequestContext context, 
		ClientOperations operations
	)
	{
		operations.ShowMessage($"Hello {context.CurrentUserSessionInfo.AccountName}");
	}
}
```

### Buttons in the domain menu

*The code below is equivalent to [this example](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Configuration/Commands/#displaying-context-menu-items-for-the-domain-menu) in the Developer Portal.*

```csharp
public class VaultApplication 
: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
	// Create a command with "Say hello" as the button text.
	[CustomCommand("Say hello")]
	// Add it to the domain context menu.
	[DomainMenuCommandLocation]
	public void SayHello
	(
		IConfigurationRequestContext context, 
		ClientOperations operations
	)
	{
		operations.ShowMessage($"Hello {context.CurrentUserSessionInfo.AccountName}");
	}
}
```

The `DomainMenuCommandLocationAttribute` allows you to additionally define other appropriate content such as the priority and icon.

### Buttons in the configuration menu

*The code below is equivalent to [this example](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Configuration/Commands/#displaying-context-menu-items-for-the-configuration-menumenu) in the Developer Portal.*

```csharp
public class VaultApplication 
: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
	// Create a command with "Say hello" as the button text.
	[CustomCommand("Say hello")]
	// Add it to the configuration context menu.
	[ConfigurationMenuCommandLocation]
	public void SayHello
	(
		IConfigurationRequestContext context, 
		ClientOperations operations
	)
	{
		operations.ShowMessage($"Hello {context.CurrentUserSessionInfo.AccountName}");
	}
}
```

The `ConfigurationMenuCommandLocationAttribute` allows you to additionally define other appropriate content such as the priority and icon.

### Defining commands and referencing them in a dashboard

It is also possible to use attributes to define a command, and then to manually render the command inside a dashboard.  To do this you must provide a static command ID when declaring the command:

```csharp
public class VaultApplication 
: MFiles.VAF.Extensions.ConfigurableVaultApplicationBase<Configuration>
{
	// Define the constant command ID.
	private const string SayHelloCommandId = "SayHello";

	// Create a command with "Say hello" as the button text and an explicit command ID.
	[CustomCommand("Say hello", CommandId = SayHelloCommandId)]
	public void SayHello
	(
		IConfigurationRequestContext context, 
		ClientOperations operations
	)
	{
		operations.ShowMessage($"Hello {context.CurrentUserSessionInfo.AccountName}");
	}

	// An example of returning the command; typically you would not
	// replace the entire dashboard with it!
	public override IEnumerable<IDashboardContent> GetStatusDashboardRootItems
	(
		IConfigurationRequestContext context
	)
	{
		// Just return the button.
		yield return this.GetCustomDomainCommandResolver()?
			.GetDashboardDomainCommand(SayHelloCommandId); // Use the explicit command ID to find it again.
	}
}
```

## Getting the owner or default property definitions of a configured object type

It is sometimes important to be able to identify the automatically-generated "default" and "owner" property definitions for a given configured object type. These can be retrieved via the API, but boilerplate code is required everywhere. It should be easy for the developer to retrieve these properties.

The `[DefaultPropertyDef]` and `[OwnerPropertyDef]` attributes can be used to easily find these items.  In the example below the ObjectType reference can be configured, and the default and owner properties simply refer to it.  Note that these properties will be null if the object type is not correctly configured.

**Note: The default and owner property definitions do not have `[DataMember]` nor `[MFPropertyDef]` attributes as they are not expected to be configured by the vault administrator.**

```csharp
[DataContract]
public class Configuration
{
	[DataMember]
	[MFObjType]
	public MFIdentifier ObjectType { get; set; }

	[DefaultPropertyDef(nameof(ObjectType))]
	public MFIdentifier DefaultPropertyDef { get; set; }

	[OwnerPropertyDef(nameof(ObjectType))]
	public MFIdentifier OwnerPropertyDef { get; set; }

}

public class VaultApplication
	: MFiles.VAF.Core.ConfigurableVaultApplicationBase<Configuration>
{
	[VaultExtensionMethod("MyExtensionMethod")]
	public void MyExtensionMethod(EventHandlerEnvironment env)
	{
		Console.WriteLine($"Configured object type: {this.Configuration.ObjectType?.ID} (default: {this.Configuration.DefaultPropertyDef?.ID}, owner: {this.Configuration.OwnerPropertyDef?.ID})");
	}
}
```