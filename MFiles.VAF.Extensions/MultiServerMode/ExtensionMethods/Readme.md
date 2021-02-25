# Multi-Server Mode extension methods

## ConfigurableVaultApplicationBaseExtensionMethods

Provides multi-server mode extension methods for working with vault applications that inherit from `ConfigurableVaultApplicationBase<T>`.

### EnableConfigurationRebroadcasting

Removes the requirement for vault applications to implement a [custom broadcast processor](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes) to ensure that configuration updates are broadcast to all M-Files servers.

The calling vault application must override `GetRebroadcastQueueId` and return the queue ID returned by the call to `EnableConfigurationRebroadcasting`:

```csharp
/// <summary>
/// The rebroadcast queue Id.
/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
/// </summary>
protected string ConfigurationRebroadcastQueueId { get; private set; }

/// <summary>
/// The rebroadcast queue processor.
/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
/// </summary>
protected AppTaskBatchProcessor ConfigurationRebroadcastTaskProcessor { get; private set; }

/// <inheritdoc />
public override string GetRebroadcastQueueId()
{
	// If we do not have a rebroadcast queue for the configuration data
	// then create one.
	if (null == this.ConfigurationRebroadcastTaskProcessor)
	{
		// Enable the configuration rebroadcasting.
		this.EnableConfigurationRebroadcasting
			(
			out AppTaskBatchProcessor processor,
			out string queueId
			);

		// Populate references to the task processor and queue Id.
		this.ConfigurationRebroadcastQueueId = queueId;
		this.ConfigurationRebroadcastTaskProcessor = processor;
	}

	// Return the broadcast queue Id.
	return this.ConfigurationRebroadcastQueueId;
}
```

Note that this method should be called only once and the queue ID cached, as in the example above.

Where a vault application needs to react to its own broadcast tasks, a collection of task types and task handlers can be provided and the same queue utilised.  This is more efficient than using multiple broadcast queues.

```csharp
/// <summary>
/// The rebroadcast queue Id.
/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
/// </summary>
protected string ConfigurationRebroadcastQueueId { get; private set; }

/// <summary>
/// The rebroadcast queue processor.
/// Populated during the first call to <see cref="GetRebroadcastQueueId"/>.
/// </summary>
protected AppTaskBatchProcessor ConfigurationRebroadcastTaskProcessor { get; private set; }

/// <inheritdoc />
public override string GetRebroadcastQueueId()
{
	// If we do not have a rebroadcast queue for the configuration data
	// then create one.
	if (null == this.ConfigurationRebroadcastTaskProcessor)
	{
		// Set up the other task handlers that this queue should process.
		var taskHandlers = new Dictionary<string, TaskProcessorJobHandler>();
		taskHandlers.Add( "myTaskTypeId", this.TaskTypeHandler );

		// Enable the configuration rebroadcasting.
		this.EnableConfigurationRebroadcasting
			(
			out AppTaskBatchProcessor processor,
			out string queueId,
			taskHandlers: taskHandlers
			);

		// Populate references to the task processor and queue Id.
		this.ConfigurationRebroadcastQueueId = queueId;
		this.ConfigurationRebroadcastTaskProcessor = processor;
	}

	// Return the broadcast queue Id.
	return this.ConfigurationRebroadcastQueueId;
}
```

## TaskProcessorJobExtensionMethods

Provides methods for interacting with task processor jobs.

### GetTaskQueueDirective

Extracts the task queue directive associated with the job:

```csharp
// Get the directive as a TaskQueueDirective.
var directive = job.GetTaskQueueDirective();

// Get the directive as a custom directive type.
var customDirective = job.GetTaskQueueDirective<MyTaskQueueDirectiveType>();
```

## VaultApplicationBaseExtensionMethods

Provides multi-server mode extension methods for creating various task processors.

### CreateSequentialTaskProcessor

Utility method for creating a [sequential task processor](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Task-Queues/Sequential/) for the current vault application.

### CreateConcurrentTaskProcessor

Utility method for creating a [concurrent task processor](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Task-Queues/Concurrent/) for the current vault application.

### CreateBroadcastTaskProcessor

Utility method for creating a [broadcast task processor](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Task-Queues/Broadcast/) for the current vault application.
