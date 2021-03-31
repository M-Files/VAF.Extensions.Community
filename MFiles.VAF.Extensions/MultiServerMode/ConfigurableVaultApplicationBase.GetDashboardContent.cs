// ReSharper disable once CheckNamespace
using MFiles.VAF.Common.ApplicationTaskQueue;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions.MultiServerMode.ExtensionMethods;
using MFiles.VAF.MultiserverMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Extensions.Dashboard;

namespace MFiles.VAF.Extensions.MultiServerMode
{
	public abstract partial class ConfigurableVaultApplicationBase<TSecureConfiguration>
	{
		// TODO: remove commented code, just indicating usage example
		//[ShowRunCommandOnDashboardAttribute( LabelText = "Label", ButtonText = "Run", Name = "Process Stuff" )]
		//protected TaskQueueBackgroundOperation ProcessStuffBackgroundOperation { get; private set; }

		protected override AdminConfigurationManager CreateAdminConfigurationManager()
		{
			// TODO: not sure if this belongs in this file, but this is pretty critical and we need some way of enforcing that if this is overriden it inherits from BackgroundOperationAdminConfigurationManager
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

			// Do we have any background operations content?
			// TODO: clean this up, confusing with the above, this one is for running operations above is for showing schedule...might want to merge them
			//var backgroundOperationsDashboardContent = this.GetBackgroundOperationsDashboardContent();
			//if( null != backgroundOperationsDashboardContent )
			//	dashboard.AddContent( backgroundOperationsDashboardContent );

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
			if( backgroundOperationsForDashboard != null )
				return this.backgroundOperationsForDashboard;

			this.backgroundOperationsForDashboard = new List<DashboardBackgroundOperationConfiguration>();

			// TODO: there should be validation here around enforcing that the property/field is TaskQueueBackgroundOperation (no directives)
			// TODO: shouldn't be this...and may need bindingAttrs
			foreach( var propertyInfo in this.GetType().GetProperties( BindingFlags.Instance | BindingFlags.NonPublic ) )
			{
				var attr = propertyInfo.GetCustomAttribute<ShowRunCommandOnDashboardAttribute>();
				if( attr != null )
				{
					this.backgroundOperationsForDashboard.Add( new DashboardBackgroundOperationConfiguration { Attribute = attr, MemberInfo = propertyInfo, ParentObject = this } );
				}
			}

			foreach( var fieldInfo in this.GetType().GetFields() )
			{
				var attr = fieldInfo.GetCustomAttribute<ShowRunCommandOnDashboardAttribute>();
				if( attr != null )
				{
					this.backgroundOperationsForDashboard.Add( new DashboardBackgroundOperationConfiguration { Attribute = attr, MemberInfo = fieldInfo, ParentObject = this } );
				}
			}

			return this.backgroundOperationsForDashboard;
		}

		/// <summary>
		/// Returns the dashboard content showing background operations configuration.
		/// </summary>
		/// <returns>The dashboard content.  Can be null if no background operations.</returns>
		public virtual DashboardPanel GetBackgroundOperationsDashboardContent()
		{
			// Get the current background operations configuration and die if we don't have one.
			// TODO: where should this retrieve from
			var backgroundOperationConfiguration = this.GetBackgroundOperationsForDashboard();
			if( null == backgroundOperationConfiguration || !backgroundOperationConfiguration.Any() )
				return null;

			// Add the panel for the background operations content.
			var panel = new DashboardPanel
			{
				Title = "Background Operations"
			};

			// Set up the list of log targets.
			var list = new DashboardList();
			int count = 0;
			foreach( var configuration in backgroundOperationConfiguration )
			{
				// Set up the basic list item contents.
				var listItem = new DashboardListItem
				{
					Title = configuration.Attribute.Name,
					InnerContent = Dashboard.DashboardHelper.CreateSimpleButtonContent( configuration.Attribute.LabelText, configuration.Attribute.ButtonText, configuration.CommandId ),
					StatusSummary = new DomainStatusSummary
					{
						Status = DomainStatus.Enabled,
						Label = "Enabled"
					}
				};
				count++;

				// Add the item to the list.
				list.Items.Add( listItem );
			}

			// If there are no background operations then highlight that.
			if( 0 == list.Items.Count )
				list.Items.Add( new DashboardListItem()
				{
					Title = "There are currently no commands configured.",
					StatusSummary = new DomainStatusSummary()
					{
						Status = DomainStatus.Undefined
					}
				} );

			// Add the list to the panel.
			panel.InnerContent = list;
			return panel;
		}
	}
}
