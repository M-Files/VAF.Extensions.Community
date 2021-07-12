using MFiles.VAF.AppTasks;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.Dashboards;
using MFilesAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions
{
	public partial class TaskManagerEx
		: TaskManager
	{
		public TaskManagerEx(string id, Vault permanentVault, IVaultTransactionRunner transactionRunner, TimeSpan? processingInterval = null, uint maxConcurrency = 16, TimeSpan? maxLockWaitTime = null, TaskExceptionSettings exceptionSettings = null)
			: base(id, permanentVault, transactionRunner, processingInterval, maxConcurrency, maxLockWaitTime, exceptionSettings)
		{
		}
	}
}
