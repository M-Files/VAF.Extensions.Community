# M-Files Vault Application Framework Extensions (Community)

Please drill into the sub-folders for details on available objects/methods and their use.

## ConfigurableVaultApplicationBase<T>

This base class should be used for vault applications that use the VAF Extensions library.  This base class also additional functionality such as:

* Implements the [pattern required for broadcasting configuration changes to other servers](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Multi-Server-Mode/#configuration-changes)
* Creates and exposes a `TaskQueueBackgroundOperationManager` for use within your vault application.  This is automatically instantiated when needed, but do not call it before the `StartApplication` method otherwise it may return null.
* Automatically displays your `TaskQueueBackgroundOperation` instances (created through the above instance) in the [dashboard](https://developer.m-files.com/Frameworks/Vault-Application-Framework/Configuration/Custom-Dashboards/).  If you are rendering your own dashboard then you can create the required panel by calling `base.GetBackgroundOperationDashboardContent` and inserting it as appropriate into your dashboard.

![An image showing a sample dashboard with a list of background operations and their current status](sample-dashboard.png)

**View more details here: [TaskQueueBackgroundOperationManager](TaskQueueBackgroundOperations/TaskQueueBackgroundOperationManager)**