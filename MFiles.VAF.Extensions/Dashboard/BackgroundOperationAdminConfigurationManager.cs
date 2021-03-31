using System.Collections.Generic;
using System.Linq;
using MFiles.VAF.AdminConfigurations;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration.AdminConfigurations;
using MFiles.VAF.Configuration.Domain;
using MFiles.VAF.Configuration.Interfaces.Domain;
using MFilesAPI;

namespace MFiles.VAF.Extensions.Dashboard
{
	public class BackgroundOperationAdminConfigurationManager : AdminConfigurationManager
	{
		private readonly List<DashboardBackgroundOperationConfiguration> configurations;

		public BackgroundOperationAdminConfigurationManager(
			VaultApplicationBase vaultApplication,
			List<DashboardBackgroundOperationConfiguration> configurations,
			string rootNamespace = null,
			string proxyMethod = "AdminConfigurationRequest" )
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
						this.GetType().GetMethod( nameof(ProcessCall) ),
						this,
						MFVaultAccess.MFVaultAccessChangeFullControlRole,
						false ) );
			}
		}

		public string ProcessCall( EventHandlerEnvironment env )
		{
			var commandId = env.InputParams[ 0 ];
			var configuration = this.configurations.FirstOrDefault( c => c.CommandId == commandId );
			var backgroundOperation = configuration?.GetValue();
			backgroundOperation?.RunOnce();

			return string.Empty;
		}

		public override IEnumerable<IConfigurationDomainCommand> GetCommands( IConfigurationRequestContext context, DomainNodeLocation nodeLocation )
		{
			var result = base.GetCommands( context, nodeLocation ).ToList();

			foreach( var configuration in this.configurations )
			{
				result.Add( CreateConfigurationDomainCommand( configuration.CommandId, configuration.CommandId ) );
			}

			return result;
		}

		public ConfigurationDomainCommand CreateConfigurationDomainCommand(
			string commandId,
			string extMethod,
			params object[] additionalParameters
		)
		{
			var commandParams = new List<object> { extMethod };
			if( additionalParameters != null && additionalParameters.Length > 0 )
			{
				commandParams.AddRange( additionalParameters );
			}

			var extMethodCall = new VaultExtensionMethodCall
			{
				Method = $"{this.VaultApplication.GetType().Namespace}.{nameof(MFEventHandlerType.MFEventHandlerVaultExtensionMethod)}",
				Params = commandParams.ToArray()
			};
			var extMethodSrc = new VaultExtensionMethodSource { Read = extMethodCall };
			var extMethodSrcDef = new MethodSourceDefinition( extMethodSrc );

			return new ConfigurationDomainCommand
			{
				ID = commandId,
				ExtensionMethod = extMethodSrcDef
			};
		}
	}
}
