# Multi-Server Mode extension methods

## ConfigurableVaultApplicationBaseExtensionMethods

Provides multi-server mode extension methods for working with vault applications that inherit from `ConfigurableVaultApplicationBase<T>`.

### EnableConfigurationRebroadcasting

Removes the requirement for vault applications to implement a [custom broadcast processor](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes) to ensure that configuration updates are broadcast to all M-Files servers.

The calling vault application must override `GetRebroadcastQueueId` and return the queue ID returned by the call to `EnableConfigurationRebroadcasting`:

```csharp
/// <summary>
/// The cached rebroadcast queue Id.
/// </summary>
private string rebroadcastQueueId = null;

/// <inheritdoc />
public override string GetRebroadcastQueueId()
{
	if (string.IsNullOrWhiteSpace(this.rebroadcastQueueId))
		this.rebroadcastQueueId = this.EnableConfigurationRebroadcasting();
	return this.rebroadcastQueueId;
}
```

Note that this method should be called only once and the queue ID cached, as in the example above.
{:.note}

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
