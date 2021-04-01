// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.MultiServerMode;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MFiles.VAF.Extensions
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{

		/// <inheritdoc />
		public override string GetDashboardContent(IConfigurationRequestContext context)
		{
			var dashboard = new StatusDashboard();
			dashboard.RefreshInterval = 30;

			// If there's some base content then add that.
			var baseContent = base.GetDashboardContent(context);
			if (false == string.IsNullOrWhiteSpace(baseContent))
				dashboard.AddContent(new DashboardCustomContent(baseContent));

			// Do we have any background operation content?
			var backgroundOperationContent = this.GetBackgroundOperationDashboardContent();
			if(null != backgroundOperationContent)
				dashboard.AddContent(backgroundOperationContent);

			// Return the dashboard.
			return dashboard.ToString();
		}

		/// <summary>
		/// Returns the dashboard content showing background operation status.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no background operation managers or background operations.</returns>
		public virtual DashboardPanel GetBackgroundOperationDashboardContent()
		{
			// Add each manager's data in turn.
			var list = new DashboardList();
			var taskQueueBackgroundOperationManagers = this.GetType().GetPropertiesAndFieldsOfType<TaskQueueBackgroundOperationManager>(this);
			foreach (var manager in taskQueueBackgroundOperationManagers)
			{
				var listItems = manager.GetDashboardContent();
				if (null == listItems)
					continue;
				list.Items.AddRange(listItems);
			}

			// Did we get anything?
			if (0 == list.Items.Count)
				list.Items.Add(new DashboardListItem()
				{
					Title = "There are no current background operations.",
					StatusSummary = new Configuration.Domain.DomainStatusSummary()
					{
						Status = VAF.Configuration.Domain.DomainStatus.Undefined
					}
				});

			// Return the panel.
			return new DashboardPanel()
			{
				Title = "Background Operations",
				InnerContent = new DashboardContentCollection
				{
					list,
					new DashboardCustomContent($"<em>Time on server: {DateTime.Now.ToLocalTime().ToString("HH:mm:ss")}</em>")
				}
			};
		}
	}
}
