using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFiles.VAF.Extensions.MultiServerMode;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Dashboard
{
	public class BackgroundOperationAdminConfigurationManager : AdminConfigurationManager
	{
		private readonly List<DashboardBackgroundOperationConfiguration> configurations;

		public BackgroundOperationAdminConfigurationManager( VaultApplicationBase vaultApplication, List<DashboardBackgroundOperationConfiguration> configurations, string rootNamespace = null, string proxyMethod = "AdminConfigurationRequest" )
			: base( vaultApplication, rootNamespace, proxyMethod )
		{
			this.configurations = configurations;

			this.RegisterVaultExtensionMethod();
		}

		public void RegisterVaultExtensionMethod()
		{
			foreach( var configuration in this.configurations )
			{
				this.VaultApplication.AddVaultExtensionMethod(
					configuration.CommandId,
					new VaultExtensionMethodInfo(
						this.GetType().GetMethod( nameof( ProcessCall ) ),
						this,
						MFVaultAccess.MFVaultAccessChangeFullControlRole,
						false ) );
			}
		}

		public string ProcessCall( EventHandlerEnvironment env )
		{
			Trace.TraceInformation( "TempDebugAdminConfigurationManager.ProcessCall + " + env.Input );

			var commandId = env.InputParams[ 0 ];
			var configuration = this.configurations.FirstOrDefault( c => c.CommandId == commandId );

			if( configuration != null )
			{
				// TODO: this likely needs work
				var propertyValue = ( (PropertyInfo) configuration.MemberInfo ).GetValue( this.VaultApplication );
				if( propertyValue is TaskQueueBackgroundOperation backgroundOperation )
				{
					backgroundOperation.RunOnce();
				}
			}

			return string.Empty;
		}

		public override IEnumerable<IConfigurationDomainCommand> GetCommands( IConfigurationRequestContext context, DomainNodeLocation nodeLocation )
		{
			var result = base.GetCommands( context, nodeLocation ).ToList();

			foreach( var configuration in this.configurations )
			{
				result.Add( this.VaultApplication.CreateConfigurationDomainCommand( configuration.CommandId, configuration.CommandId ) );
			}

			return result;
		}
	}
}
