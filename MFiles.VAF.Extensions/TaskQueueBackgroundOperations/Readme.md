# Task queue extensions

## Extension methods

**View more details here: [ExtensionMethods](ExtensionMethods)**

## TaskQueueBackgroundOperationManager

**View more details here: [TaskQueueBackgroundOperationManager](TaskQueueBackgroundOperationManager)**

The Vault Application Framework's `BackgroundOperationManager`, and the associated concept of background operations, are depreciated when using version 2.2 and higher of the Vault Application Framework.  This is because VAF 2.2 and onwards enable support for M-Files Multi-Server Mode and try and depreciate functionality that is not directly compatible with this functionality.  VAF 1.0-style background operations will continue to function when targeting VAF 2.2, but their behaviour may be unexpected when running in Multi-Server environments.

The [replacement approach](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Recurring-Tasks/) is, instead, to use a task queue.  Your task queue would be populated with a single task representing the code you wish to execute and, if the operation is to recur, the task is set to [automatically re-queue itself upon completion](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/Recurring-Tasks/#recurring).  However, this approach requires a significant amount of boilerplate code.

The `TaskQueueBackgroundOperationManager` class wraps the above approach, allowing a method signature very similar to the typical background operation approach.

## TaskQueueBackgroundOperation

The `TaskQueueBackgroundOperation` class is returned by the `TaskQueueBackgroundOperationManager` and represents a single background operation.

**View more details on creating and running background operations are here: [TaskQueueBackgroundOperationManager](TaskQueueBackgroundOperationManager)**