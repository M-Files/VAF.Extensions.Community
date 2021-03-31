// ReSharper disable once CheckNamespace
using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.Common;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		protected override AdminConfigurationManager CreateAdminConfigurationManager()
		{
			return new BackgroundOperationAdminConfigurationManager( this, this.GetBackgroundOperationsForDashboard() );
		}

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
			foreach (var manager in this.GetTaskQueueBackgroundOperationManagers() ?? new TaskQueueBackgroundOperationManager[0])
			{
				var listItems = manager.GetDashboardContent( this.GetBackgroundOperationsForDashboard() );
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

		/// <summary>
		/// Returns <see cref="TaskQueueBackgroundOperationManager"/> instances declared on properties and fields
		/// on this instance.
		/// </summary>
		/// <returns>A collection of background operation managers.</returns>
		protected virtual IEnumerable<TaskQueueBackgroundOperationManager> GetTaskQueueBackgroundOperationManagers()
		{
			var taskQueueBackgroundOperationManagerType = typeof(TaskQueueBackgroundOperationManager);
			TaskQueueBackgroundOperationManager value = null;

			// Get all properties.
			foreach (var p in this.GetType().GetProperties(System.Reflection.BindingFlags.Instance
				 | System.Reflection.BindingFlags.FlattenHierarchy
				 | System.Reflection.BindingFlags.Public
				 | System.Reflection.BindingFlags.NonPublic)
				.Where(p => p.CanRead && taskQueueBackgroundOperationManagerType.IsAssignableFrom(p.PropertyType)))
			{
				value = null;
				try
				{
					value = p.GetValue(this) as TaskQueueBackgroundOperationManager;
				}
				catch { }
				if (null != value)
					yield return value;
			}

			// Get all fields.
			foreach (var f in this.GetType().GetFields(System.Reflection.BindingFlags.Instance
				 | System.Reflection.BindingFlags.FlattenHierarchy
				 | System.Reflection.BindingFlags.Public
				 | System.Reflection.BindingFlags.NonPublic)
				.Where(f => !f.Name.EndsWith("_BackingField")  // Ignore backing fields for properties otherwise we report twice.
				&& taskQueueBackgroundOperationManagerType.IsAssignableFrom(f.FieldType)))
			{
				value = null;
				try
				{
					value = f.GetValue(this) as TaskQueueBackgroundOperationManager;
				}
				catch { }
				if (null != value)
					yield return value;
			}
		}

		private List<DashboardBackgroundOperationConfiguration> backgroundOperationsForDashboard;

		public List<DashboardBackgroundOperationConfiguration> GetBackgroundOperationsForDashboard()
		{
			if( this.backgroundOperationsForDashboard != null )
				return this.backgroundOperationsForDashboard;

			var configurations = new List<DashboardBackgroundOperationConfiguration>();

			// TODO: the logic here for getting properties/fields is largely copied from GetTaskQueueBackgroundOperationManagers, ideally they would be consolidated in some way
			// Get all properties.
			foreach( var p in this.GetType().GetProperties( System.Reflection.BindingFlags.Instance
															| System.Reflection.BindingFlags.FlattenHierarchy
															| System.Reflection.BindingFlags.Public
															| System.Reflection.BindingFlags.NonPublic )
				.Where( p => p.CanRead ) )
			{
				try
				{
					var attr = p.GetCustomAttribute<ShowRunCommandOnDashboardAttribute>();
					if( attr != null )
					{
						if( p.PropertyType == typeof(TaskQueueBackgroundOperation) )
						{
							configurations.Add( new DashboardBackgroundOperationConfiguration { Attribute = attr, MemberInfo = p, ParentObject = this } );
						}
						else
						{
							SysUtils.ReportErrorToEventLog( $"Error creating {nameof(ShowRunCommandOnDashboardAttribute)}, invalid property type for {p.DeclaringType?.Name ?? string.Empty}.{p.Name}, should be {nameof(TaskQueueBackgroundOperation)}" );
						}
					}
				}
				catch( Exception e )
				{
					SysUtils.ReportErrorMessageToEventLog( $"Error creating {nameof(ShowRunCommandOnDashboardAttribute)}, issue resolving property for {p.DeclaringType?.Name ?? string.Empty}.{p.Name}", e );
				}
			}

			// Get all fields.
			foreach( var f in this.GetType().GetFields( System.Reflection.BindingFlags.Instance
														| System.Reflection.BindingFlags.FlattenHierarchy
														| System.Reflection.BindingFlags.Public
														| System.Reflection.BindingFlags.NonPublic )
				.Where( f => !f.Name.EndsWith( "_BackingField" ) ) ) // Ignore backing fields for properties otherwise we report twice.

			{
				try
				{
					var attr = f.GetCustomAttribute<ShowRunCommandOnDashboardAttribute>();
					if( attr != null )
					{
						if( f.FieldType == typeof(TaskQueueBackgroundOperation) )
						{
							configurations.Add( new DashboardBackgroundOperationConfiguration { Attribute = attr, MemberInfo = f, ParentObject = this } );
						}
						else
						{
							SysUtils.ReportErrorToEventLog( $"Error creating {nameof(ShowRunCommandOnDashboardAttribute)}, invalid field type for {f.DeclaringType?.Name ?? string.Empty}.{f.Name}, should be {nameof(TaskQueueBackgroundOperation)}" );
						}
					}
				}
				catch( Exception e )
				{
					SysUtils.ReportErrorMessageToEventLog( $"Error creating {nameof(ShowRunCommandOnDashboardAttribute)}, issue resolving field for {f.DeclaringType?.Name ?? string.Empty}.{f.Name}", e );
				}
			}

			this.backgroundOperationsForDashboard = configurations;
			return this.backgroundOperationsForDashboard;
		}
	}
}
